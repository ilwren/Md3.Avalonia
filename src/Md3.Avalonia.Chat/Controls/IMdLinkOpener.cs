namespace Md3.Avalonia.Chat;

/// <summary>
/// Opens a link found in a chat message. The controls never open links themselves: the application
/// decides whether to launch a browser, show a confirmation or ignore the request.
/// </summary>
public interface IMdLinkOpener
{
    /// <summary>Handles a link the user activated.</summary>
    /// <param name="uri">An absolute http or https address.</param>
    /// <returns>True when the application handled the link.</returns>
    bool Open(Uri uri);
}
