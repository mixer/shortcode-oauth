// -----------------------------------------------------------------------
// <copyright file="ShortcodeAuthorizedResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Response contract returned once a shortcode is accepted.
    /// </summary>
    [DataContract]
    internal class ShortcodeAuthorizedResponse
    {
        /// <summary>
        /// Gets or sets the authorization code to exchange for OAuth tokens.
        /// </summary>
        [DataMember(Name = "code", IsRequired = true)]
        public string Code { get; set; }
    }
}
