// -----------------------------------------------------------------------
// <copyright file="ShortcodeState.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    /// <summary>
    /// Enum used in the <see cref="OAuthShortcode"/> to indicate its state.
    /// </summary>
    public enum ShortcodeState
    {
        /// <summary>
        /// The user has granted the application the requested access. We
        /// can go retrieve OAuth tokens now.
        /// </summary>
        Accepted,

        /// <summary>
        /// The user has not yet accepted nor denied our authorization request.
        /// </summary>
        Waiting,

        /// <summary>
        /// The user denied our authorization request.
        /// </summary>
        Denied,

        /// <summary>
        /// The shortcode has expired.
        /// </summary>
        Expired,
    }
}
