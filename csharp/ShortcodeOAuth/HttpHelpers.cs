// -----------------------------------------------------------------------
// <copyright file="HttpHelpers.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Internal helpers for making HTTP requests and dealing with responses.
    /// </summary>
    internal static class HttpHelpers
    {
        /// <summary>
        /// Creates an HTTP exception from the response.
        /// </summary>
        /// <param name="response">HTTP response</param>
        /// <returns>A created exception</returns>
        public static async Task<UnexpectedHttpException> CreateExceptionFromResponse(HttpResponseMessage response)
        {
            var contents = await response.Content.ReadAsStringAsync();

            OAuthErrorResponse oauthError;
            try
            {
                oauthError = JsonConvert.DeserializeObject<OAuthErrorResponse>(contents);
            }
            catch (JsonSerializationException)
            {
                throw new UnexpectedHttpException(
                    response.StatusCode,
                    response.RequestMessage.RequestUri.AbsolutePath,
                    contents
                );
            }

            throw new OAuthException(oauthError);
        }

        /// <summary>
        /// Creates HTTP content by serializing the given object as JSON.
        /// </summary>
        /// <param name="data">Object to serialize</param>
        /// <returns>The new set of content.</returns>
        public static HttpContent JsonContent(object data)
        {
            return new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// Throws an <see cref="UnexpectedHttpException"/> if the response
        /// is not a 2xx.
        /// </summary>
        /// <param name="response">Response to check</param>
        /// <returns>A task that is completed or faulted depending on
        /// the status.</returns>
        public static async Task ThrowInvalidContentsAsync(HttpResponseMessage response)
        {
            if ((int)response.StatusCode >= 300)
            {
                throw await HttpHelpers.CreateExceptionFromResponse(response);
            }
        }
    }
}
