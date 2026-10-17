namespace Md3.Avalonia.Gallery;

internal static class GalleryNotificationIdentity
{
    public const string WindowsAppUserModelId = "Md3.Avalonia.Gallery";

    public static bool WindowsRegistrationSucceeded { get; set; }

    public static string? WindowsRegistrationError { get; set; }
}
