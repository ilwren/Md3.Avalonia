using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;

namespace Md3.Avalonia.Gallery.Pages;

public partial class FocusShortcutHeroGalleryPage : UserControl
{
    private bool _heroExpanded;
    private bool _heroTransitionRunning;
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public FocusShortcutHeroGalleryPage()
    {
        InitializeComponent();
        HeroSource.TransitionRequested += (_, args) => AdvancedStatus.Text = L($"Avatar moved: {args.SourceBounds} → {args.DestinationBounds}.", $"头像已移动：{args.SourceBounds} → {args.DestinationBounds}。");
        HeroDestination.TransitionRequested += (_, args) => AdvancedStatus.Text = L($"Avatar returned: {args.SourceBounds} → {args.DestinationBounds}.", $"头像已返回：{args.SourceBounds} → {args.DestinationBounds}。");
        ShortcutScope.Register(new KeyGesture(Key.S, KeyModifiers.Control | KeyModifiers.Shift),
            new RelayCommand(ExecuteShortcutAction));
    }

    private void DemoControlFocused(object? sender, RoutedEventArgs e)
    {
        var label = (sender as ContentControl)?.Content?.ToString() ?? L("control", "控件");
        AdvancedStatus.Text = L($"Keyboard focus moved to {label}.", $"键盘焦点已移至 {label}。");
    }

    private void FocusFirstControl(object? sender, RoutedEventArgs e)
    {
        FocusAlpha.Focus();
        AdvancedStatus.Text = L("Focus traversal reset to Alpha; Tab now advances through Beta and Gamma.", "焦点遍历已重置到 Alpha；按 Tab 会依次进入 Beta 和 Gamma。");
    }

    private void RunShortcutAction(object? sender, RoutedEventArgs e) => ExecuteShortcutAction();

    private void ExecuteShortcutAction() =>
        AdvancedStatus.Text = L($"Shortcut action ran at {DateTime.Now:HH:mm:ss}.", $"快捷键操作已于 {DateTime.Now:HH:mm:ss} 执行。");

    private async void RequestHero(object? sender, RoutedEventArgs e)
    {
        // The flight is awaited, so a second click mid-air would interleave the two toggles.
        if (_heroTransitionRunning) return;
        _heroTransitionRunning = true;
        var source = _heroExpanded ? HeroDestination : HeroSource;
        var destination = _heroExpanded ? HeroSource : HeroDestination;

        // The destination is hidden until this click, so it has no bounds yet. Starting the
        // flight in the same pass made MdHero photograph a zero-sized rectangle and give up.
        // Reveal, let one layout pass run, fly, and only then hide the source.
        destination.IsVisible = true;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Loaded);
        await source.TransitionToAsync(destination);
        source.IsVisible = false;
        _heroExpanded = !_heroExpanded;
        _heroTransitionRunning = false;
    }
}
