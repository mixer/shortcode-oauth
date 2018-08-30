// -----------------------------------------------------------------------
// <copyright file="OAuthShortcode.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// A single instance of an OAuth shortcode. You can use this to wait
    /// for the user to grant access.
    /// </summary>
    public class OAuthShortcode
    {
        /// <summary>
        /// OAuth options.
        /// </summary>
        private readonly OAuthOptions options;

        /// <summary>
        /// Code set in <see cref="PollAsync"/> on success. Used to exchange
        /// the shortcode for an OAuth authorization token.
        /// </summary>
        private string readCode;

        internal OAuthShortcode(OAuthOptions options, ShortcodeCreateResponse response)
        {
            this.options = options;
            this.Code = response.Code;
            this.Handle = response.Handle;
            this.ExpiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(response.ExpiresIn);
        }

        /// <summary>
        /// Gets the six-digit code to display to the user.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the time at which this shortcode expires.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; }

        /// <summary>
        /// Gets the handle used to query the code status.
        /// </summary>
        public string Handle { get; }

        /// <summary>
        /// Retrieves a set of OAuth tokens using the shortcode.
        /// </summary>
        /// <param name="cancellationToken">Cancellation for the operation</param>
        /// <returns>The created set of OAuth tokens.</returns>
        /// <exception cref="InvalidOperationException">If called when the
        /// <see cref="ShortcodeState"/> is not <code>Accepted</code>.</exception>
        public async Task<OAuthTokens> GetTokensAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.readCode == null)
            {
                var state = await this.PollAsync(cancellationToken);
                if (state != ShortcodeState.Accepted)
                {
                    throw new InvalidOperationException(
                        "GetTokensAsync may only be called once a shortcode "
                        + $"is {ShortcodeState.Accepted},  but it was {state}.");
                }
            }

            var content = HttpHelpers.JsonContent(
                new
                {
                    client_id = this.options.ClientId,
                    grant_type = "authorization_code",
                    code = this.readCode
                });

            using (var response = await this.options.Client.PostAsync(
                $"{this.options.Host}/api/v1/oauth/token",
                content,
                cancellationToken))
            {
                await HttpHelpers.ThrowInvalidContentsAsync(response);
                return new OAuthTokens(
                    JsonConvert.DeserializeObject<OAuthTokenResponse>(await response.Content.ReadAsStringAsync()),
                    this.options.Scopes);
            }
        }

        /// <summary>
        /// Runs a single poll to check that status of the ShortCode.
        /// </summary>
        /// <param name="cancellationToken">Cancellation for the operation</param>
        /// <returns>The state of the shortcode</returns>
        /// <exception cref="UnexpectedHttpException">If an unknown status
        /// code is returned.</exception>
        public async Task<ShortcodeState> PollAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (DateTimeOffset.UtcNow >= this.ExpiresAt)
            {
                return ShortcodeState.Expired;
            }

            using (var response = await this.options.Client.GetAsync(
                $"{this.options.Host}/api/v1/oauth/shortcode/check/{this.Handle}",
                cancellationToken))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var parsed = JsonConvert.DeserializeObject<ShortcodeAuthorizedResponse>(
                            await response.Content.ReadAsStringAsync());
                        this.readCode = parsed.Code;
                        return ShortcodeState.Accepted;
                    case HttpStatusCode.NoContent:
                        return ShortcodeState.Waiting;
                    case HttpStatusCode.Forbidden:
                        return ShortcodeState.Denied;
                    case HttpStatusCode.NotFound:
                        return ShortcodeState.Expired;
                    default:
                        throw await HttpHelpers.CreateExceptionFromResponse(response);
                }
            }
        }
    }
}
