// -----------------------------------------------------------------------
// <copyright file="UnexpectedHttpException.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Mixer.ShortcodeOAuth
{
    using System;
    using System.Net;

    /// <summary>
    /// The exception thrown for any unsuccessful OAuth calls where the
    /// response was not otherwise expected.
    /// </summary>
    public class UnexpectedHttpException : Exception
    {
        public UnexpectedHttpException(
            HttpStatusCode statusCode,
            string path,
            string responseBody)
            : base($"Unexpected {statusCode} from {path}")
        {
            this.ResponseBody = responseBody;
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Gets or sets the contents of the response.
        /// </summary>
        public string ResponseBody { get; set; }

        /// <summary>
        /// Gets or sets the response status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
    }
}
