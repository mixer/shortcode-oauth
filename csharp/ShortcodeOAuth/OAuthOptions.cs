// -----------------------------------------------------------------------
// <copyright file="OAuthOptions.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System.Collections.Generic;
    using System.Net.Http;

    /// <summary>
    /// Options to pass to <see cref="OAuthClient"/>
    /// </summary>
    public class OAuthOptions
    {
        /// <summary>
        /// Gets or sets the http client to use for making requests.
        /// </summary>
        public HttpClient Client { get; set; } = new HttpClient();

        /// <summary>
        /// Gets or sets the OAuth client ID.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the OAuth client secret, if any.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the API host to hit.
        /// </summary>
        public string Host { get; set; } = "https://mixer.com";

        /// <summary>
        /// Gets or sets the OAuth scopes to request
        /// </summary>
        public ICollection<string> Scopes { get; set; }
    }
}
