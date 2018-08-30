// -----------------------------------------------------------------------
// <copyright file="OAuthTokens.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// A set of OAuth tokens returned from the API.
    /// </summary>
    [DataContract]
    [Serializable]
    public class OAuthTokens
    {
        public OAuthTokens()
        {
        }

        /// <summary>
        /// Creates an <see cref="OAuthTokens"/> class from the deserialized
        /// API response.
        /// </summary>
        /// <param name="rawResponse">The server response</param>
        /// <param name="scopes">The scopes we requested.</param>
        internal OAuthTokens(OAuthTokenResponse rawResponse, IEnumerable<string> scopes)
        {
            this.AccessToken = rawResponse.AccessToken;
            this.RefreshToken = rawResponse.RefreshToken;
            this.Scopes = new HashSet<string>(scopes);
            this.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(rawResponse.ExpiresIn);
        }

        /// <summary>
        /// Gets the OAuth access token.
        /// </summary>
        [DataMember(Name = "access_token", IsRequired = true)]
        public string AccessToken { get; protected set; }

        /// <summary>
        /// Gets the time at which these tokens expire.
        /// </summary>
        [DataMember(Name = "expiresAt", IsRequired = true)]
        public DateTimeOffset ExpiresAt { get; protected set; }

        /// <summary>
        /// Gets the OAuth refresh token.
        /// </summary>
        [DataMember(Name = "refresh_token", IsRequired = false)]
        public string RefreshToken { get; protected set; }

        /// <summary>
        /// Gets a list of scopes granted to the token.
        /// </summary>
        [DataMember(Name = "scopes", IsRequired = true)]
        public ISet<string> Scopes { get; protected set; }
    }
}
