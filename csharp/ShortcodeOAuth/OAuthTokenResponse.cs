// -----------------------------------------------------------------------
// <copyright file="OAuthTokenResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Response returned from the server when retrieving a new access token,
    /// either through an Authorization Code grant or refresh token.
    /// </summary>
    [DataContract]
    internal class OAuthTokenResponse
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
    }
}
