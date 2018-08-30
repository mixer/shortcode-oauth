// -----------------------------------------------------------------------
// <copyright file="ShortcodeCreateResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Response returned from the endpoint to retrieve an OAuth shortcode.
    /// </summary>
    [DataContract]
    internal class ShortcodeCreateResponse
    {
        /// <summary>
        /// Gets or sets the six-digit code to display to the user.
        /// </summary>
        [DataMember(Name = "code", IsRequired = true)]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds until the code expires.
        /// </summary>
        [DataMember(Name = "expires_in", IsRequired = true)]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the code handle to poll for success.
        /// </summary>
        [DataMember(Name = "handle", IsRequired = true)]
        public string Handle { get; set; }
    }
}
