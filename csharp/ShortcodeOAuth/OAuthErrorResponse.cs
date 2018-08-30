// -----------------------------------------------------------------------
// <copyright file="OAuthErrorResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Response type returned in OAuth errors.
    /// </summary>
    [DataContract]
    public class OAuthErrorResponse
    {
        /// <summary>
        /// Gets or sets the human-readable error description.
        /// </summary>
        [DataMember(Name = "error_description", IsRequired = false)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the OAuth error slug.
        /// </summary>
        [DataMember(Name = "error", IsRequired = true)]
        public OAuthError Error { get; set; }

        /// <summary>
        /// Gets or sets a URL describing more information about the error
        /// </summary>
        [DataMember(Name = "error_uri", IsRequired = false)]
        public string Uri { get; set; }
    }
}
