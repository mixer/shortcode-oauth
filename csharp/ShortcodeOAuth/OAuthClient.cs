// -----------------------------------------------------------------------
// <copyright file="OAuthClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// OAuthClient is a client interface for handling shortcode
    /// grants and reauthorizing access tokens.
    /// </summary>
    public class OAuthClient : IDisposable
    {
        /// <summary>
        /// How often to poll for shortcode completion if no timespan is
        /// provided.
        /// </summary>
        private static readonly TimeSpan defaultPollInterval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// OAuth options.
        /// </summary>
        private readonly OAuthOptions options;

        /// <summary>
        /// Creates a new instance of the OAuth client.
        /// </summary>
        /// <param name="options">Options to use</param>
        public OAuthClient(OAuthOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.options.Client.Dispose();
        }

        /// <summary>
        /// Attempts to retrieve a single OAuth shortcode from the server.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation for the operation</param>
        /// <returns>A task that resolves to the retrieved code.</returns>
        /// <exception cref="OAuthException">If there was an error granting
        /// access. This can occur if your client ID is incorrect, for instance,
        /// or if the user denies access. The <code>Response.Error</code>
        /// property can tell you more about what went wrong.</exception>
        /// <exception cref="UnexpectedHttpException">If some unknown
        /// error occurred.</exception>
        public async Task<OAuthShortcode> GetSingleCodeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var body = HttpHelpers.JsonContent(
                new
                {
                    scope = string.Join(" ", this.options.Scopes),
                    client_secret = this.options.ClientSecret,
                    client_id = this.options.ClientId
                });

            var response = await this.options.Client.PostAsync(
                $"{this.options.Host}/api/v1/oauth/shortcode",
                body,
                cancellationToken);

            using (response)
            {
                await HttpHelpers.ThrowInvalidContentsAsync(response);
                var parsed = JsonConvert.DeserializeObject<ShortcodeCreateResponse>(
                    await response.Content.ReadAsStringAsync());
                return new OAuthShortcode(this.options, parsed);
            }
        }

        /// <summary>
        /// Starts the authorization process. It retrieves a six-digit OAuth
        /// shortcode from the server, and then calls the <paramref name="showCodes"/>
        /// function. The caller should then display UI to the user, prompting
        /// them to enter the six-digit code on <see cref="https://mixer.com/go"/>.
        ///
        /// It continues to fire the function this until the <paramref name="cancellationToken"/>
        /// fires, or until the user grants access.
        /// </summary>
        /// <param name="showCodes">A function to show a set of OAuth codes
        /// to the user.</param>
        /// <param name="cancellationToken">Cancellation for the request</param>
        /// <param name="pollInterval">How often to poll for completion.
        /// Defaults to 5 seconds.</param>
        /// <returns>A task that resolves to the granted tokens, once
        /// the user allows access.</returns>
        /// <exception cref="OAuthException">If there was an error granting
        /// access. This can occur if your client ID is incorrect, for instance,
        /// or if the user denies access. The <code>Response.Error</code>
        /// property can tell you more about what went wrong.</exception>
        /// <exception cref="UnexpectedHttpException">If some unknown
        /// error occurred.</exception>
        public async Task<OAuthTokens> GrantAsync(
            Action<string> showCodes,
            CancellationToken cancellationToken,
            TimeSpan pollInterval = default(TimeSpan))
        {
            if (pollInterval == TimeSpan.Zero)
            {
                pollInterval = OAuthClient.defaultPollInterval;
            }

            while (true)
            {
                var code = await this.GetSingleCodeAsync(cancellationToken);
                showCodes(code.Code);

                ShortcodeState state;
                do
                {
                    state = await code.PollAsync(cancellationToken);

                    switch (state)
                    {
                        case ShortcodeState.Accepted:
                            return await code.GetTokensAsync(cancellationToken);
                        case ShortcodeState.Waiting:
                            await Task.Delay(pollInterval, cancellationToken);
                            break; // poll again
                        case ShortcodeState.Expired:
                            break; // `do` loop will exit and we'll get a new code
                        case ShortcodeState.Denied:
                            throw new OAuthException(
                                new OAuthErrorResponse
                                {
                                    Description = "The user has denied access to your application",
                                    Error = OAuthError.AccessDenied,
                                });
                    }
                } while (state != ShortcodeState.Expired);
            }
        }

        /// <summary>
        /// Refreshes the set of OAuth tokens.
        /// </summary>
        /// <param name="tokens">Previously granted tokens</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>A new set of tokens.</returns>
        /// <exception cref="OAuthException">If there was an error granting
        /// access. This can occur if your client ID is incorrect, for instance,
        /// or your refresh token is invalid. The <code>Response.Error</code>
        /// property can tell you more about what went wrong.</exception>
        /// <exception cref="UnexpectedHttpException">If some unknown
        /// error occurred.</exception>
        /// <exception cref="TaskCanceledException">If the cancellation
        /// token fires.</exception>
        public async Task<OAuthTokens> RefreshAsync(OAuthTokens tokens, CancellationToken cancellationToken = default(CancellationToken))
        {
            var content = HttpHelpers.JsonContent(
                new
                {
                    grant_type = "refresh_token",
                    refresh_token = tokens.RefreshToken,
                    client_id = this.options.ClientId
                });

            var response = await this.options.Client.PostAsync(
                $"{this.options.Host}/api/v1/oauth/token",
                content,
                cancellationToken);

            using (response)
            {
                await HttpHelpers.ThrowInvalidContentsAsync(response);
                return new OAuthTokens(
                    JsonConvert.DeserializeObject<OAuthTokenResponse>(await response.Content.ReadAsStringAsync()),
                    tokens.Scopes);
            }
        }
    }
}
