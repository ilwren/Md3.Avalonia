using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdFileSystemPickerNavigationRaceTests
{
    [AvaloniaFact]
    public async Task Attaching_Does_Not_Cancel_An_Explicit_Navigation_Already_In_Progress()
    {
        var root = Path.Combine(Path.GetTempPath(), $"md3-navigation-race-{Guid.NewGuid():N}");
        var initial = Path.Combine(root, "initial");
        Directory.CreateDirectory(initial);
        File.WriteAllText(Path.Combine(root, "target.json"), "{}");
        for (var i = 0; i < 500; i++) File.WriteAllText(Path.Combine(root, $"item-{i:D4}.txt"), "x");
        var picker = new MdFileSystemPicker { InitialDirectory = initial, FilePatterns = ["*.json"] };
        var window = new Window { Width = 700, Height = 500 };
        window.Show();
        try
        {
            // A larger fixture keeps enumeration in flight while the already-open window
            // attaches the picker; its continuation cannot commit without a dispatcher yield.
            var pending = picker.NavigateAsync(root);
            Assert.True(picker.IsLoading);
            window.Content = picker;
            window.UpdateLayout();
            Assert.True(picker.IsAttachedToVisualTree());
            Assert.True(await pending);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(Path.GetFullPath(root), picker.CurrentDirectory);
            Assert.Contains(picker.Entries, entry => entry.Name == "target.json");
            Assert.False(picker.IsLoading);
        }
        finally
        {
            window.Close();
            Directory.Delete(root, recursive: true);
        }
    }
}
