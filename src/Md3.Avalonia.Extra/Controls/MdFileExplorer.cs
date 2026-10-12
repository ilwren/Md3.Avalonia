using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Conflict details passed to an explorer host before a conflicting operation is retried.</summary>
public sealed class MdFileConflictEventArgs(MdFileOperationRequest request, string? destinationPath) : EventArgs
{
    public MdFileOperationRequest Request { get; } = request;
    public string? DestinationPath { get; } = destinationPath;
    public MdFileConflictPolicy Policy { get; set; } = MdFileConflictPolicy.Fail;
}

/// <summary>
/// Adaptive Material 3 local/provider-backed file browser. It owns navigation, selection,
/// filtering, sorting, previews, confirmation prompts and operation feedback; storage access is
/// delegated to <see cref="IMdFileSystemProvider"/>.
/// </summary>
public sealed class MdFileExplorer : UserControl
{
    public static readonly StyledProperty<IMdFileSystemProvider?> ProviderProperty =
        AvaloniaProperty.Register<MdFileExplorer, IMdFileSystemProvider?>(nameof(Provider));
    public static readonly StyledProperty<string?> CurrentLocationIdProperty =
        AvaloniaProperty.Register<MdFileExplorer, string?>(nameof(CurrentLocationId), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<string> CurrentPathProperty =
        AvaloniaProperty.Register<MdFileExplorer, string>(nameof(CurrentPath), string.Empty, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<MdFileViewMode> ViewModeProperty =
        AvaloniaProperty.Register<MdFileExplorer, MdFileViewMode>(nameof(ViewMode), MdFileViewMode.List);
    public static readonly StyledProperty<MdFileSortKey> SortKeyProperty =
        AvaloniaProperty.Register<MdFileExplorer, MdFileSortKey>(nameof(SortKey), MdFileSortKey.Name);
    public static readonly StyledProperty<bool> IsSortAscendingProperty =
        AvaloniaProperty.Register<MdFileExplorer, bool>(nameof(IsSortAscending), true);
    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<MdFileExplorer, string?>(nameof(SearchText), string.Empty);
    public static readonly StyledProperty<bool> IncludeHiddenProperty =
        AvaloniaProperty.Register<MdFileExplorer, bool>(nameof(IncludeHidden));
    public static readonly StyledProperty<bool> IsPreviewPaneOpenProperty =
        AvaloniaProperty.Register<MdFileExplorer, bool>(nameof(IsPreviewPaneOpen), true);
    public static readonly StyledProperty<double> CompactBreakpointProperty =
        AvaloniaProperty.Register<MdFileExplorer, double>(nameof(CompactBreakpoint), 1024);
    public static readonly StyledProperty<Func<MdFileEntry, string?>?> GlyphSelectorProperty =
        AvaloniaProperty.Register<MdFileExplorer, Func<MdFileEntry, string?>?>(nameof(GlyphSelector));
    public static readonly DirectProperty<MdFileExplorer, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<MdFileExplorer, bool>(nameof(IsLoading), explorer => explorer.IsLoading);
    public static readonly DirectProperty<MdFileExplorer, string?> StatusMessageProperty =
        AvaloniaProperty.RegisterDirect<MdFileExplorer, string?>(nameof(StatusMessage), explorer => explorer.StatusMessage);
    public static readonly DirectProperty<MdFileExplorer, bool> CanNavigateBackProperty =
        AvaloniaProperty.RegisterDirect<MdFileExplorer, bool>(nameof(CanNavigateBack), explorer => explorer.CanNavigateBack);
    public static readonly DirectProperty<MdFileExplorer, bool> CanNavigateForwardProperty =
        AvaloniaProperty.RegisterDirect<MdFileExplorer, bool>(nameof(CanNavigateForward), explorer => explorer.CanNavigateForward);
    public static readonly DirectProperty<MdFileExplorer, bool> CanNavigateUpProperty =
        AvaloniaProperty.RegisterDirect<MdFileExplorer, bool>(nameof(CanNavigateUp), explorer => explorer.CanNavigateUp);

    private readonly ObservableCollection<MdFileLocation> _locations = [];
    private readonly ObservableCollection<MdFileEntry> _entries = [];
    private readonly HashSet<IImage> _ownedThumbnails = [];
    private readonly MdFileOperationQueue _operations = new();
    private readonly Stack<(string LocationId, string Path)> _backStack = new();
    private readonly Stack<(string LocationId, string Path)> _forwardStack = new();
    private readonly CancellationTokenSource _lifetime = new();
    private readonly MdDialogHost _dialogHost;
    private readonly MdSurface _surface;
    private readonly MdToolbar _toolbar;
    private readonly MdFileLocationBar _locationBar;
    private readonly MdList _locationsList;
    private readonly MdFileItemsView _itemsView;
    private readonly MdFilePreviewPane _previewPane;
    private readonly MdFileSelectionToolbar _selectionToolbar;
    private readonly MdFileOperationCenter _operationCenter;
    private readonly MdLinearProgressIndicator _loadingIndicator;
    private readonly MdFileContextMenu _contextMenu;
    private readonly TextBox _searchBox;
    private readonly CheckBox _showHidden;
    private readonly Grid _mainGrid;
    private readonly Control _sidebar;
    private readonly Control _fileRegion;
    private readonly Grid _detailsHeader;
    private readonly MdText _statusText;
    private MdIconButton _backButton = null!;
    private MdIconButton _forwardButton = null!;
    private MdIconButton _upButton = null!;
    private MdIconButton _refreshButton = null!;
    private MdButton _newFolderButton = null!;
    private CancellationTokenSource? _navigationCancellation;
    private CancellationTokenSource? _previewCancellation;
    private CancellationTokenSource? _thumbnailCancellation;
    private bool _syncingNavigation;
    private bool _syncingSearch;
    private bool _isCompactLayout;
    private bool _isLoading;
    private bool _canNavigateBack;
    private bool _canNavigateForward;
    private bool _canNavigateUp;
    private string? _statusMessage;

    static MdFileExplorer()
    {
        ProviderProperty.Changed.AddClassHandler<MdFileExplorer>((explorer, _) => explorer.OnProviderChanged());
        CurrentLocationIdProperty.Changed.AddClassHandler<MdFileExplorer>((explorer, _) => explorer.OnNavigationPropertyChanged());
        CurrentPathProperty.Changed.AddClassHandler<MdFileExplorer>((explorer, _) => explorer.OnNavigationPropertyChanged());
        ViewModeProperty.Changed.AddClassHandler<MdFileExplorer>((explorer, _) => explorer.UpdateViewMode());
        SortKeyProperty.Changed.AddClassHandler<MdFileExplorer>((explorer, _) =>
        {
            explorer.ApplyFilterAndSort();
            explorer.UpdateDetailsHeader();
        });
        IsSortAscendingProperty.Changed.AddClassHandler<MdFileExplorer>((explorer, _) =>
        {
            explorer.ApplyFilterAndSort();
            explorer.UpdateDetailsHeader();
        });
        SearchTextProperty.Changed.AddClassHandler<MdFileExplorer>((explorer, _) =>
        {
            explorer.SyncSearchText();
            explorer.ApplyFilterAndSort();
        });
        IncludeHiddenProperty.Changed.AddClassHandler<MdFileExplorer>((explorer, args) =>
        {
            explorer.SyncHiddenItems();
            _ = explorer.ReloadCurrentDirectory();
        });
        IsPreviewPaneOpenProperty.Changed.AddClassHandler<MdFileExplorer>((explorer, args) =>
        {
            explorer.UpdateResponsiveLayout();
            _ = explorer.UpdatePreviewForSelectionAsync();
        });
        CompactBreakpointProperty.Changed.AddClassHandler<MdFileExplorer>((explorer, _) => explorer.UpdateResponsiveLayout());
        GlyphSelectorProperty.Changed.AddClassHandler<MdFileExplorer>((explorer, _) => explorer.UpdateGlyphSelector());
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (double.IsFinite(availableSize.Width) && availableSize.Width > 0 &&
            (availableSize.Width < CompactBreakpoint) != _isCompactLayout)
            UpdateResponsiveLayout(availableSize.Width);
        return base.MeasureOverride(availableSize);
    }

    public MdFileExplorer()
    {
        _toolbar = new MdToolbar
        {
            Mode = MdToolbarMode.Docked,
            Density = MdToolbarDensity.Compact,
            Variant = MdToolbarVariant.Standard,
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        AddToolbarActions();

        _locationBar = new MdFileLocationBar { Locations = _locations };
        _locationBar.NavigateUpRequested += (_, _) => _ = NavigateUpAsync();
        _locationBar.NavigationRequested += (_, args) =>
        {
            if (args.Location is not null) _ = NavigateToAsync(args.Location.Id, args.RelativePath);
        };

        _locationsList = new MdList
        {
            ItemsSource = _locations,
            SelectionMode = SelectionMode.Single,
            ItemTemplate = new FuncDataTemplate<MdFileLocation>((location, _) => MdFileLocationBar.CreateLocationItem(location))
        };
        _locationsList.SelectionChanged += (_, _) =>
        {
            if (!_syncingNavigation && _locationsList.SelectedItem is MdFileLocation location)
                _ = NavigateToAsync(location.Id, string.Empty);
        };
        AutomationProperties.SetName(_locationsList, "File locations");

        _itemsView = new MdFileItemsView { ViewMode = ViewMode };
        _itemsView.SelectionChanged += OnFileSelectionChanged;
        _itemsView.ItemActivated += (_, entry) => _ = OpenEntryAsync(entry);
        _itemsView.ItemContextRequested += (_, entry) => ShowContextMenu(entry);
        _itemsView.RenameRequested += (_, _) => _ = RenameSelectedAsync();
        _itemsView.DeleteRequested += (_, _) => _ = DeleteSelectedAsync();

        _previewPane = new MdFilePreviewPane();
        _previewPane.CloseRequested += (_, _) => SetCurrentValue(IsPreviewPaneOpenProperty, false);
        _previewPane.OpenRequested += (_, _) =>
        {
            if (_previewPane.Entry is { } entry) _ = OpenEntryAsync(entry);
        };

        _selectionToolbar = new MdFileSelectionToolbar();
        _selectionToolbar.ClearRequested += (_, _) => _itemsView.EndSelectionMode();
        _selectionToolbar.ActionInvoked += OnSelectionActionInvoked;

        _operationCenter = new MdFileOperationCenter { Queue = _operations, IsVisible = false };
        _operations.CollectionChanged += OnOperationsChanged;
        _loadingIndicator = new MdLinearProgressIndicator { Height = 4, IsIndeterminate = false, IsVisible = false };

        _contextMenu = new MdFileContextMenu();
        _contextMenu.ActionInvoked += OnContextActionInvoked;

        _searchBox = new MdTextBox
        {
            Variant = MdTextBoxVariant.Outlined,
            Label = "Search files",
            PlaceholderText = "Search this folder",
            ShowClearButton = true,
            MinHeight = 48,
            VerticalAlignment = VerticalAlignment.Center
        };
        _searchBox.TextChanged += (_, _) =>
        {
            if (!_syncingSearch) SetCurrentValue(SearchTextProperty, _searchBox.Text ?? string.Empty);
        };
        _showHidden = new CheckBox { Content = "Show hidden", IsChecked = IncludeHidden, VerticalAlignment = VerticalAlignment.Center };
        _showHidden.IsCheckedChanged += (_, _) => SetCurrentValue(IncludeHiddenProperty, _showHidden.IsChecked == true);

        var browseBar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto"),
            ColumnSpacing = 8,
            Children = { _searchBox, _showHidden }
        };
        var listButton = CreateIconButton("☷", "List view", () => SetCurrentValue(ViewModeProperty, MdFileViewMode.List));
        var gridButton = CreateIconButton("▦", "Grid view", () => SetCurrentValue(ViewModeProperty, MdFileViewMode.Grid));
        var detailsButton = CreateIconButton("☰", "Details view", () => SetCurrentValue(ViewModeProperty, MdFileViewMode.Details));
        Grid.SetColumn(_showHidden, 1);
        Grid.SetColumn(listButton, 2);
        Grid.SetColumn(gridButton, 3);
        // Keep details alongside the other view toggles without turning the search field into a
        // toolbar-sized control on phones.
        browseBar.ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto,Auto");
        Grid.SetColumn(detailsButton, 4);
        browseBar.Children.Add(listButton);
        browseBar.Children.Add(gridButton);
        browseBar.Children.Add(detailsButton);

        _selectionToolbar.SelectedCount = 0;
        _detailsHeader = CreateDetailsHeader();
        var fileList = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            Children = { _detailsHeader, _itemsView, _contextMenu }
        };
        _contextMenu.Width = 0;
        _contextMenu.Height = 0;
        _contextMenu.HorizontalAlignment = HorizontalAlignment.Left;
        _contextMenu.VerticalAlignment = VerticalAlignment.Top;
        Grid.SetRow(_itemsView, 1);
        _fileRegion = new MdSurface
        {
            Variant = MdSurfaceVariant.Surface,
            Padding = new Thickness(4),
            Content = fileList
        };
        _sidebar = new MdSurface
        {
            Variant = MdSurfaceVariant.ContainerLow,
            Padding = new Thickness(8),
            Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new MdText { Text = "Locations", StyleRole = MdTextStyle.TitleMedium, Margin = new Thickness(8, 8, 8, 0) },
                    _locationsList
                }
            }
        };
        _mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("208,*,300"),
            RowDefinitions = new RowDefinitions("*,Auto"),
            ColumnSpacing = 8,
            RowSpacing = 8,
            Children = { _sidebar, _fileRegion, _previewPane }
        };
        Grid.SetColumn(_fileRegion, 1);
        Grid.SetColumn(_previewPane, 2);

        _statusText = new MdText { StyleRole = MdTextStyle.BodySmall, Margin = new Thickness(8, 4), IsVisible = false };
        var toolbarViewport = new MdScrollViewer
        {
            HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = _toolbar
        };
        var shell = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,*,Auto,Auto"),
            RowSpacing = 8,
            Margin = new Thickness(12),
            Children = { toolbarViewport, _locationBar, browseBar, _loadingIndicator, _selectionToolbar, _mainGrid, _statusText, _operationCenter }
        };
        Grid.SetRow(_locationBar, 1);
        Grid.SetRow(browseBar, 2);
        Grid.SetRow(_loadingIndicator, 3);
        Grid.SetRow(_selectionToolbar, 4);
        Grid.SetRow(_mainGrid, 5);
        Grid.SetRow(_statusText, 6);
        Grid.SetRow(_operationCenter, 7);
        _surface = new MdSurface { Variant = MdSurfaceVariant.Surface, Content = shell };
        _dialogHost = new MdDialogHost { Content = _surface };
        Content = _dialogHost;
        SizeChanged += (_, _) => UpdateResponsiveLayout();
        UpdateViewMode();
        UpdateResponsiveLayout();
        UpdateStatus("Choose a provider to begin browsing.");
    }

    public IMdFileSystemProvider? Provider { get => GetValue(ProviderProperty); set => SetValue(ProviderProperty, value); }
    public string? CurrentLocationId { get => GetValue(CurrentLocationIdProperty); set => SetValue(CurrentLocationIdProperty, value); }
    public string CurrentPath { get => GetValue(CurrentPathProperty); set => SetValue(CurrentPathProperty, value ?? string.Empty); }
    public MdFileViewMode ViewMode { get => GetValue(ViewModeProperty); set => SetValue(ViewModeProperty, value); }
    public MdFileSortKey SortKey { get => GetValue(SortKeyProperty); set => SetValue(SortKeyProperty, value); }
    public bool IsSortAscending { get => GetValue(IsSortAscendingProperty); set => SetValue(IsSortAscendingProperty, value); }
    public string? SearchText { get => GetValue(SearchTextProperty); set => SetValue(SearchTextProperty, value); }
    public bool IncludeHidden { get => GetValue(IncludeHiddenProperty); set => SetValue(IncludeHiddenProperty, value); }
    public bool IsPreviewPaneOpen { get => GetValue(IsPreviewPaneOpenProperty); set => SetValue(IsPreviewPaneOpenProperty, value); }
    public double CompactBreakpoint { get => GetValue(CompactBreakpointProperty); set => SetValue(CompactBreakpointProperty, value); }
    public Func<MdFileEntry, string?>? GlyphSelector { get => GetValue(GlyphSelectorProperty); set => SetValue(GlyphSelectorProperty, value); }
    public bool IsLoading { get => _isLoading; private set => SetAndRaise(IsLoadingProperty, ref _isLoading, value); }
    public string? StatusMessage { get => _statusMessage; private set => SetAndRaise(StatusMessageProperty, ref _statusMessage, value); }
    /// <summary>Gets whether a previous location is available in navigation history.</summary>
    public bool CanNavigateBack { get => _canNavigateBack; private set => SetAndRaise(CanNavigateBackProperty, ref _canNavigateBack, value); }
    /// <summary>Gets whether a forward location is available in navigation history.</summary>
    public bool CanNavigateForward { get => _canNavigateForward; private set => SetAndRaise(CanNavigateForwardProperty, ref _canNavigateForward, value); }
    /// <summary>Gets whether the current folder has a parent within its configured location.</summary>
    public bool CanNavigateUp { get => _canNavigateUp; private set => SetAndRaise(CanNavigateUpProperty, ref _canNavigateUp, value); }
    public ObservableCollection<MdFileLocation> Locations => _locations;
    public ObservableCollection<MdFileEntry> Entries => _entries;
    public MdFileOperationQueue OperationQueue => _operations;
    public IReadOnlyList<MdFileEntry> SelectedEntries => _itemsView.SelectedEntries;
    public MdFileLocation? CurrentLocation => _locations.FirstOrDefault(location => location.Id == CurrentLocationId);

    /// <summary>Request a policy from the host before showing the stock Material conflict dialog.</summary>
    public event EventHandler<MdFileConflictEventArgs>? ConflictRequested;
    public event EventHandler<MdFileEntry>? EntryOpened;
    public event EventHandler<MdFileActionInvokedEventArgs>? FileActionRequested;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var provider = Provider;
        if (provider is null)
        {
            UpdateStatus("Choose a file-system provider to browse locations.");
            return;
        }
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.Token);
        try
        {
            var locations = await provider.GetLocationsAsync(linked.Token);
            if (!ReferenceEquals(Provider, provider)) return;
            _locations.Clear();
            foreach (var location in locations) _locations.Add(location);
            if (_locations.Count == 0)
            {
                SetCurrentLocationAndPath(null, string.Empty);
                ClearEntries();
                ApplyFilterAndSort();
                _previewPane.Entry = null;
                SetPreview(null);
                UpdateStatus("No locations are available from this provider.");
                return;
            }
            var selected = _locations.FirstOrDefault(location => location.Id == CurrentLocationId) ?? _locations[0];
            await NavigateToAsync(selected.Id, CurrentPath, addHistory: false, linked.Token);
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested) { }
        catch (Exception exception)
        {
            if (ReferenceEquals(Provider, provider)) UpdateStatus($"Could not load locations: {exception.Message}");
        }
    }

    public async Task NavigateToAsync(
        string locationId,
        string relativePath = "",
        bool addHistory = true,
        CancellationToken cancellationToken = default)
    {
        var provider = Provider;
        if (provider is null)
        {
            UpdateStatus("No file-system provider is configured.");
            return;
        }
        var normalized = MdFileLocationBar.NormalizeRelativePath(relativePath);
        if (normalized is null)
        {
            UpdateStatus("Enter a path inside the selected location.");
            return;
        }
        var location = _locations.FirstOrDefault(candidate => candidate.Id == locationId);
        if (location is null)
        {
            UpdateStatus($"Unknown location '{locationId}'.");
            return;
        }

        if (addHistory && CurrentLocationId is { } previousLocation &&
            (previousLocation != locationId || !string.Equals(CurrentPath, normalized, StringComparison.Ordinal)))
        {
            _backStack.Push((previousLocation, CurrentPath));
            _forwardStack.Clear();
            UpdateNavigationCapabilities();
        }

        var previous = _navigationCancellation;
        var operation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.Token);
        _navigationCancellation = operation;
        _contextMenu.Dismiss();
        previous?.Cancel();
        _thumbnailCancellation?.Cancel();
        var previousPreview = _previewCancellation;
        _previewCancellation = null;
        previousPreview?.Cancel();
        _previewPane.IsLoading = false;
        _previewPane.Entry = null;
        _previewPane.ErrorMessage = null;
        SetPreview(null);
        ClearEntries();
        _itemsView.ClearSelection();
        ApplyFilterAndSort();
        SetCurrentLocationAndPath(locationId, normalized);
        SetLoading(true);
        UpdateStatus($"Loading {location.Name}…");
        try
        {
            var entries = await provider.GetEntriesAsync(locationId, normalized, IncludeHidden, operation.Token);
            if (!ReferenceEquals(_navigationCancellation, operation) || !ReferenceEquals(Provider, provider)) return;
            ClearEntries();
            foreach (var entry in entries) _entries.Add(entry);
            ApplyFilterAndSort();
            _itemsView.ClearSelection();
            await UpdatePreviewForSelectionAsync();
            if (ViewMode == MdFileViewMode.Grid) _ = LoadGridThumbnailsAsync();
            UpdateStatus($"{_entries.Count} items");
        }
        catch (OperationCanceledException) when (operation.IsCancellationRequested)
        {
            if (ReferenceEquals(_navigationCancellation, operation)) UpdateStatus("Navigation cancelled.");
        }
        catch (Exception exception)
        {
            if (ReferenceEquals(_navigationCancellation, operation) && ReferenceEquals(Provider, provider))
            {
                ClearEntries();
                ApplyFilterAndSort();
                _previewPane.Entry = null;
                SetPreview(null);
                UpdateStatus($"Could not open this folder: {exception.Message}");
            }
        }
        finally
        {
            if (ReferenceEquals(_navigationCancellation, operation))
            {
                SetLoading(false);
                _navigationCancellation = null;
            }
            operation.Dispose();
        }
    }

    public async Task<bool> NavigateUpAsync(CancellationToken cancellationToken = default)
    {
        var locationId = CurrentLocationId;
        if (string.IsNullOrWhiteSpace(locationId) || string.IsNullOrEmpty(CurrentPath)) return false;
        var parent = CurrentPath.TrimEnd('/');
        var separator = parent.LastIndexOf('/');
        parent = separator < 0 ? string.Empty : parent[..separator];
        await NavigateToAsync(locationId, parent, cancellationToken: cancellationToken);
        return true;
    }

    public async Task<bool> NavigateBackAsync(CancellationToken cancellationToken = default)
    {
        if (_backStack.Count == 0) return false;
        var previous = _backStack.Pop();
        if (CurrentLocationId is { } currentLocation)
            _forwardStack.Push((currentLocation, CurrentPath));
        UpdateNavigationCapabilities();
        await NavigateToAsync(previous.LocationId, previous.Path, addHistory: false, cancellationToken);
        return true;
    }

    /// <summary>Navigates forward to the next location in the explorer's history, if available.</summary>
    public async Task<bool> NavigateForwardAsync(CancellationToken cancellationToken = default)
    {
        if (_forwardStack.Count == 0) return false;
        var next = _forwardStack.Pop();
        if (CurrentLocationId is { } currentLocation)
            _backStack.Push((currentLocation, CurrentPath));
        UpdateNavigationCapabilities();
        await NavigateToAsync(next.LocationId, next.Path, addHistory: false, cancellationToken);
        return true;
    }

    public async Task OpenEntryAsync(MdFileEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.CanNavigate && CurrentLocationId is { } locationId)
            await NavigateToAsync(locationId, entry.RelativePath, cancellationToken: cancellationToken);
        else if (entry.IsSymbolicLink)
            UpdateStatus("Symbolic links are not opened by this explorer.");
        else if (!entry.IsDirectory)
            EntryOpened?.Invoke(this, entry);
    }

    public Task<MdFileOperationResult> CreateDirectoryAsync(string name, CancellationToken cancellationToken = default) =>
        ExecuteOperationAsync(new MdFileOperationRequest(MdFileOperationKind.CreateDirectory,
            CurrentLocationId ?? string.Empty, CurrentPath, NewName: name), cancellationToken);

    public Task<MdFileOperationResult> RenameAsync(MdFileEntry entry, string newName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.IsReadOnly) return Task.FromResult(MdFileOperationResult.Failure("This item is read-only."));
        return ExecuteOperationAsync(new MdFileOperationRequest(MdFileOperationKind.Rename,
            CurrentLocationId ?? string.Empty, entry.RelativePath, NewName: newName), cancellationToken);
    }

    public async Task DeleteSelectedAsync(CancellationToken cancellationToken = default)
    {
        var selected = SelectedEntries;
        var provider = Provider;
        var locationId = CurrentLocationId;
        var sourcePath = CurrentPath;
        if (selected.Count == 0 || provider is null || string.IsNullOrWhiteSpace(locationId)) return;
        if (selected.Any(entry => entry.IsReadOnly))
        {
            UpdateStatus("Read-only items cannot be deleted from this selection.");
            return;
        }
        var title = selected.Count == 1 ? $"Delete ‘{selected[0].Name}’?" : $"Delete {selected.Count} items?";
        var body = selected.Count == 1
            ? "This item will be permanently deleted. This action cannot be undone."
            : "These items will be permanently deleted. This action cannot be undone.";
        if (await ShowChoiceDialogAsync(title, body, new("Cancel", null, MdButtonVariant.Text), new("Delete", true, MdButtonVariant.Filled)) is not true ||
            !IsCurrentExplorerContext(provider, locationId, sourcePath))
            return;

        foreach (var entry in selected)
        {
            if (!IsCurrentExplorerContext(provider, locationId, sourcePath)) return;
            var result = await ExecuteOperationAsync(new MdFileOperationRequest(MdFileOperationKind.Delete,
                locationId, entry.RelativePath), cancellationToken);
            if (!result.Succeeded && !result.WasSkipped) break;
        }
        if (IsCurrentExplorerContext(provider, locationId, sourcePath))
        {
            _itemsView.ClearSelection();
            await ReloadCurrentDirectory(cancellationToken);
        }
    }

    public async Task CopySelectedAsync(CancellationToken cancellationToken = default) =>
        await CopyOrMoveSelectedAsync(MdFileOperationKind.Copy, cancellationToken);

    public async Task MoveSelectedAsync(CancellationToken cancellationToken = default) =>
        await CopyOrMoveSelectedAsync(MdFileOperationKind.Move, cancellationToken);

    public void SortBy(MdFileSortKey sortKey)
    {
        if (SortKey == sortKey) SetCurrentValue(IsSortAscendingProperty, !IsSortAscending);
        else
        {
            SetCurrentValue(SortKeyProperty, sortKey);
            SetCurrentValue(IsSortAscendingProperty, true);
        }
    }

    public void SetViewMode(MdFileViewMode mode) => SetCurrentValue(ViewModeProperty, mode);

    private void AddToolbarActions()
    {
        _backButton = CreateIconButton("←", "Go back", () => _ = NavigateBackAsync());
        _forwardButton = CreateIconButton("→", "Go forward", () => _ = NavigateForwardAsync());
        _upButton = CreateIconButton("↑", "Go up one folder", () => _ = NavigateUpAsync());
        _refreshButton = CreateIconButton("↻", "Refresh", () => _ = ReloadCurrentDirectory());
        _newFolderButton = new MdButton { Content = "New folder", Variant = MdButtonVariant.Tonal, Margin = new Thickness(4, 0) };
        _newFolderButton.Click += (_, _) => _ = PromptCreateDirectoryAsync();
        var preview = CreateIconButton("▣", "Toggle preview pane", () => SetCurrentValue(IsPreviewPaneOpenProperty, !IsPreviewPaneOpen));
        _toolbar.Items.Add(_backButton);
        _toolbar.Items.Add(_forwardButton);
        _toolbar.Items.Add(_upButton);
        _toolbar.Items.Add(_refreshButton);
        _toolbar.Items.Add(_newFolderButton);
        _toolbar.Items.Add(preview);
        UpdateNavigationCapabilities();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!e.Handled)
        {
            var modifiers = e.KeyModifiers;
            var command = (modifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0;
            var alt = (modifiers & KeyModifiers.Alt) != 0;
            if (command && e.Key == Key.L)
            {
                _locationBar.BeginEdit();
                e.Handled = true;
            }
            else if (command && e.Key == Key.F)
            {
                _searchBox.Focus();
                _searchBox.SelectAll();
                e.Handled = true;
            }
            else if (alt && e.Key == Key.Left)
            {
                _ = NavigateBackAsync();
                e.Handled = true;
            }
            else if (alt && e.Key == Key.Right)
            {
                _ = NavigateForwardAsync();
                e.Handled = true;
            }
            else if (alt && e.Key == Key.Up)
            {
                _ = NavigateUpAsync();
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                _ = ReloadCurrentDirectory();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && _searchBox.IsFocused && !string.IsNullOrEmpty(SearchText))
            {
                SetCurrentValue(SearchTextProperty, string.Empty);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && (_itemsView.IsSelectionMode || SelectedEntries.Count > 0))
            {
                _itemsView.EndSelectionMode();
                e.Handled = true;
            }
        }

        if (!e.Handled) base.OnKeyDown(e);
    }

    private static MdIconButton CreateIconButton(string icon, string description, Action action)
    {
        var button = new MdIconButton { Icon = icon, Variant = MdIconButtonVariant.Standard };
        ToolTip.SetTip(button, description);
        AutomationProperties.SetName(button, description);
        button.Click += (_, _) => action();
        return button;
    }

    private Grid CreateDetailsHeader()
    {
        var header = new Grid
        {
            ColumnSpacing = 12,
            Margin = new Thickness(8, 4),
            MinHeight = 40
        };
        ConfigureDetailsHeader(header);
        return header;
    }

    private void UpdateDetailsHeader()
    {
        if (_detailsHeader is not null) ConfigureDetailsHeader(_detailsHeader);
    }

    private void ConfigureDetailsHeader(Grid header)
    {
        header.Children.Clear();
        header.ColumnDefinitions = _isCompactLayout
            ? new ColumnDefinitions("40,*,88")
            : new ColumnDefinitions("40,*,120,84,136");
        AddSortButton(header, "Name", 1, MdFileSortKey.Name);
        if (_isCompactLayout)
        {
            AddSortButton(header, "Size", 2, MdFileSortKey.Size);
            return;
        }
        AddSortButton(header, "Type", 2, MdFileSortKey.Kind);
        AddSortButton(header, "Size", 3, MdFileSortKey.Size);
        AddSortButton(header, "Modified", 4, MdFileSortKey.Modified);
    }

    private void AddSortButton(Grid header, string label, int column, MdFileSortKey key)
    {
        var isActive = SortKey == key;
        var direction = IsSortAscending ? "ascending" : "descending";
        var content = isActive ? $"{label} {(IsSortAscending ? "↑" : "↓")}" : label;
        var button = new MdButton { Content = content, Variant = MdButtonVariant.Text, HorizontalAlignment = HorizontalAlignment.Left };
        ToolTip.SetTip(button, isActive ? $"Sorted by {label}, {direction}" : $"Sort by {label}");
        AutomationProperties.SetName(button, isActive ? $"{label}, sorted {direction}" : $"Sort by {label}");
        button.Click += (_, _) => SortBy(key);
        Grid.SetColumn(button, column);
        header.Children.Add(button);
    }

    private void UpdateViewMode()
    {
        if (_itemsView is null) return;
        _itemsView.ViewMode = ViewMode;
        _detailsHeader.IsVisible = ViewMode == MdFileViewMode.Details;
        if (ViewMode == MdFileViewMode.Grid) _ = LoadGridThumbnailsAsync();
        else _thumbnailCancellation?.Cancel();
    }

    private async Task LoadGridThumbnailsAsync()
    {
        var provider = Provider;
        if (provider is not IMdFileThumbnailProvider thumbnailProvider || ViewMode != MdFileViewMode.Grid)
            return;
        var previous = _thumbnailCancellation;
        var operation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        _thumbnailCancellation = operation;
        previous?.Cancel();
        var candidates = _entries
            .Where(entry => !entry.IsDirectory && !entry.IsSymbolicLink && entry.Thumbnail is null &&
                (entry.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true ||
                 new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp" }.Contains(entry.Extension, StringComparer.OrdinalIgnoreCase)))
            .Take(24)
            .ToArray();
        if (candidates.Length == 0)
        {
            if (ReferenceEquals(_thumbnailCancellation, operation)) _thumbnailCancellation = null;
            operation.Dispose();
            return;
        }
        try
        {
            foreach (var entry in candidates)
            {
                operation.Token.ThrowIfCancellationRequested();
                var thumbnail = await thumbnailProvider.GetThumbnailAsync(CurrentLocationId ?? string.Empty, entry, operation.Token);
                if (thumbnail is null) continue;
                if (operation.IsCancellationRequested || !ReferenceEquals(Provider, provider))
                {
                    DisposeImage(thumbnail);
                    operation.Token.ThrowIfCancellationRequested();
                    return;
                }
                var index = _entries.IndexOf(entry);
                if (index >= 0 && _entries[index].Thumbnail is null)
                {
                    _ownedThumbnails.Add(thumbnail);
                    _entries[index] = entry with { Thumbnail = thumbnail };
                }
                else
                {
                    DisposeImage(thumbnail);
                }
            }
            if (ReferenceEquals(_thumbnailCancellation, operation) && ReferenceEquals(Provider, provider)) RefreshItemsViewPreservingSelection();
        }
        catch (OperationCanceledException) when (operation.IsCancellationRequested) { }
        catch (Exception exception)
        {
            if (ReferenceEquals(_thumbnailCancellation, operation) && ReferenceEquals(Provider, provider))
            {
                RefreshItemsViewPreservingSelection();
                UpdateStatus($"Some thumbnails could not be loaded: {exception.Message}");
            }
        }
        finally
        {
            if (ReferenceEquals(_thumbnailCancellation, operation)) _thumbnailCancellation = null;
            operation.Dispose();
        }
    }

    private void RefreshItemsViewPreservingSelection()
    {
        var selectedIds = _itemsView.SelectedEntries.Select(entry => entry.Id).ToHashSet(StringComparer.Ordinal);
        ApplyFilterAndSort();
        if (_itemsView.SelectedItems is not { } selection) return;
        foreach (var entry in _itemsView.Items.OfType<MdFileEntry>().Where(entry => selectedIds.Contains(entry.Id)))
            selection.Add(entry);
    }

    private void UpdateGlyphSelector()
    {
        if (_itemsView is not null) _itemsView.GlyphSelector = GlyphSelector;
    }

    private void UpdateResponsiveLayout(double? availableWidth = null)
    {
        if (_mainGrid is null) return;
        var width = availableWidth ?? Bounds.Width;
        var compact = width > 0 && width < CompactBreakpoint;
        if (compact != _isCompactLayout)
        {
            _isCompactLayout = compact;
            _itemsView.IsCompact = compact;
            UpdateDetailsHeader();
        }
        _sidebar.IsVisible = !compact;
        _mainGrid.ColumnDefinitions = compact
            ? new ColumnDefinitions("0,*,0")
            : new ColumnDefinitions("208,*,300");
        if (compact)
        {
            _mainGrid.RowDefinitions = new RowDefinitions("*,Auto");
            Grid.SetColumn(_previewPane, 1);
            Grid.SetRow(_previewPane, 1);
            _previewPane.MaxHeight = 300;
        }
        else
        {
            _mainGrid.RowDefinitions = new RowDefinitions("*");
            Grid.SetColumn(_previewPane, 2);
            Grid.SetRow(_previewPane, 0);
            _previewPane.MaxHeight = double.PositiveInfinity;
        }
        _previewPane.IsVisible = IsPreviewPaneOpen && (!compact || SelectedEntries.Count > 0);
        _locationBar.MinHeight = compact ? 52 : 48;
        _upButton.IsVisible = !compact;
        _newFolderButton.Content = compact ? "New" : "New folder";
    }

    private void OnProviderChanged()
    {
        var navigation = _navigationCancellation;
        var preview = _previewCancellation;
        var thumbnails = _thumbnailCancellation;
        _navigationCancellation = null;
        _previewCancellation = null;
        _thumbnailCancellation = null;
        _contextMenu.Dismiss();
        navigation?.Cancel();
        preview?.Cancel();
        thumbnails?.Cancel();
        SetLoading(false);
        _locations.Clear();
        ClearEntries();
        _backStack.Clear();
        _forwardStack.Clear();
        _itemsView.ClearSelection();
        SetCurrentLocationAndPath(null, string.Empty);
        ApplyFilterAndSort();
        _previewPane.Entry = null;
        SetPreview(null);
        _previewPane.IsLoading = false;
        _previewPane.ErrorMessage = null;
        if (Provider is null)
        {
            UpdateStatus("Choose a provider to begin browsing.");
            return;
        }
        _ = InitializeAsync();
    }

    private void SyncSearchText()
    {
        if (_searchBox is null) return;
        _syncingSearch = true;
        try { _searchBox.Text = SearchText ?? string.Empty; }
        finally { _syncingSearch = false; }
    }

    private void SyncHiddenItems() => _showHidden.IsChecked = IncludeHidden;

    private void OnNavigationPropertyChanged()
    {
        var locationId = CurrentLocationId;
        if (_syncingNavigation || string.IsNullOrWhiteSpace(locationId)) return;
        _ = NavigateToAsync(locationId, CurrentPath);
    }

    private void SetCurrentLocationAndPath(string? locationId, string path)
    {
        _syncingNavigation = true;
        try
        {
            SetCurrentValue(CurrentLocationIdProperty, locationId);
            SetCurrentValue(CurrentPathProperty, path);
            _locationBar.Location = _locations.FirstOrDefault(location => location.Id == locationId);
            _locationBar.RelativePath = path;
            _locationsList.SelectedItem = _locations.FirstOrDefault(location => location.Id == locationId);
        }
        finally { _syncingNavigation = false; }
        UpdateNavigationCapabilities();
    }

    private void UpdateNavigationCapabilities()
    {
        CanNavigateBack = _backStack.Count > 0;
        CanNavigateForward = _forwardStack.Count > 0;
        CanNavigateUp = !string.IsNullOrWhiteSpace(CurrentLocationId) && !string.IsNullOrEmpty(CurrentPath);
        _backButton.IsEnabled = CanNavigateBack;
        _forwardButton.IsEnabled = CanNavigateForward;
        _upButton.IsEnabled = CanNavigateUp;
        _refreshButton.IsEnabled = !string.IsNullOrWhiteSpace(CurrentLocationId);
    }

    private async Task ReloadCurrentDirectory(CancellationToken cancellationToken = default)
    {
        var locationId = CurrentLocationId;
        if (string.IsNullOrWhiteSpace(locationId)) return;
        await NavigateToAsync(locationId, CurrentPath, addHistory: false, cancellationToken);
    }

    private void ApplyFilterAndSort()
    {
        if (_itemsView is null) return;
        var selectedIds = _itemsView.SelectedEntries.Select(entry => entry.Id).ToHashSet(StringComparer.Ordinal);
        IEnumerable<MdFileEntry> query = _entries;
        var search = SearchText?.Trim();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(entry => entry.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                entry.ContentType?.Contains(search, StringComparison.CurrentCultureIgnoreCase) == true);

        IOrderedEnumerable<MdFileEntry> ordered = query.OrderByDescending(entry => entry.IsDirectory);
        ordered = SortKey switch
        {
            MdFileSortKey.Kind when IsSortAscending => ordered.ThenBy(entry => entry.IsDirectory ? "Folder" : entry.ContentType ?? entry.Extension, StringComparer.CurrentCultureIgnoreCase),
            MdFileSortKey.Kind => ordered.ThenByDescending(entry => entry.IsDirectory ? "Folder" : entry.ContentType ?? entry.Extension, StringComparer.CurrentCultureIgnoreCase),
            MdFileSortKey.Size when IsSortAscending => ordered.ThenBy(entry => entry.SizeBytes ?? -1),
            MdFileSortKey.Size => ordered.ThenByDescending(entry => entry.SizeBytes ?? -1),
            MdFileSortKey.Modified when IsSortAscending => ordered.ThenBy(entry => entry.LastModified ?? DateTimeOffset.MinValue),
            MdFileSortKey.Modified => ordered.ThenByDescending(entry => entry.LastModified ?? DateTimeOffset.MinValue),
            _ when IsSortAscending => ordered.ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase),
            _ => ordered.ThenByDescending(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
        };
        var sortedEntries = ordered.ToArray();
        _itemsView.ItemsSource = sortedEntries;
        if (selectedIds.Count > 0 && _itemsView.SelectedItems is { } selection)
            foreach (var entry in sortedEntries.Where(entry => selectedIds.Contains(entry.Id)))
                selection.Add(entry);
    }

    private async void OnFileSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = SelectedEntries;
        _selectionToolbar.SelectedCount = selected.Count;
        UpdateSelectionActions(selected);
        UpdateResponsiveLayout();
        await UpdatePreviewForSelectionAsync();
    }

    private void UpdateSelectionActions(IReadOnlyList<MdFileEntry> selected)
    {
        _selectionToolbar.Actions.Clear();
        if (selected.Count == 0) return;

        if (selected.Count == 1)
        {
            var entry = selected[0];
            if (entry.CanNavigate || (!entry.IsDirectory && !entry.IsSymbolicLink))
                _selectionToolbar.Actions.Add(new MdFileAction("open", entry.IsDirectory ? "Open folder" : "Open"));
            if (!entry.IsReadOnly)
                _selectionToolbar.Actions.Add(new MdFileAction("rename", "Rename"));
        }

        _selectionToolbar.Actions.Add(new MdFileAction("copy", "Copy to"));
        if (selected.All(entry => !entry.IsReadOnly))
        {
            _selectionToolbar.Actions.Add(new MdFileAction("move", "Move to"));
            _selectionToolbar.Actions.Add(new MdFileAction("delete", "Delete", IsDestructive: true));
        }
    }

    private async Task UpdatePreviewForSelectionAsync()
    {
        var provider = Provider;
        var previous = _previewCancellation;
        var operation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        _previewCancellation = operation;
        previous?.Cancel();
        var selected = SelectedEntries;
        if (provider is null || selected.Count != 1 || !IsPreviewPaneOpen)
        {
            if (ReferenceEquals(_previewCancellation, operation)) _previewCancellation = null;
            operation.Dispose();
            _previewPane.IsLoading = false;
            _previewPane.Entry = null;
            _previewPane.ErrorMessage = null;
            SetPreview(null);
            return;
        }
        var entry = selected[0];
        _previewPane.Entry = entry;
        _previewPane.IsLoading = true;
        _previewPane.ErrorMessage = null;
        try
        {
            var preview = await provider.GetPreviewAsync(CurrentLocationId ?? string.Empty, entry, operation.Token);
            if (ReferenceEquals(_previewCancellation, operation) && ReferenceEquals(Provider, provider))
                SetPreview(preview);
            else if (!ReferenceEquals(_previewPane.Preview?.Image, preview?.Image))
                DisposeImage(preview?.Image);
        }
        catch (OperationCanceledException) when (operation.IsCancellationRequested) { }
        catch (Exception exception)
        {
            if (ReferenceEquals(_previewCancellation, operation) && ReferenceEquals(Provider, provider))
            {
                SetPreview(null);
                _previewPane.ErrorMessage = exception.Message;
            }
        }
        finally
        {
            if (ReferenceEquals(_previewCancellation, operation))
            {
                _previewPane.IsLoading = false;
                _previewCancellation = null;
            }
            operation.Dispose();
        }
    }

    private async Task PromptCreateDirectoryAsync()
    {
        var provider = Provider;
        var locationId = CurrentLocationId;
        var sourcePath = CurrentPath;
        if (provider is null || string.IsNullOrWhiteSpace(locationId)) return;
        var name = await PromptForNameAsync("New folder", "Folder name", "New folder", "Create");
        if (!string.IsNullOrWhiteSpace(name) && IsCurrentExplorerContext(provider, locationId, sourcePath))
            await ExecuteOperationAsync(new MdFileOperationRequest(MdFileOperationKind.CreateDirectory,
                locationId, sourcePath, NewName: name));
    }

    private async Task RenameSelectedAsync()
    {
        var provider = Provider;
        var locationId = CurrentLocationId;
        var sourcePath = CurrentPath;
        var selected = SelectedEntries;
        if (provider is null || string.IsNullOrWhiteSpace(locationId) || selected.Count != 1) return;
        var entry = selected[0];
        if (entry.IsReadOnly)
        {
            UpdateStatus("This item is read-only.");
            return;
        }
        var name = await PromptForNameAsync($"Rename ‘{entry.Name}’", "New name", entry.Name, "Rename");
        if (IsCurrentExplorerContext(provider, locationId, sourcePath) && SelectedEntries.Count == 1 &&
            string.Equals(SelectedEntries[0].Id, entry.Id, StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(name) && !string.Equals(name, entry.Name, StringComparison.Ordinal))
            await ExecuteOperationAsync(new MdFileOperationRequest(MdFileOperationKind.Rename,
                locationId, entry.RelativePath, NewName: name));
    }

    private async Task<string?> PromptForNameAsync(string title, string label, string initialValue, string confirmation)
    {
        var editor = new MdTextBox
        {
            Variant = MdTextBoxVariant.Outlined,
            Label = label,
            Text = initialValue,
            MinWidth = 280,
            MaxWidth = 520
        };
        editor.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) _dialogHost.Close(editor.Text?.Trim());
        };
        var result = await ShowChoiceDialogAsync(title, editor, new("Cancel", null, MdButtonVariant.Text), new(confirmation, null, MdButtonVariant.Filled, () => editor.Text?.Trim()));
        return result as string;
    }

    private async Task CopyOrMoveSelectedAsync(MdFileOperationKind kind, CancellationToken cancellationToken)
    {
        var selected = SelectedEntries;
        var provider = Provider;
        var sourceLocationId = CurrentLocationId;
        var sourcePath = CurrentPath;
        if (selected.Count == 0 || provider is null || string.IsNullOrWhiteSpace(sourceLocationId) || _locations.Count == 0) return;

        var destinationLocation = CurrentLocation ?? _locations[0];
        var destinationPath = CurrentPath;
        var loadVersion = 0;
        using var destinationPickerCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.Token);
        var destinationBar = new MdFileLocationBar
        {
            Locations = _locations,
            Location = destinationLocation,
            RelativePath = destinationPath,
            MinHeight = 48
        };
        var foldersView = new MdFileItemsView
        {
            ViewMode = MdFileViewMode.List,
            IsCompact = true,
            MinHeight = 180,
            MaxHeight = 300
        };
        var folderStatus = new MdText
        {
            StyleRole = MdTextStyle.BodySmall,
            Opacity = 0.76,
            Text = "Loading folders…"
        };
        foldersView.ItemActivated += (_, entry) =>
        {
            if (entry.CanNavigate) _ = LoadDestinationAsync(destinationLocation, entry.RelativePath);
        };
        destinationBar.NavigationRequested += (_, args) =>
        {
            if (args.Location is { } location) _ = LoadDestinationAsync(location, args.RelativePath);
        };
        destinationBar.NavigateUpRequested += (_, _) =>
        {
            if (!string.IsNullOrEmpty(destinationPath))
            {
                var trimmed = destinationPath.TrimEnd('/');
                var separator = trimmed.LastIndexOf('/');
                var parent = separator < 0 ? string.Empty : trimmed[..separator];
                _ = LoadDestinationAsync(destinationLocation, parent);
            }
        };
        var content = new StackPanel
        {
            Spacing = 8,
            MinWidth = 280,
            MaxWidth = 560,
            Children =
            {
                destinationBar,
                new MdText
                {
                    Text = "Tap or double-click a folder to browse. Choose the current folder as the destination.",
                    StyleRole = MdTextStyle.BodySmall,
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = 0.76
                },
                foldersView,
                folderStatus
            }
        };
        var verb = kind == MdFileOperationKind.Copy ? "Copy" : "Move";
        await LoadDestinationAsync(destinationLocation, destinationPath);
        var result = await ShowChoiceDialogAsync($"{verb} {selected.Count} item(s)", content,
            new("Cancel", null, MdButtonVariant.Text), new("Use this folder", true, MdButtonVariant.Filled));
        Interlocked.Increment(ref loadVersion);
        destinationPickerCancellation.Cancel();
        if (result is not true || !IsCurrentExplorerContext(provider, sourceLocationId, sourcePath)) return;
        foreach (var entry in selected)
        {
            if (!IsCurrentExplorerContext(provider, sourceLocationId, sourcePath)) return;
            var operation = new MdFileOperationRequest(kind, sourceLocationId, entry.RelativePath,
                destinationLocation.Id, destinationPath);
            var operationResult = await ExecuteOperationAsync(operation, cancellationToken);
            if (!operationResult.Succeeded && !operationResult.WasSkipped) break;
        }
        if (IsCurrentExplorerContext(provider, sourceLocationId, sourcePath))
            await ReloadCurrentDirectory(cancellationToken);

        async Task LoadDestinationAsync(MdFileLocation location, string requestedPath)
        {
            var normalized = MdFileLocationBar.NormalizeRelativePath(requestedPath);
            if (normalized is null) return;
            destinationLocation = location;
            destinationPath = normalized;
            destinationBar.Location = location;
            destinationBar.RelativePath = normalized;
            var version = Interlocked.Increment(ref loadVersion);
            foldersView.ClearSelection();
            foldersView.ItemsSource = Array.Empty<MdFileEntry>();
            folderStatus.Text = "Loading folders…";
            try
            {
                var entries = await provider.GetEntriesAsync(location.Id, normalized, IncludeHidden, destinationPickerCancellation.Token);
                if (version != Volatile.Read(ref loadVersion) || destinationPickerCancellation.IsCancellationRequested) return;
                var folders = entries.Where(entry => entry.CanNavigate)
                    .OrderBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
                    .ToArray();
                foldersView.ItemsSource = folders;
                folderStatus.Text = folders.Length == 0
                    ? "No subfolders. Use this folder to choose the current location."
                    : $"{folders.Length} folder(s)";
            }
            catch (OperationCanceledException) when (destinationPickerCancellation.IsCancellationRequested) { }
            catch (Exception exception)
            {
                if (version == Volatile.Read(ref loadVersion))
                    folderStatus.Text = $"Could not list folders: {exception.Message}";
            }
        }
    }

    private bool IsCurrentExplorerContext(IMdFileSystemProvider provider, string? locationId, string path) =>
        ReferenceEquals(Provider, provider) &&
        string.Equals(CurrentLocationId, locationId, StringComparison.Ordinal) &&
        string.Equals(CurrentPath, path, StringComparison.Ordinal);

    private async Task<MdFileOperationResult> ExecuteOperationAsync(MdFileOperationRequest request, CancellationToken cancellationToken = default)
    {
        _selectionToolbar.IsBusy = true;
        try
        {
            return await ExecuteOperationCoreAsync(request, cancellationToken);
        }
        finally
        {
            _selectionToolbar.IsBusy = _operations.Any(task => task.CanCancel);
        }
    }

    private async Task<MdFileOperationResult> ExecuteOperationCoreAsync(MdFileOperationRequest request, CancellationToken cancellationToken)
    {
        var provider = Provider;
        var visibleLocationId = CurrentLocationId;
        var visiblePath = CurrentPath;
        if (provider is null)
        {
            var noProvider = MdFileOperationResult.Failure("No file-system provider is configured.");
            UpdateStatus(noProvider.ErrorMessage);
            return noProvider;
        }
        var result = await _operations.ExecuteAsync(provider, request, cancellationToken);
        if (!IsCurrentExplorerContext(provider, visibleLocationId, visiblePath)) return result;
        if (result.HasConflict)
        {
            var args = new MdFileConflictEventArgs(request, result.ResultPath);
            ConflictRequested?.Invoke(this, args);
            if (!IsCurrentExplorerContext(provider, visibleLocationId, visiblePath)) return result;
            var policy = args.Policy;
            if (policy == MdFileConflictPolicy.Fail)
            {
                var choice = await ShowChoiceDialogAsync("An item already exists",
                    $"‘{result.ResultPath ?? request.NewName ?? Path.GetFileName(request.SourcePath)}’ already exists at the destination.",
                    new("Cancel", MdFileConflictPolicy.Fail),
                    new("Skip", MdFileConflictPolicy.Skip),
                    new("Keep both", MdFileConflictPolicy.KeepBoth),
                    new("Replace", MdFileConflictPolicy.Overwrite, MdButtonVariant.Filled));
                policy = choice is MdFileConflictPolicy selected ? selected : MdFileConflictPolicy.Fail;
            }
            if (!IsCurrentExplorerContext(provider, visibleLocationId, visiblePath)) return result;
            if (policy != MdFileConflictPolicy.Fail)
                result = await _operations.ExecuteAsync(provider, request with { ConflictPolicy = policy }, cancellationToken);
            if (!IsCurrentExplorerContext(provider, visibleLocationId, visiblePath)) return result;
        }
        UpdateStatus(result.Succeeded
            ? result.WasSkipped ? "Operation skipped." : "Operation completed."
            : result.HasConflict ? "Resolve the file name conflict to continue."
            : result.ErrorMessage ?? "The file operation failed.");
        if (result.HasPartialChanges || (result.Succeeded && request.Kind is not MdFileOperationKind.Delete))
            await ReloadCurrentDirectory(result.HasPartialChanges ? CancellationToken.None : cancellationToken);
        return result;
    }

    private void ShowContextMenu(MdFileEntry entry)
    {
        var selection = SelectedEntries;
        _contextMenu.Entry = entry;
        _contextMenu.Selection = selection.Count > 1 ? selection : new[] { entry };
        _contextMenu.Actions.Clear();
        _contextMenu.Actions.Add(new MdFileAction("open", entry.IsDirectory ? "Open folder" : "Open"));
        if (!entry.IsReadOnly && selection.Count <= 1) _contextMenu.Actions.Add(new MdFileAction("rename", "Rename"));
        _contextMenu.Actions.Add(new MdFileAction("copy", "Copy to"));
        _contextMenu.Actions.Add(new MdFileAction("move", "Move to"));
        if (selection.All(selected => !selected.IsReadOnly)) _contextMenu.Actions.Add(new MdFileAction("delete", "Delete", IsDestructive: true));
        _contextMenu.ShowAt(_itemsView);
    }

    private void OnContextActionInvoked(object? sender, MdFileActionInvokedEventArgs e)
    {
        FileActionRequested?.Invoke(this, e);
        switch (e.Action.Id)
        {
            case "open" when _contextMenu.Entry is { } entry: _ = OpenEntryAsync(entry); break;
            case "rename": _ = RenameSelectedAsync(); break;
            case "copy": _ = CopySelectedAsync(); break;
            case "move": _ = MoveSelectedAsync(); break;
            case "delete": _ = DeleteSelectedAsync(); break;
        }
    }

    private void OnSelectionActionInvoked(object? sender, MdFileActionInvokedEventArgs e)
    {
        FileActionRequested?.Invoke(this, e);
        switch (e.Action.Id)
        {
            case "open" when SelectedEntries.Count == 1: _ = OpenEntryAsync(SelectedEntries[0]); break;
            case "rename": _ = RenameSelectedAsync(); break;
            case "copy": _ = CopySelectedAsync(); break;
            case "move": _ = MoveSelectedAsync(); break;
            case "delete": _ = DeleteSelectedAsync(); break;
        }
    }

    private async Task<object?> ShowChoiceDialogAsync(string title, object content, params MdFileDialogChoice[] choices)
    {
        var actionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };
        var dialog = new MdDialog
        {
            Variant = MdDialogVariant.Basic,
            Headline = title,
            Content = content,
            Actions = actionPanel
        };
        AutomationProperties.SetName(dialog, title);
        foreach (var choice in choices)
        {
            var button = new MdButton
            {
                Content = choice.Label,
                Variant = choice.Variant,
                CommandParameter = choice.Result
            };
            button.Click += (_, _) =>
            {
                var result = choice.ResultFactory is null ? choice.Result : choice.ResultFactory();
                _dialogHost.Close(result);
            };
            actionPanel.Children.Add(button);
        }
        return await _dialogHost.ShowAsync(dialog, _lifetime.Token);
    }

    private void OnOperationsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _operationCenter.IsVisible = _operations.Count > 0;
    }

    private void UpdateStatus(string? message)
    {
        StatusMessage = message;
        _statusText.Text = message;
        _statusText.IsVisible = !string.IsNullOrWhiteSpace(message);
    }

    private void SetLoading(bool isLoading)
    {
        IsLoading = isLoading;
        _itemsView.IsEnabled = !isLoading;
        _loadingIndicator.IsVisible = isLoading;
        _loadingIndicator.IsIndeterminate = isLoading;
    }

    private void ClearEntries()
    {
        foreach (var image in _ownedThumbnails.ToArray()) DisposeImage(image);
        _ownedThumbnails.Clear();
        _entries.Clear();
    }

    private void SetPreview(MdFilePreview? preview)
    {
        var previousImage = _previewPane.Preview?.Image;
        _previewPane.Preview = preview;
        if (previousImage is not null && !ReferenceEquals(previousImage, preview?.Image))
            DisposeImage(previousImage);
    }

    private void DisposeImage(IImage? image)
    {
        if (image is IDisposable disposable)
        {
            _ownedThumbnails.Remove(image);
            disposable.Dispose();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var navigation = _navigationCancellation;
        var preview = _previewCancellation;
        var thumbnails = _thumbnailCancellation;
        _navigationCancellation = null;
        _previewCancellation = null;
        _thumbnailCancellation = null;
        _contextMenu.Dismiss();
        navigation?.Cancel();
        preview?.Cancel();
        thumbnails?.Cancel();
        SetLoading(false);
        _previewPane.IsLoading = false;
        _previewPane.Entry = null;
        _previewPane.ErrorMessage = null;
        SetPreview(null);
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateResponsiveLayout();
        if (Provider is not null) _ = InitializeAsync();
    }

    private sealed record MdFileDialogChoice(string Label, object? Result, MdButtonVariant Variant = MdButtonVariant.Text, Func<object?>? ResultFactory = null);
}
