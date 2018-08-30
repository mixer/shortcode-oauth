// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Example
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Mixer.ShortcodeOAuth;

    /// <summary>
    /// Demo app entry point.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Stub to run <see cref="RunAsync"/>
        /// </summary>
        /// <param name="args">Program arguments</param>
        static void Main(string[] args)
        {
            Program.RunAsync().Wait();
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        /// <summary>
        /// Retrieves an OAuth shortcode for the user, prompting them
        /// to enter it and waiting until they do.
        /// </summary>
        /// <returns>A task that resolves once codes are retrieved.</returns>
        private static async Task RunAsync()
        {
            // Create your OAuth client. Specify your client ID, and which permissions you want.
            var client = new OAuthClient(
                new OAuthOptions
                {
                    ClientId = "your client id here!",
                    Scopes = new[] { "interactive:robot:self" },
                });

            // Use the helper GrantAsync to get codes. Alternately, you can run
            // the granting/polling loop manually using client.GetSingleCodeAsync.
            var tokens = await client.GrantAsync(
                code => Console.WriteLine($"Go to mixer.com/go and enter {code}"),
                CancellationToken.None);

            // and that's it!
            Console.WriteLine($"Access token: {tokens.AccessToken}");
            Console.WriteLine($"Refresh token: {tokens.RefreshToken}");
            Console.WriteLine($"Expires At: {tokens.ExpiresAt}");
        }
    }
}
