using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Localization;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdFileSystemPickerTests
{
    [AvaloniaFact]
    public async Task Picker_Enumerates_Real_Paths_Filters_Files_And_Confirms_Selection()
    {
        var root = CreateFixture();
        var picker = new MdFileSystemPicker
        {
            Mode = MdFileSystemPickerMode.File,
            FilePatterns = ["*.json"],
            Width = 680,
            Height = 480
        };
        var window = new Window { Width = 760, Height = 560, Content = picker };
        window.Show();
        try
        {
            Assert.True(await picker.NavigateAsync(root));
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(Path.GetFullPath(root), picker.CurrentDirectory);
            Assert.Contains(picker.Entries, entry => entry.IsDirectory && entry.Name == "folder");
            var json = Assert.Single(picker.Entries, entry => !entry.IsDirectory);
            Assert.Equal("theme.json", json.Name);
            Assert.DoesNotContain(picker.Entries, entry => entry.Name == "notes.txt");
            Assert.True(Path.IsPathFullyQualified(json.FullPath));

            var list = picker.GetVisualDescendants().OfType<MdList>()
                .Single(control => control.Name == "PART_ItemsHost");
            list.SelectedItem = json;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal([json.FullPath], picker.SelectedPaths);
            Assert.True(picker.CanConfirm);

            IReadOnlyList<string>? confirmed = null;
            picker.SelectionConfirmed += (_, args) => confirmed = args.Paths;
            picker.ConfirmSelection();
            Assert.Equal([json.FullPath], confirmed);
        }
        finally
        {
            window.Close();
            Directory.Delete(root, recursive: true);
        }
    }

    [AvaloniaFact]
    public async Task Folder_Mode_Can_Confirm_Current_Directory_And_Reports_Navigation_Errors()
    {
        var root = CreateFixture();
        var picker = new MdFileSystemPicker { Mode = MdFileSystemPickerMode.Folder };
        try
        {
            Assert.True(await picker.NavigateAsync(root));
            Assert.All(picker.Entries, entry => Assert.True(entry.IsDirectory));
            Assert.True(picker.CanConfirm);

            IReadOnlyList<string>? confirmed = null;
            picker.SelectionConfirmed += (_, args) => confirmed = args.Paths;
            picker.ConfirmSelection();
            Assert.Equal([Path.GetFullPath(root)], confirmed);

            MdFileSystemNavigationFailedEventArgs? failure = null;
            picker.NavigationFailed += (_, args) => failure = args;
            var missing = Path.Combine(root, "missing");
            Assert.False(await picker.NavigateAsync(missing));
            Assert.NotNull(failure);
            Assert.Equal(missing, failure!.Path);
            Assert.NotNull(picker.ErrorMessage);
            Assert.Equal(Path.GetFullPath(root), picker.CurrentDirectory);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [AvaloniaFact]
    public async Task Breadcrumb_Path_Editing_And_Responsive_Quick_Access_Are_Available()
    {
        var root = CreateFixture();
        var nested = Path.Combine(root, "folder", "nested");
        Directory.CreateDirectory(nested);
        var picker = new MdFileSystemPicker();
        var window = new Window { Width = 920, Height = 620, Content = picker };
        window.Show();
        try
        {
            Assert.True(await picker.NavigateAsync(nested));
            Dispatcher.UIThread.RunJobs();

            Assert.NotEmpty(picker.PathSegments);
            Assert.Equal(Path.GetFullPath(nested), picker.PathSegments[^1].FullPath);
            Assert.Equal("nested", picker.PathSegments[^1].Label);
            Assert.True(picker.PathSegments[^1].IsCurrent);
            Assert.All(picker.PathSegments.Take(picker.PathSegments.Count - 1), segment => Assert.False(segment.IsCurrent));
            Assert.All(picker.PathSegments, segment => Assert.True(Path.IsPathFullyQualified(segment.FullPath)));

            var quickAccess = picker.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_QuickAccess");
            var compactHome = picker.GetVisualDescendants().OfType<MdIconButton>()
                .Single(control => control.Name == "PART_CompactHomeButton");
            Assert.True(quickAccess.IsVisible);
            Assert.False(compactHome.IsVisible);

            var list = picker.GetVisualDescendants().OfType<MdList>()
                .Single(control => control.Name == "PART_ItemsHost");
            Assert.True(
                list.SelectionMode.HasFlag(SelectionMode.Toggle),
                $"single-selection list should include Toggle; actual mode was {list.SelectionMode}");

            picker.BreadcrumbStyle = MdFileSystemBreadcrumbStyle.Powerline;
            Dispatcher.UIThread.RunJobs();
            var powerline = picker.GetVisualDescendants().OfType<Grid>()
                .First(control => control.Classes.Contains("powerline-segment"));
            Assert.True(
                powerline.IsVisible,
                $"powerline visual should be visible; property={picker.IsPowerlineBreadcrumb}, local={powerline.GetValue(Visual.IsVisibleProperty)}");

            MdLocalization.SetCulture(picker, CultureInfo.GetCultureInfo("zh-CN"));
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("位置", picker.PathLabel);
            Assert.Equal("快捷访问", picker.QuickAccessText);
            Assert.Equal("显示隐藏项目", picker.ShowHiddenText);
            picker.Title = "Custom browser";
            MdLocalization.SetCulture(picker, CultureInfo.GetCultureInfo("en-US"));
            Assert.Equal("Custom browser", picker.Title);

            var pathToggle = picker.GetVisualDescendants().OfType<MdIconButton>()
                .Single(control => control.Name == "PART_PathToggleButton");
            var pathBox = picker.GetVisualDescendants().OfType<MdTextBox>()
                .Single(control => control.Name == "PART_PathBox");
            pathToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            Assert.True(pathBox.IsVisible);

            window.Width = 680;
            Dispatcher.UIThread.RunJobs();
            Assert.False(quickAccess.IsVisible);
            Assert.True(compactHome.IsVisible);
        }
        finally
        {
            window.Close();
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateFixture()
    {
        var root = Path.Combine(Path.GetTempPath(), $"md3-picker-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "folder"));
        File.WriteAllText(Path.Combine(root, "theme.json"), "{}");
        File.WriteAllText(Path.Combine(root, "notes.txt"), "notes");
        File.WriteAllText(Path.Combine(root, ".hidden.json"), "{}");
        return root;
    }
}
