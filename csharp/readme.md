# Microsoft.Mixer.ShortcodeOAuth

```
nuget install Microsoft.Mixer.ShortcodeOAuth
```

This is a client for Mixer shortcode OAuth in C#. An example can be found in `Example/Program.cs` in this directory. For more information about OAuth in general, check out the [OAuth reference page](https://dev.mixer.com/reference/oauth/index.html) on the Mixer developer site.

```csharp
class Program
{
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
```
