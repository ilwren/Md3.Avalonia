using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class TagInputGalleryPage : UserControl
{
    private readonly ObservableCollection<string> _tags = [L("Accessibility", "无障碍"), "Android"];
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public TagInputGalleryPage()
    {
        InitializeComponent();
        TagInput.TagsSource = _tags;
        TagInput.SuggestionsSource = new[] { L("Accessibility", "无障碍"), "Android", L("Desktop", "桌面"), L("Localization", "本地化"), L("Motion", "动效"), L("Performance", "性能"), L("Testing", "测试"), L("Theming", "主题") };
        TagInput.TagValidator = tag => tag.Length < 3 ? L("Use at least 3 characters", "请至少输入 3 个字符") : null;
    }

    private void TagAdded(object? sender, MdTagChangedEventArgs e) =>
        TagStatus.Text = L($"Added {e.Tag}; {TagInput.Tags.Count} of {TagInput.MaximumTags} selected.", $"已添加 {e.Tag}；已选择 {TagInput.Tags.Count} / {TagInput.MaximumTags} 项。");

    private void TagRemoved(object? sender, MdTagChangedEventArgs e) =>
        TagStatus.Text = L($"Removed {e.Tag}; {TagInput.Tags.Count} remain.", $"已移除 {e.Tag}；剩余 {TagInput.Tags.Count} 项。");

    private void AddLocalizationTag(object? sender, RoutedEventArgs e) => TagInput.AddTag(L("Localization", "本地化"));

    private void ClearTags(object? sender, RoutedEventArgs e)
    {
        TagInput.ClearTags();
        TagStatus.Text = L("All tags cleared through the direct API.", "已通过直接 API 清除全部标签。");
    }
}
