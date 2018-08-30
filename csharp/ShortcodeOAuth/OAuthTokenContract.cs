// -----------------------------------------------------------------------
// <copyright file="OAuthTokenContract.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System.Runtime.Serialization;

    /// <summary>
    /// JSON serialization of <see cref="OAuthTokens"/>
    /// </summary>
    [DataContract]
    public class OAuthTokenContract
    {
        /// <summary>
        /// Gets or sets the OAuth access token.
        /// </summary>
        [DataMember(Name = "access_token", IsRequired = true)]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds until the <see cref="AccessToken"/>
        /// expires.
        /// </summary>
        [DataMember(Name = "expires_in", IsRequired = true)]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the OAuth refresh token.
        /// </summary>
        [DataMember(Name = "refresh_token", IsRequired = false)]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets a list of scopes granted to the token.
        /// </summary>
        [DataMember(Name = "scopes", IsRequired = true)]
        public string[] Scopes { get; set; }
    }
}
