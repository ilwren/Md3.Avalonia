using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Localization;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdEmojiPickerTests
{
    [AvaloniaFact]
    public void Search_Categories_And_Localized_Accessible_Names_Work()
    {
        var picker = new MdEmojiPicker { Width = 480 };
        MdLocalization.SetCulture(picker, CultureInfo.GetCultureInfo("en-US"));
        var window = Show(picker);
        try
        {
            Assert.Equal(16, picker.ResultCount);
            Click(picker, "Hands");
            Assert.Equal("Hands", picker.SelectedCategory);
            Assert.Equal(4, picker.ResultCount);
            Assert.Equal("Hands, selected", ControlAutomationPeer.CreatePeerForElement(ButtonNamed(picker, "Hands, selected")).GetName());
            MdLocalization.SetCulture(picker, CultureInfo.GetCultureInfo("zh-CN"));
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(ButtonNamed(picker, "赞"));
            var search = picker.GetVisualDescendants().OfType<TextBox>().Single(c => c.Name == "PART_Search");
            Assert.Equal("搜索表情", AutomationProperties.GetName(search));
            search.Text = "挥手";
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("挥手", picker.SearchText);
            Assert.Equal("👋", Assert.Single(picker.VisibleItems).Emoji);
            Assert.NotNull(ButtonNamed(picker, "挥手"));
            picker.SearchText = "not present";
            Dispatcher.UIThread.RunJobs();
            Assert.Empty(picker.VisibleItems);
            var status = picker.GetVisualDescendants().OfType<TextBlock>().Single(c => c.Name == "PART_Status");
            Assert.Equal("无结果", status.Text);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Pointer_Selection_Updates_TwoWay_Binding_And_Emits_Repeated_Selections()
    {
        var model = new Model();
        var picker = new MdEmojiPicker { Width = 480 };
        MdLocalization.SetCulture(picker, CultureInfo.GetCultureInfo("en-US"));
        picker.Bind(MdEmojiPicker.SelectedEmojiProperty, new Binding(nameof(Model.Value)) { Source = model, Mode = BindingMode.TwoWay });
        var window = Show(picker);
        try
        {
            var count = 0;
            picker.EmojiSelected += (_, item) => { Assert.Equal("😀", item.Emoji); count++; };
            var button = ButtonNamed(picker, "Grinning face");
            var point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
            window.MouseMove(point, RawInputModifiers.None);
            window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("😀", picker.SelectedEmoji);
            Assert.Equal("😀", model.Value);
            Click(picker, "Grinning face");
            Assert.Equal(2, count);
            Assert.Equal(["😀"], picker.RecentEmoji);
            model.Value = "external";
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("external", picker.SelectedEmoji);
            Assert.Equal(["😀"], picker.RecentEmoji);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Keyboard_Activation_Keeps_Focus_Across_Category_And_Recent_Rebuilds()
    {
        var picker = new MdEmojiPicker { Width = 480 };
        MdLocalization.SetCulture(picker, CultureInfo.GetCultureInfo("en-US"));
        var window = Show(picker);
        try
        {
            var hands = ButtonNamed(picker, "Hands");
            Assert.True(hands.Focus());
            window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Hands", picker.SelectedCategory);
            Assert.True(ButtonNamed(picker, "Hands, selected").IsKeyboardFocusWithin);
            Click(picker, "Thumbs up");
            picker.IsRecentView = true;
            Dispatcher.UIThread.RunJobs();
            var recentItem = ButtonNamed(picker, "Thumbs up");
            Assert.True(recentItem.Focus());
            window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("👍", picker.SelectedEmoji);
            Assert.True(ButtonNamed(picker, "Thumbs up").IsKeyboardFocusWithin);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Recent_Items_Are_Bounded_Deduplicated_And_Filtered_Against_The_Catalog()
    {
        var items = new ObservableCollection<MdEmojiItem>
        {
            new("A", "Alpha", "One"), new("B", "Beta", "One"), new("C", "Gamma", "Two")
        };
        var picker = new MdEmojiPicker { Width = 480, ItemsSource = items, RecentLimit = 2 };
        var window = Show(picker);
        try
        {
            Click(picker, "Alpha");
            Click(picker, "Beta");
            Click(picker, "Alpha");
            Click(picker, "Gamma");
            Assert.Equal(["C", "A"], picker.RecentEmoji);
            picker.IsRecentView = true;
            Assert.Equal(["C", "A"], picker.VisibleItems.Select(x => x.Emoji));
            items.RemoveAt(2);
            Assert.Equal(["A"], picker.VisibleItems.Select(x => x.Emoji));
            picker.ClearRecents();
            Assert.Empty(picker.RecentEmoji);
            Assert.Empty(picker.VisibleItems);
            picker.IsRecentView = false;
            picker.RecentLimit = 0;
            Click(picker, "Alpha");
            Assert.Empty(picker.RecentEmoji);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Observable_Catalog_Updates_Attached_And_Refreshes_After_Reattachment()
    {
        var items = new ObservableCollection<MdEmojiItem> { new("A", "Alpha", "One") };
        var picker = new MdEmojiPicker { Width = 480, ItemsSource = items };
        var window = Show(picker);
        try
        {
            items.Add(new("B", "Beta", "Two"));
            Assert.Equal(2, picker.ResultCount);
            Assert.NotNull(ButtonNamed(picker, "Two"));
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            items.Add(new("C", "Gamma", "Two"));
            Assert.Equal(2, picker.ResultCount);
            window.Content = picker;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(3, picker.ResultCount);
            Assert.NotNull(ButtonNamed(picker, "Gamma"));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Visible_Limit_Does_Not_Change_Filtered_Count_And_Reports_Truncation()
    {
        var items = Enumerable.Range(0, 8).Select(i => new MdEmojiItem($"{i}", $"Item {i}", "Custom")).ToArray();
        var picker = new MdEmojiPicker { Width = 480, ItemsSource = items, MaxVisibleItems = 3 };
        MdLocalization.SetCulture(picker, CultureInfo.GetCultureInfo("en-US"));
        var window = Show(picker);
        try
        {
            Assert.Equal(8, picker.ResultCount);
            Assert.Equal(3, picker.VisibleItems.Count);
            Assert.Equal("Showing 3 of 8 results", picker.GetVisualDescendants().OfType<TextBlock>().Single(c => c.Name == "PART_Status").Text);
            picker.SearchText = "Item 7";
            Assert.Equal("7", Assert.Single(picker.VisibleItems).Emoji);
            Assert.Equal(1, picker.ResultCount);
        }
        finally { window.Close(); }
    }

    private static MdButton ButtonNamed(MdEmojiPicker picker, string name) => picker.GetVisualDescendants()
        .OfType<MdButton>().Single(button => AutomationProperties.GetName(button) == name ||
            (AutomationProperties.GetName(button) is null && Equals(button.Content, name)));

    private static void Click(MdEmojiPicker picker, string name)
    {
        ButtonNamed(picker, name).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
    }

    private static Window Show(Control picker)
    {
        picker.HorizontalAlignment = HorizontalAlignment.Left;
        picker.VerticalAlignment = VerticalAlignment.Top;
        var window = new Window { Width = 700, Height = 700, Content = picker };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private sealed class Model : INotifyPropertyChanged
    {
        private string? _value;
        public string? Value { get => _value; set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); } }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
