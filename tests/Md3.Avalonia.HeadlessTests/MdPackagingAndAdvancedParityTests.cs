using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdPackagingAndAdvancedParityTests
{
    [AvaloniaFact]
    public void Three_Package_Dependency_Direction_Is_Optional_And_One_Way()
    {
        var coreReferences = typeof(MdButton).Assembly.GetReferencedAssemblies().Select(reference => reference.Name).ToArray();
        var iconReferences = typeof(Md3.Avalonia.Icons.MdExternalMaterialSymbols).Assembly.GetReferencedAssemblies().Select(reference => reference.Name).ToArray();
        var ecosystemReferences = typeof(MdAvatar).Assembly.GetReferencedAssemblies().Select(reference => reference.Name).ToArray();

        Assert.DoesNotContain("Md3.Avalonia.Icons", coreReferences);
        Assert.DoesNotContain("Md3.Avalonia.Icons", ecosystemReferences);
        Assert.Contains("Md3.Avalonia", ecosystemReferences);
        Assert.DoesNotContain("Md3.Avalonia", iconReferences);
    }

    [AvaloniaFact]
    public void Phase2_Pagination_Reorder_And_Dismiss_APIs_Work()
    {
        var table = new MdPaginatedDataTable { ItemsSource = Enumerable.Range(1, 8).ToArray(), RowsPerPage = 3 };
        Assert.Equal(new object?[] { 1, 2, 3 }, table.PageItems);
        table.NextPage();
        Assert.Equal(1, table.PageIndex);
        Assert.Equal(new object?[] { 4, 5, 6 }, table.PageItems);
        Assert.Equal("4–6 of 8", table.PageLabel);

        var items = new ObservableCollection<string>(["One", "Two", "Three"]);
        var list = new MdReorderableList { ItemsSource = items, SelectedIndex = 1 };
        Assert.True(list.MoveSelectedUp());
        Assert.Equal(["Two", "One", "Three"], items);

        var dismissible = new MdDismissible { Width = 300, Content = "Swipe" };
        Assert.True(dismissible.Dismiss(MdDismissDirection.EndToStart));
        Assert.True(dismissible.IsDismissed);
        dismissible.Restore();
        Assert.False(dismissible.IsDismissed);
        Assert.Equal(0, dismissible.DragOffset);
    }

    [AvaloniaFact]
    public void Phase3_Form_Dialog_License_And_Restoration_APIs_Work()
    {
        var required = new MdFormField { IsRequired = true, Value = string.Empty };
        var form = new MdForm { Content = required };
        using var host = Show(form);
        Assert.False(form.Validate());
        required.Value = "Material";
        Assert.True(form.Submit());

        var dialog = new MdSimpleDialog { ItemsSource = new[] { "A", "B" } };
        dialog.Show();
        Assert.True(dialog.IsOpen);
        dialog.Dismiss();
        Assert.False(dialog.IsOpen);

        var licenses = new[] { new MdLicenseEntry("Core", "Apache-2.0", "Text"), new MdLicenseEntry("UI", "MIT", "Text") };
        var page = new MdLicensePage { Licenses = licenses, Filter = "Core" };
        Assert.Single(page.FilteredLicenses);

        var state = new MdPickerRestorationStore();
        var date = new DateTimeOffset(2026, 9, 24, 0, 0, 0, TimeSpan.Zero);
        var time = new TimeSpan(10, 15, 0);
        state.SaveDate("date", date);
        state.SaveTime("time", time);
        Assert.Equal(date, state.RestoreDate("date"));
        Assert.Equal(time, state.RestoreTime("time"));
    }

    [AvaloniaFact]
    public void Phase4_Sheet_Adaptive_Hero_And_Shortcut_APIs_Work()
    {
        var sheet = new MdDraggableScrollableSheet { Width = 600, Height = 400, MinimumExtent = 0.25, MaximumExtent = 1, Extent = 0.5 };
        using var host = Show(sheet, 640, 440);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(sheet.Bounds.Height * sheet.Extent, sheet.SheetHeight, precision: 3);
        sheet.JumpTo(1);
        Assert.Equal(1, sheet.Extent);
        sheet.Reset();
        Assert.Equal(sheet.InitialExtent, sheet.Extent);

        var adaptiveSwitch = new MdAdaptiveSwitch { Platform = MdAdaptivePlatform.Cupertino, IsChecked = true };
        var adaptiveProgress = new MdAdaptiveProgressIndicator { Platform = MdAdaptivePlatform.Material, IsIndeterminate = true };
        Assert.True(adaptiveSwitch.UsesCupertinoStyle);
        Assert.False(adaptiveProgress.UsesCupertinoStyle);

        var source = new MdHero { Tag = "hero" };
        var destination = new MdHero { Tag = "hero" };
        MdHeroTransitionEventArgs? request = null;
        source.TransitionRequested += (_, args) => request = args;
        source.RequestTransitionTo(destination);
        Assert.NotNull(request);
    }

    [AvaloniaFact]
    public void Ecosystem_Controls_Render_And_Rating_Remains_Fractional()
    {
        var avatar = new MdAvatar { Initials = "M3" };
        var rating = new MdRating { Value = 3.5, Precision = 0.5 };
        var breadcrumb = new MdBreadcrumb { Items = { "Home", "Components", "Ecosystem" }, SelectedIndex = 1 };
        var panel = new StackPanel { Children = { avatar, rating, breadcrumb } };
        using var host = Show(panel);
        Dispatcher.UIThread.RunJobs();

        Assert.NotNull(avatar.Template);
        Assert.Equal(3.5, rating.Value);
        Assert.Null(breadcrumb.SelectedItem);
        Assert.Equal(-1, breadcrumb.SelectedIndex);
        Assert.NotNull(((Scope)host).Window.CaptureRenderedFrame());
    }

    [AvaloniaFact]
    public void Gallery_Uses_Material_NavigationDrawer_And_Mobile_Destinations_Change_Content()
    {
        var gallery = new MainWindow { Width = 520, Height = 760 };
        gallery.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var drawer = gallery.GetVisualDescendants().OfType<MdNavigationDrawer>().Single(control => control.Name == "NavigationPane");
            Assert.True(drawer.IsModal);

            var mobile = new AndroidGalleryView();
            using var host = Show(mobile, 420, 760);
            var navigation = mobile.GetVisualDescendants().OfType<MdNavigationBar>().Single();
            var content = mobile.GetVisualDescendants().OfType<ContentControl>().Single(control => control.Name == "MobilePageHost");
            var original = content.Content;
            navigation.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.NotSame(original, content.Content);
        }
        finally { gallery.Close(); }
    }

    [AvaloniaFact]
    public void Numeric_Spinner_And_Refresh_Pushdown_Surface_Are_Real()
    {
        var numeric = new MdNumericBox { Value = 4, Minimum = 0, Maximum = 10, ShowButtonSpinner = true };
        var refresh = new MdRefreshIndicator { Height = 240, Content = new TextBlock { Text = "Content" } };
        using var host = Show(new StackPanel { Children = { numeric, refresh } }, 900, 800);
        Dispatcher.UIThread.RunJobs();

        var spinnerButtons = numeric.GetVisualDescendants().OfType<RepeatButton>().ToArray();
        Assert.Equal(2, spinnerButtons.Length);
        Assert.All(spinnerButtons, button => { Assert.Equal(32, button.Bounds.Width); Assert.Equal(20, button.Bounds.Height); });
        refresh.BeginRefresh();
        Dispatcher.UIThread.RunJobs();
        var indicatorHost = refresh.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_IndicatorHost");
        // Headless does not guarantee a sampled intermediate transition frame. The control's
        // logical extent and configured settle transition are deterministic.
        Assert.Equal(refresh.Displacement, refresh.PullOffset);
        Assert.NotNull(indicatorHost.Transitions);
        Assert.Equal("Refreshing…", refresh.StateText);
    }

    [AvaloniaFact]
    public void Search_Shortcut_Opens_Expanded_View_And_Rapid_Tab_Changes_Use_One_Content_Host()
    {
        var searchBar = new MdSearchBar();
        var search = new MdSearchView { Header = searchBar, IsOpen = false };
        var tabs = new MdTabView
        {
            Items =
            {
                new MdTabViewItem { Header = "One", Content = "First" },
                new MdTabViewItem { Header = "Two", Content = "Second" }
            }
        };
        var window = new Window { Width = 700, Height = 400, Content = new StackPanel { Children = { search, tabs } } };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.KeyPress(Key.K, RawInputModifiers.Control, PhysicalKey.K, "k");
            window.KeyRelease(Key.K, RawInputModifiers.Control, PhysicalKey.K, "k");
            Assert.True(search.IsOpen);
            for (var index = 0; index < 100; index++) tabs.SelectedIndex = index % 2;
            tabs.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Second", tabs.AnimatedSelectedContent);
            Assert.Single(tabs.GetVisualDescendants().OfType<Control>(), control => control.Name == "PART_SelectedContentHost");
            Assert.DoesNotContain(tabs.GetVisualDescendants(), control => control is TransitioningContentControl);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void About_Dialog_Fills_Itself_In_From_The_Entry_Assembly()
    {
        var dialog = new MdAboutDialog();
        using var host = Show(dialog, 620, 360);

        // Nothing was set, so the common case must still name the application.
        Assert.False(string.IsNullOrWhiteSpace(dialog.EffectiveApplicationName));
        Assert.Null(dialog.ApplicationName);

        // Whatever the host assembly declares, the version line never shows raw SourceLink
        // provenance: "1.2.3+<sha>" is a version plus a commit, not a version.
        Assert.DoesNotContain('+', dialog.EffectiveApplicationVersion ?? string.Empty);
    }

    [AvaloniaFact]
    public void About_Dialog_Prefers_What_The_Application_Sets()
    {
        var dialog = new MdAboutDialog();
        using var host = Show(dialog, 620, 360);
        var inferred = dialog.EffectiveApplicationName;

        dialog.ApplicationName = "Ledger";
        dialog.ApplicationVersion = "4.2.0";
        dialog.Legalese = "Copyright the Ledger authors.";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("Ledger", dialog.EffectiveApplicationName);
        Assert.Equal("4.2.0", dialog.EffectiveApplicationVersion);
        Assert.Equal("Copyright the Ledger authors.", dialog.EffectiveLegalese);

        // Clearing hands the field back to the assembly rather than blanking the dialog, and a
        // whitespace value counts as unset for the same reason.
        dialog.ApplicationName = "   ";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(inferred, dialog.EffectiveApplicationName);
    }

    [AvaloniaFact]
    public void About_Dialog_Collapses_The_Lines_It_Has_Nothing_For()
    {
        var dialog = new MdAboutDialog { Legalese = "Copyright the Ledger authors." };
        using var host = Show(dialog, 620, 360);

        var icon = dialog.GetVisualDescendants().OfType<ContentPresenter>().Single(part => part.Name == "PART_Icon");
        var layout = dialog.GetVisualDescendants().OfType<Grid>().Single(part => part.Name == "PART_Layout");
        var legalese = dialog.GetVisualDescendants().OfType<TextBlock>().Single(part => part.Name == "PART_Legalese");
        var version = dialog.GetVisualDescendants().OfType<TextBlock>().Single(part => part.Name == "PART_Version");

        // No icon means no icon column and no 20 DIP gutter leading nowhere.
        Assert.False(icon.IsVisible);
        Assert.Equal(0, layout.ColumnSpacing);

        dialog.ApplicationIcon = new Border { Width = 48, Height = 48 };
        Dispatcher.UIThread.RunJobs();
        Assert.True(icon.IsVisible);
        Assert.Equal(20, layout.ColumnSpacing);

        // Each optional line is shown exactly when there is text behind it.
        Assert.True(legalese.IsVisible);
        Assert.Equal(!string.IsNullOrWhiteSpace(dialog.EffectiveApplicationVersion), version.IsVisible);
    }

    private static IDisposable Show(Control content, double width = 800, double height = 600)
    {
        var window = new Window { Width = width, Height = height, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public Window Window { get; } = window;
        public void Dispose() => Window.Close();
    }
}
