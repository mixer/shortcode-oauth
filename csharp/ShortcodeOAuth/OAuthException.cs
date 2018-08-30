// -----------------------------------------------------------------------
// <copyright file="OAuthException.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System;

    /// <summary>
    /// Exception type thrown when a known OAuth error occurs.
    /// </summary>
    public class OAuthException : Exception
    {
        public OAuthException(OAuthErrorResponse response)
            : base($"The server returned an {response.Error} error")
        {
            this.Response = response;
        }

        /// <summary>
        /// Gets the error that the server returned.
        /// </summary>
        public OAuthErrorResponse Response { get; }
    }
}
