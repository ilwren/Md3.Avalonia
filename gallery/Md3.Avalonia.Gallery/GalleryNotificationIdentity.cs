namespace Md3.Avalonia.Gallery;

internal static class GalleryNotificationIdentity
{
    public const string WindowsAppUserModelId = "Md3.Avalonia11.Gallery";

    public static bool WindowsRegistrationSucceeded { get; set; }

    public static string? WindowsShortcutPath { get; set; }

    public static string? WindowsRegistrationError { get; set; }
}
