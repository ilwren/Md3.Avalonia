using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Layout;
using Avalonia.Media;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Optional Material preview and metadata surface for a selected local or provider-backed entry.</summary>
[PseudoClasses(":loading", ":has-preview", ":has-error")]
public sealed class MdFilePreviewPane : UserControl
{
    public static readonly StyledProperty<MdFileEntry?> EntryProperty =
        AvaloniaProperty.Register<MdFilePreviewPane, MdFileEntry?>(nameof(Entry));
    public static readonly StyledProperty<MdFilePreview?> PreviewProperty =
        AvaloniaProperty.Register<MdFilePreviewPane, MdFilePreview?>(nameof(Preview));
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<MdFilePreviewPane, bool>(nameof(IsLoading));
    public static readonly StyledProperty<string?> ErrorMessageProperty =
        AvaloniaProperty.Register<MdFilePreviewPane, string?>(nameof(ErrorMessage));

    private readonly MdSurface _surface;
    private readonly MdText _title;
    private readonly MdText _description;
    private readonly MdIconButton _closeButton;
    private readonly StackPanel _body;
    private readonly MdText _emptyMessage;

    static MdFilePreviewPane()
    {
        EntryProperty.Changed.AddClassHandler<MdFilePreviewPane>((pane, _) => pane.UpdateView());
        PreviewProperty.Changed.AddClassHandler<MdFilePreviewPane>((pane, _) => pane.UpdateView());
        IsLoadingProperty.Changed.AddClassHandler<MdFilePreviewPane>((pane, _) => pane.UpdateView());
        ErrorMessageProperty.Changed.AddClassHandler<MdFilePreviewPane>((pane, _) => pane.UpdateView());
    }

    public MdFilePreviewPane()
    {
        _title = new MdText { StyleRole = MdTextStyle.TitleMedium, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center };
        _description = new MdText { StyleRole = MdTextStyle.BodySmall, TextWrapping = TextWrapping.Wrap };
        _closeButton = new MdIconButton { Icon = "×", Variant = MdIconButtonVariant.Standard };
        ToolTip.SetTip(_closeButton, "Close preview");
        AutomationProperties.SetName(_closeButton, "Close preview");
        _closeButton.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        _emptyMessage = new MdText { Text = "Select a file to preview", StyleRole = MdTextStyle.BodyMedium, TextWrapping = TextWrapping.Wrap, Opacity = 0.72 };
        _body = new StackPanel { Spacing = 12 };
        _surface = new MdSurface
        {
            Variant = MdSurfaceVariant.ContainerLow,
            Padding = new Thickness(16),
            Content = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,Auto,*"),
                RowSpacing = 12,
                Children =
                {
                    CreateHeader(),
                    _description,
                    CreateBodyHost()
                }
            }
        };
        Grid.SetRow(_description, 1);
        Content = _surface;
        UpdateView();
    }

    public MdFileEntry? Entry { get => GetValue(EntryProperty); set => SetValue(EntryProperty, value); }
    public MdFilePreview? Preview { get => GetValue(PreviewProperty); set => SetValue(PreviewProperty, value); }
    public bool IsLoading { get => GetValue(IsLoadingProperty); set => SetValue(IsLoadingProperty, value); }
    public string? ErrorMessage { get => GetValue(ErrorMessageProperty); set => SetValue(ErrorMessageProperty, value); }

    public event EventHandler? CloseRequested;

    private Control CreateHeader()
    {
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 8,
            Children = { _title, _closeButton }
        };
        Grid.SetColumn(_closeButton, 1);
        return header;
    }

    private Control CreateBodyHost()
    {
        var host = new ContentControl { Content = _body, VerticalContentAlignment = VerticalAlignment.Top };
        Grid.SetRow(host, 2);
        return host;
    }

    private void UpdateView()
    {
        var entry = Entry;
        var preview = Preview;
        var isLoading = IsLoading;
        var errorMessage = ErrorMessage;
        PseudoClasses.Set(":loading", isLoading);
        PseudoClasses.Set(":has-preview", preview is not null);
        PseudoClasses.Set(":has-error", !string.IsNullOrWhiteSpace(errorMessage) || !string.IsNullOrWhiteSpace(preview?.Error));
        _title.Text = preview?.Title ?? entry?.Name ?? "Preview";
        _description.Text = preview?.Description ?? (entry is not null
            ? string.Join(" · ", new[] { entry.IsDirectory ? "Folder" : entry.ContentType ?? entry.Extension, entry.DisplaySize }.Where(part => !string.IsNullOrWhiteSpace(part)))
            : string.Empty);
        _description.IsVisible = !string.IsNullOrWhiteSpace(_description.Text);
        _closeButton.IsVisible = entry is not null;
        _body.Children.Clear();

        if (isLoading)
        {
            _body.Children.Add(new MdLoadingIndicator { Size = 40, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(12) });
            return;
        }

        var error = errorMessage ?? preview?.Error;
        if (!string.IsNullOrWhiteSpace(error))
        {
            _body.Children.Add(new MdText { Text = error, StyleRole = MdTextStyle.BodyMedium, TextWrapping = TextWrapping.Wrap });
            return;
        }

        if (preview?.Image is { } image)
        {
            _body.Children.Add(new Border
            {
                MinHeight = 120,
                MaxHeight = 460,
                CornerRadius = new CornerRadius(12),
                ClipToBounds = true,
                Child = new Image { Source = image, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch }
            });
        }
        else if (preview?.Text is { } text)
        {
            _body.Children.Add(new TextBox
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                IsReadOnly = true,
                MaxHeight = 520,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Padding = new Thickness(0),
                FontFamily = FontFamily.Default,
                FontSize = 13
            });
        }
        else if (entry is null)
        {
            _body.Children.Add(_emptyMessage);
        }
        else if (entry.IsDirectory)
        {
            _body.Children.Add(new MdText { Text = "Choose a file to see its preview.", StyleRole = MdTextStyle.BodyMedium, TextWrapping = TextWrapping.Wrap });
        }
        else
        {
            _body.Children.Add(new MdText
            {
                Text = preview?.Description ?? entry.ContentType ?? entry.Extension,
                StyleRole = MdTextStyle.BodyMedium,
                TextWrapping = TextWrapping.Wrap
            });
        }
    }
}
