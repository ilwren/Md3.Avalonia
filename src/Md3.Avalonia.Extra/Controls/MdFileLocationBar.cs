using System.Collections;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Request to navigate to a location-relative path.</summary>
public sealed class MdFilePathNavigationEventArgs(MdFileLocation? location, string relativePath) : EventArgs
{
    public MdFileLocation? Location { get; } = location;
    public string RelativePath { get; } = relativePath;
}

/// <summary>
/// Material path navigation for a file browser. It combines a location switcher, an up action,
/// collapsible breadcrumbs and an editable relative-path field; it does not perform filesystem I/O.
/// </summary>
public sealed class MdFileLocationBar : UserControl
{
    public static readonly StyledProperty<IEnumerable?> LocationsProperty =
        AvaloniaProperty.Register<MdFileLocationBar, IEnumerable?>(nameof(Locations));
    public static readonly StyledProperty<MdFileLocation?> LocationProperty =
        AvaloniaProperty.Register<MdFileLocationBar, MdFileLocation?>(nameof(Location), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<string> RelativePathProperty =
        AvaloniaProperty.Register<MdFileLocationBar, string>(nameof(RelativePath), string.Empty, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<MdFileLocationBar, bool>(nameof(IsEditing));

    private readonly MdComboBox _locationPicker;
    private readonly MdIconButton _upButton;
    private readonly MdBreadcrumb _breadcrumb;
    private readonly MdTextBox _pathEditor;
    private readonly MdIconButton _editButton;
    private bool _syncingControls;

    static MdFileLocationBar()
    {
        LocationsProperty.Changed.AddClassHandler<MdFileLocationBar>((bar, _) => bar.UpdateLocations());
        LocationProperty.Changed.AddClassHandler<MdFileLocationBar>((bar, _) => bar.UpdateVisuals());
        RelativePathProperty.Changed.AddClassHandler<MdFileLocationBar>((bar, _) => bar.UpdateVisuals());
        IsEditingProperty.Changed.AddClassHandler<MdFileLocationBar>((bar, _) => bar.UpdateVisuals());
    }

    public MdFileLocationBar()
    {
        _locationPicker = new MdComboBox
        {
            MinWidth = 132,
            MaxWidth = 220,
            Label = "Location",
            ItemTemplate = new FuncDataTemplate<MdFileLocation>((location, _) => CreateLocationItem(location))
        };
        _locationPicker.SelectionChanged += OnLocationSelectionChanged;

        _upButton = new MdIconButton
        {
            Icon = "↑",
            Variant = MdIconButtonVariant.Standard
        };
        ToolTip.SetTip(_upButton, "Go up one folder");
        AutomationProperties.SetName(_upButton, "Go up one folder");
        _upButton.Click += (_, _) => NavigateUpRequested?.Invoke(this, EventArgs.Empty);

        _breadcrumb = new MdBreadcrumb
        {
            OverflowBehavior = MdBreadcrumbOverflowBehavior.Collapse,
            MaxDisplayedItems = 5,
            ItemsBeforeCollapse = 1,
            ItemsAfterCollapse = 2,
            VerticalAlignment = VerticalAlignment.Center
        };
        _breadcrumb.ItemInvoked += OnBreadcrumbInvoked;

        _pathEditor = new MdTextBox
        {
            Variant = MdTextBoxVariant.Outlined,
            Watermark = "Enter a path relative to this location",
            MinHeight = 40,
            VerticalAlignment = VerticalAlignment.Center
        };
        _pathEditor.KeyDown += OnPathEditorKeyDown;

        _editButton = new MdIconButton
        {
            Icon = "✎",
            Variant = MdIconButtonVariant.Standard
        };
        ToolTip.SetTip(_editButton, "Edit path or confirm the current path");
        AutomationProperties.SetName(_editButton, "Edit path");
        _editButton.Click += (_, _) =>
        {
            if (IsEditing) CommitEdit();
            else BeginEdit();
        };

        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*,Auto"),
            ColumnSpacing = 4,
            MinHeight = 48,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(_locationPicker, 0);
        Grid.SetColumn(_upButton, 1);
        Grid.SetColumn(_breadcrumb, 2);
        Grid.SetColumn(_pathEditor, 2);
        Grid.SetColumn(_editButton, 3);
        root.Children.Add(_locationPicker);
        root.Children.Add(_upButton);
        root.Children.Add(_breadcrumb);
        root.Children.Add(_pathEditor);
        root.Children.Add(_editButton);
        Content = root;
        UpdateLocations();
        UpdateVisuals();
    }

    public IEnumerable? Locations { get => GetValue(LocationsProperty); set => SetValue(LocationsProperty, value); }
    public MdFileLocation? Location { get => GetValue(LocationProperty); set => SetValue(LocationProperty, value); }
    public string RelativePath { get => GetValue(RelativePathProperty); set => SetValue(RelativePathProperty, value); }
    public bool IsEditing { get => GetValue(IsEditingProperty); set => SetValue(IsEditingProperty, value); }
    public bool CanGoUp => !string.IsNullOrWhiteSpace(RelativePath);

    public event EventHandler? NavigateUpRequested;
    public event EventHandler<MdFilePathNavigationEventArgs>? NavigationRequested;
    public event EventHandler<MdFileLocation>? LocationSelectionRequested;

    public void BeginEdit()
    {
        _pathEditor.ErrorText = null;
        _pathEditor.Text = RelativePath;
        SetCurrentValue(IsEditingProperty, true);
        _pathEditor.Focus(NavigationMethod.Pointer);
        _pathEditor.SelectAll();
    }

    public void CancelEdit()
    {
        _pathEditor.ErrorText = null;
        SetCurrentValue(IsEditingProperty, false);
        _pathEditor.Text = RelativePath;
    }

    public bool CommitEdit()
    {
        var edited = NormalizeRelativePath(_pathEditor.Text ?? string.Empty);
        if (edited is null)
        {
            _pathEditor.ErrorText = "Use a location-relative path without ‘..’ segments.";
            return false;
        }
        _pathEditor.ErrorText = null;
        SetCurrentValue(IsEditingProperty, false);
        NavigationRequested?.Invoke(this, new MdFilePathNavigationEventArgs(Location, edited));
        return true;
    }

    internal static Control CreateLocationItem(MdFileLocation? location)
    {
        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center
        };
        if (location?.Icon is IImage image)
            content.Children.Add(new Image { Source = image, Width = 20, Height = 20, Stretch = Stretch.Uniform });
        else if (location?.Icon is string glyph && glyph.Length > 0)
            content.Children.Add(new MdIcon { Glyph = glyph, Width = 20, Height = 20 });
        content.Children.Add(new MdText
        {
            Text = location?.Name,
            StyleRole = MdTextStyle.BodyLarge,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center
        });
        return content;
    }

    public static string? NormalizeRelativePath(string path)
    {
        var normalized = (path ?? string.Empty).Trim().Replace('\\', '/');
        var hasDrivePrefix = normalized.Length >= 2 && char.IsLetter(normalized[0]) && normalized[1] == ':';
        if (Path.IsPathRooted(normalized) || normalized.StartsWith("//", StringComparison.Ordinal) || hasDrivePrefix ||
            normalized.Split('/').Any(segment => segment == "..")) return null;
        return string.Join("/", normalized.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(segment => segment != "."));
    }

    private void OnLocationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_syncingControls || _locationPicker.SelectedItem is not MdFileLocation location) return;
        SetCurrentValue(LocationProperty, location);
        SetCurrentValue(RelativePathProperty, string.Empty);
        LocationSelectionRequested?.Invoke(this, location);
        NavigationRequested?.Invoke(this, new MdFilePathNavigationEventArgs(location, string.Empty));
    }

    private void OnBreadcrumbInvoked(object? sender, object? item)
    {
        if (item is not MdBreadcrumbItem { CommandParameter: string path }) return;
        NavigationRequested?.Invoke(this, new MdFilePathNavigationEventArgs(Location, path));
    }

    private void OnPathEditorKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = CommitEdit();
        }
        else if (e.Key == Key.Escape)
        {
            CancelEdit();
            e.Handled = true;
        }
    }

    private void UpdateLocations()
    {
        _locationPicker.ItemsSource = Locations;
        _locationPicker.SelectedItem = Location;
        UpdateBreadcrumbs();
    }

    private void UpdateVisuals()
    {
        _syncingControls = true;
        try
        {
            _locationPicker.ItemsSource = Locations;
            _locationPicker.SelectedItem = Location;
            _upButton.IsEnabled = CanGoUp;
            _breadcrumb.IsVisible = !IsEditing;
            _pathEditor.IsVisible = IsEditing;
            _editButton.Icon = IsEditing ? "✓" : "✎";
            AutomationProperties.SetName(_breadcrumb, Location is null ? "Path" : $"Path in {Location.Name}");
            if (!IsEditing) _pathEditor.Text = RelativePath;
            UpdateBreadcrumbs();
        }
        finally { _syncingControls = false; }
    }

    private void UpdateBreadcrumbs()
    {
        var items = new ObservableCollection<MdBreadcrumbItem>();
        var location = Location;
        if (location is not null)
        {
            items.Add(new MdBreadcrumbItem
            {
                Label = location.Name,
                IsCurrent = string.IsNullOrEmpty(RelativePath),
                CommandParameter = string.Empty
            });
            var accumulated = string.Empty;
            foreach (var segment in (RelativePath ?? string.Empty).Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                accumulated = accumulated.Length == 0 ? segment : $"{accumulated}/{segment}";
                items.Add(new MdBreadcrumbItem
                {
                    Label = segment,
                    IsCurrent = string.Equals(accumulated, RelativePath, StringComparison.Ordinal),
                    CommandParameter = accumulated
                });
            }
        }
        _breadcrumb.ItemsSource = items;
    }
}
