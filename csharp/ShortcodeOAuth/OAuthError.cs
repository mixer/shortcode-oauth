// -----------------------------------------------------------------------
// <copyright file="OAuthError.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Enum in the <see cref="OAuthErrorResponse"/> which describes what went
    /// wrong. See <see cref="https://tools.ietf.org/html/rfc6749#section-5.2"/>
    /// for details.
    /// </summary>
    [DataContract]
    public enum OAuthError
    {
        /// <summary>
        /// The request is missing a required parameter
        /// </summary>
        [EnumMember(Value = "invalid_request")]
        InvalidRequest,

        /// <summary>
        /// Client authentication failed.
        /// </summary>
        [EnumMember(Value = "invalid_client")]
        InvalidClient,

        /// <summary>
        /// The provided authorization grant (e.g., authorization code, resource
        /// owner credentials) or refresh token is invalid, expired, revoked,
        /// does not match the redirection URI used in the authorization
        /// request, or was issued to another client.
        /// </summary>
        [EnumMember(Value = "invalid_grant")]
        InvalidGrant,

        /// <summary>
        /// The authenticated client is not authorized to use this
        /// authorization grant type.
        /// </summary>
        [EnumMember(Value = "unauthorized_client")]
        UnauthorizedClient,

        /// <summary>
        /// The authorization grant type is not supported by the authorization server.
        /// </summary>
        [EnumMember(Value = "unsupported_grant_type")]
        UnsupportedGrantType,

        /// <summary>
        /// The requested scope is invalid, unknown, or malformed. You're
        /// most likely requesting a permission which does not exist.
        /// </summary>
        [EnumMember(Value = "invalid_scope")]
        InvalidScope,

        /// <summary>
        /// The resource owner or authorization server denied the request.
        /// This can happen during shortcode authorization if the user clicks
        /// the "Deny" button on mixer.com/go.
        /// </summary>
        [EnumMember(Value = "access_denied")]
        AccessDenied
    }
}
