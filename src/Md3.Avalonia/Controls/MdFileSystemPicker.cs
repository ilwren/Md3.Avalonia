using System.Globalization;
using System.IO.Enumeration;
using System.Security;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A Material file and folder browser that returns normalized real file-system paths without
/// using Avalonia's <c>StorageProvider</c> or a platform document provider.
/// </summary>
/// <remarks>
/// The control can only enumerate paths the process is allowed to access. In particular, it does
/// not bypass Android scoped storage, application sandboxes, or operating-system permissions.
/// Hosts should start it in an application-accessible directory and request any required platform
/// permissions themselves.
/// </remarks>
[TemplatePart("PART_PathBox", typeof(MdTextBox))]
[TemplatePart("PART_BreadcrumbHost", typeof(ItemsControl))]
[TemplatePart("PART_ItemsHost", typeof(MdList))]
[TemplatePart("PART_UpButton", typeof(MdIconButton))]
[TemplatePart("PART_HomeButton", typeof(MdButton))]
[TemplatePart("PART_RootButton", typeof(MdButton))]
[TemplatePart("PART_CompactHomeButton", typeof(MdIconButton))]
[TemplatePart("PART_PathToggleButton", typeof(MdIconButton))]
[TemplatePart("PART_RefreshButton", typeof(MdIconButton))]
[TemplatePart("PART_ConfirmButton", typeof(MdButton))]
[TemplatePart("PART_CancelButton", typeof(MdButton))]
[PseudoClasses(":file-mode", ":folder-mode", ":multiple", ":compact", ":powerline-breadcrumb", ":editing-path", ":loading", ":empty", ":error")]
public sealed class MdFileSystemPicker : TemplatedControl
{
    public static readonly StyledProperty<MdFileSystemPickerMode> ModeProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, MdFileSystemPickerMode>(nameof(Mode));

    public static readonly StyledProperty<bool> AllowMultipleProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, bool>(nameof(AllowMultiple));

    public static readonly StyledProperty<string?> InitialDirectoryProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, string?>(nameof(InitialDirectory));

    public static readonly StyledProperty<IEnumerable<string>?> FilePatternsProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, IEnumerable<string>?>(nameof(FilePatterns));

    public static readonly StyledProperty<bool> ShowHiddenItemsProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, bool>(nameof(ShowHiddenItems));

    public static readonly StyledProperty<MdFileSystemBreadcrumbStyle> BreadcrumbStyleProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, MdFileSystemBreadcrumbStyle>(nameof(BreadcrumbStyle));

    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(Title), "Choose a file-system path");

    public static readonly StyledProperty<object?> PathLabelProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(PathLabel), "Location");

    public static readonly StyledProperty<object?> ConfirmTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(ConfirmText), "Choose");

    public static readonly StyledProperty<object?> CancelTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(CancelText), "Cancel");

    public static readonly StyledProperty<object?> EmptyTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(EmptyText), "No matching items in this folder.");

    public static readonly StyledProperty<object?> QuickAccessTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(QuickAccessText), "Quick access");

    public static readonly StyledProperty<object?> HomeTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(HomeText), "Home");

    public static readonly StyledProperty<object?> RootTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(RootText), "File-system root");

    public static readonly StyledProperty<object?> FilesTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(FilesText), "Files");

    public static readonly StyledProperty<object?> FoldersTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(FoldersText), "Folders");

    public static readonly StyledProperty<object?> SelectionModeTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(SelectionModeText), "Selection mode");

    public static readonly StyledProperty<object?> ShowHiddenTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(ShowHiddenText), "Show hidden");

    public static readonly StyledProperty<object?> LoadingTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(LoadingText), "Loading folder…");

    public static readonly StyledProperty<object?> ErrorHintTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(ErrorHintText), "Enter another accessible path or go to Home.");

    public static readonly StyledProperty<object?> ParentFolderTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(ParentFolderText), "Parent folder");

    public static readonly StyledProperty<object?> RefreshTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(RefreshText), "Refresh folder");

    public static readonly StyledProperty<object?> EditPathTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(EditPathText), "Edit full path");

    public static readonly StyledProperty<object?> EntriesTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(EntriesText), "Files and folders");

    public static readonly StyledProperty<object?> OpenFolderTextProperty =
        AvaloniaProperty.Register<MdFileSystemPicker, object?>(nameof(OpenFolderText), "Open folder");

    public static readonly DirectProperty<MdFileSystemPicker, bool> IsPowerlineBreadcrumbProperty =
        AvaloniaProperty.RegisterDirect<MdFileSystemPicker, bool>(
            nameof(IsPowerlineBreadcrumb), control => control.IsPowerlineBreadcrumb);

    public static readonly DirectProperty<MdFileSystemPicker, IReadOnlyList<MdFileSystemEntry>> EntriesProperty =
        AvaloniaProperty.RegisterDirect<MdFileSystemPicker, IReadOnlyList<MdFileSystemEntry>>(
            nameof(Entries), control => control.Entries);

    public static readonly DirectProperty<MdFileSystemPicker, IReadOnlyList<MdFileSystemPathSegment>> PathSegmentsProperty =
        AvaloniaProperty.RegisterDirect<MdFileSystemPicker, IReadOnlyList<MdFileSystemPathSegment>>(
            nameof(PathSegments), control => control.PathSegments);

    public static readonly DirectProperty<MdFileSystemPicker, IReadOnlyList<string>> SelectedPathsProperty =
        AvaloniaProperty.RegisterDirect<MdFileSystemPicker, IReadOnlyList<string>>(
            nameof(SelectedPaths), control => control.SelectedPaths);

    public static readonly DirectProperty<MdFileSystemPicker, string?> CurrentDirectoryProperty =
        AvaloniaProperty.RegisterDirect<MdFileSystemPicker, string?>(
            nameof(CurrentDirectory), control => control.CurrentDirectory);

    public static readonly DirectProperty<MdFileSystemPicker, string?> ErrorMessageProperty =
        AvaloniaProperty.RegisterDirect<MdFileSystemPicker, string?>(
            nameof(ErrorMessage), control => control.ErrorMessage);

    public static readonly DirectProperty<MdFileSystemPicker, string> SelectionSummaryProperty =
        AvaloniaProperty.RegisterDirect<MdFileSystemPicker, string>(
            nameof(SelectionSummary), control => control.SelectionSummary);

    public static readonly DirectProperty<MdFileSystemPicker, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<MdFileSystemPicker, bool>(
            nameof(IsLoading), control => control.IsLoading);

    public static readonly DirectProperty<MdFileSystemPicker, bool> CanGoUpProperty =
        AvaloniaProperty.RegisterDirect<MdFileSystemPicker, bool>(
            nameof(CanGoUp), control => control.CanGoUp);

    public static readonly DirectProperty<MdFileSystemPicker, bool> CanConfirmProperty =
        AvaloniaProperty.RegisterDirect<MdFileSystemPicker, bool>(
            nameof(CanConfirm), control => control.CanConfirm);

    private bool _isPowerlineBreadcrumb;
    private IReadOnlyList<MdFileSystemEntry> _entries = Array.Empty<MdFileSystemEntry>();
    private IReadOnlyList<MdFileSystemPathSegment> _pathSegments = Array.Empty<MdFileSystemPathSegment>();
    private IReadOnlyList<string> _selectedPaths = Array.Empty<string>();
    private string? _currentDirectory;
    private string? _errorMessage;
    private string _selectionSummary = "No file selected.";
    private bool _isLoading;
    private bool _canGoUp;
    private bool _canConfirm;
    private bool _isAttached;
    private int _navigationVersion;
    private CancellationTokenSource? _navigationCancellation;
    private MdTextBox? _pathBox;
    private ItemsControl? _breadcrumbHost;
    private MdList? _itemsHost;
    private MdIconButton? _upButton;
    private MdButton? _homeButton;
    private MdButton? _rootButton;
    private MdIconButton? _compactHomeButton;
    private MdIconButton? _pathToggleButton;
    private MdIconButton? _refreshButton;
    private MdButton? _confirmButton;
    private MdButton? _cancelButton;
    private bool _isEditingPath;
    private object? _defaultTitle = "Choose a file-system path";
    private object? _defaultPathLabel = "Location";
    private object? _defaultConfirmText = "Choose";
    private object? _defaultCancelText = "Cancel";
    private object? _defaultEmptyText = "No matching items in this folder.";
    private object? _defaultQuickAccessText = "Quick access";
    private object? _defaultHomeText = "Home";
    private object? _defaultRootText = "File-system root";
    private object? _defaultFilesText = "Files";
    private object? _defaultFoldersText = "Folders";
    private object? _defaultSelectionModeText = "Selection mode";
    private object? _defaultShowHiddenText = "Show hidden";
    private object? _defaultLoadingText = "Loading folder…";
    private object? _defaultErrorHintText = "Enter another accessible path or go to Home.";
    private object? _defaultParentFolderText = "Parent folder";
    private object? _defaultRefreshText = "Refresh folder";
    private object? _defaultEditPathText = "Edit full path";
    private object? _defaultEntriesText = "Files and folders";
    private object? _defaultOpenFolderText = "Open folder";

    static MdFileSystemPicker()
    {
        ModeProperty.Changed.AddClassHandler<MdFileSystemPicker>((control, _) => control.OnOptionsChanged(reload: true));
        AllowMultipleProperty.Changed.AddClassHandler<MdFileSystemPicker>((control, _) => control.OnOptionsChanged(reload: false));
        InitialDirectoryProperty.Changed.AddClassHandler<MdFileSystemPicker>((control, _) => control.OnInitialDirectoryChanged());
        FilePatternsProperty.Changed.AddClassHandler<MdFileSystemPicker>((control, _) => control.OnOptionsChanged(reload: true));
        ShowHiddenItemsProperty.Changed.AddClassHandler<MdFileSystemPicker>((control, _) => control.OnOptionsChanged(reload: true));
        BreadcrumbStyleProperty.Changed.AddClassHandler<MdFileSystemPicker>((control, _) => control.UpdateBreadcrumbStyle());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdFileSystemPicker>((control, _) => control.OnCultureChanged());
        BoundsProperty.Changed.AddClassHandler<MdFileSystemPicker>((control, _) => control.UpdatePseudoClasses());
    }

    public MdFileSystemPicker()
    {
        Focusable = true;
        UpdateLocalizedText();
        UpdatePseudoClasses();
        UpdateSelectionState();
    }

    /// <summary>Whether files or folders are returned.</summary>
    public MdFileSystemPickerMode Mode { get => GetValue(ModeProperty); set => SetValue(ModeProperty, value); }

    /// <summary>Whether more than one item can be selected.</summary>
    public bool AllowMultiple { get => GetValue(AllowMultipleProperty); set => SetValue(AllowMultipleProperty, value); }

    /// <summary>The preferred absolute directory used for the first navigation.</summary>
    public string? InitialDirectory { get => GetValue(InitialDirectoryProperty); set => SetValue(InitialDirectoryProperty, value); }

    /// <summary>
    /// Optional simple file-name expressions such as <c>*.json</c> and <c>*.txt</c>.
    /// Directories are never filtered. Separate multiple expressions into the enumerable or with
    /// semicolons/commas inside an item.
    /// </summary>
    public IEnumerable<string>? FilePatterns { get => GetValue(FilePatternsProperty); set => SetValue(FilePatternsProperty, value); }

    /// <summary>Whether hidden entries are shown.</summary>
    public bool ShowHiddenItems { get => GetValue(ShowHiddenItemsProperty); set => SetValue(ShowHiddenItemsProperty, value); }

    /// <summary>The built-in visual treatment used for breadcrumb path segments.</summary>
    public MdFileSystemBreadcrumbStyle BreadcrumbStyle { get => GetValue(BreadcrumbStyleProperty); set => SetValue(BreadcrumbStyleProperty, value); }

    /// <summary>Whether the effective breadcrumb treatment is <see cref="MdFileSystemBreadcrumbStyle.Powerline"/>.</summary>
    public bool IsPowerlineBreadcrumb => _isPowerlineBreadcrumb;

    /// <summary>Header title. A locally supplied value is preserved across culture and mode changes.</summary>
    public object? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public object? PathLabel { get => GetValue(PathLabelProperty); set => SetValue(PathLabelProperty, value); }
    public object? ConfirmText { get => GetValue(ConfirmTextProperty); set => SetValue(ConfirmTextProperty, value); }
    public object? CancelText { get => GetValue(CancelTextProperty); set => SetValue(CancelTextProperty, value); }
    public object? EmptyText { get => GetValue(EmptyTextProperty); set => SetValue(EmptyTextProperty, value); }
    public object? QuickAccessText { get => GetValue(QuickAccessTextProperty); set => SetValue(QuickAccessTextProperty, value); }
    public object? HomeText { get => GetValue(HomeTextProperty); set => SetValue(HomeTextProperty, value); }
    public object? RootText { get => GetValue(RootTextProperty); set => SetValue(RootTextProperty, value); }
    public object? FilesText { get => GetValue(FilesTextProperty); set => SetValue(FilesTextProperty, value); }
    public object? FoldersText { get => GetValue(FoldersTextProperty); set => SetValue(FoldersTextProperty, value); }
    public object? SelectionModeText { get => GetValue(SelectionModeTextProperty); set => SetValue(SelectionModeTextProperty, value); }
    public object? ShowHiddenText { get => GetValue(ShowHiddenTextProperty); set => SetValue(ShowHiddenTextProperty, value); }
    public object? LoadingText { get => GetValue(LoadingTextProperty); set => SetValue(LoadingTextProperty, value); }
    public object? ErrorHintText { get => GetValue(ErrorHintTextProperty); set => SetValue(ErrorHintTextProperty, value); }
    public object? ParentFolderText { get => GetValue(ParentFolderTextProperty); set => SetValue(ParentFolderTextProperty, value); }
    public object? RefreshText { get => GetValue(RefreshTextProperty); set => SetValue(RefreshTextProperty, value); }
    public object? EditPathText { get => GetValue(EditPathTextProperty); set => SetValue(EditPathTextProperty, value); }
    public object? EntriesText { get => GetValue(EntriesTextProperty); set => SetValue(EntriesTextProperty, value); }
    public object? OpenFolderText { get => GetValue(OpenFolderTextProperty); set => SetValue(OpenFolderTextProperty, value); }

    /// <summary>The directory entries currently visible after mode and pattern filtering.</summary>
    public IReadOnlyList<MdFileSystemEntry> Entries
    {
        get => _entries;
        private set => SetAndRaise(EntriesProperty, ref _entries, value);
    }

    /// <summary>The root-to-current-directory segments displayed by the breadcrumb path bar.</summary>
    public IReadOnlyList<MdFileSystemPathSegment> PathSegments
    {
        get => _pathSegments;
        private set => SetAndRaise(PathSegmentsProperty, ref _pathSegments, value);
    }

    /// <summary>The normalized absolute paths represented by the current valid selection.</summary>
    public IReadOnlyList<string> SelectedPaths
    {
        get => _selectedPaths;
        private set => SetAndRaise(SelectedPathsProperty, ref _selectedPaths, value);
    }

    /// <summary>The normalized absolute directory currently being browsed.</summary>
    public string? CurrentDirectory
    {
        get => _currentDirectory;
        private set => SetAndRaise(CurrentDirectoryProperty, ref _currentDirectory, value);
    }

    /// <summary>The most recent navigation error, or <see langword="null"/>.</summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetAndRaise(ErrorMessageProperty, ref _errorMessage, value);
    }

    public string SelectionSummary
    {
        get => _selectionSummary;
        private set => SetAndRaise(SelectionSummaryProperty, ref _selectionSummary, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetAndRaise(IsLoadingProperty, ref _isLoading, value);
    }

    public bool CanGoUp
    {
        get => _canGoUp;
        private set => SetAndRaise(CanGoUpProperty, ref _canGoUp, value);
    }

    public bool CanConfirm
    {
        get => _canConfirm;
        private set => SetAndRaise(CanConfirmProperty, ref _canConfirm, value);
    }

    /// <summary>Raised after the user confirms one or more paths.</summary>
    public event EventHandler<MdFileSystemSelectionEventArgs>? SelectionConfirmed;

    /// <summary>Raised when the picker cancel action is invoked.</summary>
    public event EventHandler? CancelRequested;

    /// <summary>Raised when a path cannot be enumerated.</summary>
    public event EventHandler<MdFileSystemNavigationFailedEventArgs>? NavigationFailed;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateParts();
        base.OnApplyTemplate(e);
        _pathBox = e.NameScope.Find<MdTextBox>("PART_PathBox");
        _breadcrumbHost = e.NameScope.Find<ItemsControl>("PART_BreadcrumbHost");
        _itemsHost = e.NameScope.Find<MdList>("PART_ItemsHost");
        _upButton = e.NameScope.Find<MdIconButton>("PART_UpButton");
        _homeButton = e.NameScope.Find<MdButton>("PART_HomeButton");
        _rootButton = e.NameScope.Find<MdButton>("PART_RootButton");
        _compactHomeButton = e.NameScope.Find<MdIconButton>("PART_CompactHomeButton");
        _pathToggleButton = e.NameScope.Find<MdIconButton>("PART_PathToggleButton");
        _refreshButton = e.NameScope.Find<MdIconButton>("PART_RefreshButton");
        _confirmButton = e.NameScope.Find<MdButton>("PART_ConfirmButton");
        _cancelButton = e.NameScope.Find<MdButton>("PART_CancelButton");

        if (_pathBox is not null) _pathBox.KeyDown += OnPathKeyDown;
        if (_breadcrumbHost is not null) _breadcrumbHost.AddHandler(Button.ClickEvent, OnPathSegmentClicked);
        if (_itemsHost is not null)
        {
            _itemsHost.SelectionChanged += OnSelectionChanged;
            _itemsHost.DoubleTapped += OnItemDoubleTapped;
            _itemsHost.AddHandler(Button.ClickEvent, OnEntryOpenClicked);
        }
        if (_upButton is not null) _upButton.Click += OnUpClick;
        if (_homeButton is not null) _homeButton.Click += OnHomeClick;
        if (_rootButton is not null) _rootButton.Click += OnRootClick;
        if (_compactHomeButton is not null) _compactHomeButton.Click += OnHomeClick;
        if (_pathToggleButton is not null) _pathToggleButton.Click += OnPathToggleClick;
        if (_refreshButton is not null) _refreshButton.Click += OnRefreshClick;
        if (_confirmButton is not null) _confirmButton.Click += OnConfirmClick;
        if (_cancelButton is not null) _cancelButton.Click += OnCancelClick;

        ConfigureSelectionMode();
        if (_pathBox is not null) _pathBox.Text = CurrentDirectory;
        UpdateSelectionState();
        Dispatcher.UIThread.Post(ApplyBreadcrumbStyleToTemplate, DispatcherPriority.Loaded);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        if (CurrentDirectory is null)
            _ = NavigateAsync(ResolveStartDirectory());
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        CancelNavigation();
        DetachTemplateParts();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            _ = RefreshAsync();
            e.Handled = true;
        }
        else if (e.Key == Key.Back && !ReferenceEquals(e.Source, _pathBox))
        {
            _ = GoUpAsync();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && ReferenceEquals(e.Source, _itemsHost))
        {
            ActivateSelectedEntry();
            e.Handled = true;
        }
        else
        {
            base.OnKeyDown(e);
        }
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdFileSystemPickerAutomationPeer(this);

    /// <summary>Navigates to and asynchronously enumerates an absolute or relative directory path.</summary>
    /// <returns><see langword="true"/> when the directory was opened.</returns>
    public async Task<bool> NavigateAsync(string? path, CancellationToken cancellationToken = default)
    {
        var requestedPath = string.IsNullOrWhiteSpace(path) ? ResolveStartDirectory() : path.Trim();
        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(requestedPath);
            if (!Directory.Exists(fullPath))
                throw new DirectoryNotFoundException($"Directory does not exist: {fullPath}");
        }
        catch (Exception exception) when (IsExpectedFileSystemFailure(exception))
        {
            await ReportNavigationFailureAsync(requestedPath, exception);
            return false;
        }

        var version = Interlocked.Increment(ref _navigationVersion);
        var navigation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var previous = Interlocked.Exchange(ref _navigationCancellation, navigation);
        previous?.Cancel();
        previous?.Dispose();

        await RunOnUiThreadAsync(() =>
        {
            IsLoading = true;
            ErrorMessage = null;
            UpdateSelectionState();
            UpdatePseudoClasses();
        });

        try
        {
            var options = new EnumerationOptionsSnapshot(
                Mode,
                ShowHiddenItems,
                NormalizePatterns(FilePatterns),
                MdLocalization.ResolveCulture(this));
            var entries = await Task.Run(() => Enumerate(fullPath, options, navigation.Token), navigation.Token);
            navigation.Token.ThrowIfCancellationRequested();
            if (version != _navigationVersion) return false;

            await RunOnUiThreadAsync(() =>
            {
                CurrentDirectory = fullPath;
                PathSegments = BuildPathSegments(fullPath);
                Entries = entries;
                ErrorMessage = null;
                IsLoading = false;
                CanGoUp = Directory.GetParent(fullPath) is not null;
                _isEditingPath = false;
                if (_pathBox is not null) _pathBox.Text = fullPath;
                _itemsHost?.UnselectAll();
                UpdateSelectionState();
                UpdatePseudoClasses();
                Dispatcher.UIThread.Post(ApplyBreadcrumbStyleToTemplate, DispatcherPriority.Loaded);
            });
            return true;
        }
        catch (OperationCanceledException)
        {
            if (version == _navigationVersion)
            {
                await RunOnUiThreadAsync(() =>
                {
                    IsLoading = false;
                    UpdatePseudoClasses();
                });
            }
            return false;
        }
        catch (Exception exception) when (IsExpectedFileSystemFailure(exception))
        {
            if (version == _navigationVersion)
                await ReportNavigationFailureAsync(fullPath, exception);
            return false;
        }
    }

    /// <summary>Reloads the current directory.</summary>
    public Task<bool> RefreshAsync(CancellationToken cancellationToken = default) =>
        NavigateAsync(CurrentDirectory ?? ResolveStartDirectory(), cancellationToken);

    /// <summary>Opens the parent of the current directory when one exists.</summary>
    public Task<bool> GoUpAsync(CancellationToken cancellationToken = default)
    {
        var parent = CurrentDirectory is null ? null : Directory.GetParent(CurrentDirectory)?.FullName;
        return parent is null ? Task.FromResult(false) : NavigateAsync(parent, cancellationToken);
    }

    /// <summary>Opens the current user's home directory, falling back to application data.</summary>
    public Task<bool> GoHomeAsync(CancellationToken cancellationToken = default) =>
        NavigateAsync(ResolveHomeDirectory(), cancellationToken);

    /// <summary>Opens the file-system root that contains the current directory.</summary>
    public Task<bool> GoToRootAsync(CancellationToken cancellationToken = default)
    {
        var current = CurrentDirectory ?? ResolveStartDirectory();
        var root = Path.GetPathRoot(Path.GetFullPath(current));
        return NavigateAsync(string.IsNullOrWhiteSpace(root) ? current : root, cancellationToken);
    }

    /// <summary>Confirms the valid selection, or the current directory in folder mode.</summary>
    public void ConfirmSelection()
    {
        IReadOnlyList<string> paths = SelectedPaths;
        if (Mode == MdFileSystemPickerMode.Folder && paths.Count == 0 &&
            CurrentDirectory is { } current && Directory.Exists(current))
        {
            paths = [current];
        }

        if (paths.Count == 0) return;
        SelectionConfirmed?.Invoke(this, new MdFileSystemSelectionEventArgs(paths.ToArray()));
    }

    private void OnOptionsChanged(bool reload)
    {
        UpdateLocalizedText();
        ConfigureSelectionMode();
        UpdatePseudoClasses();
        UpdateSelectionState();
        if (reload && _isAttached && CurrentDirectory is not null)
            _ = RefreshAsync();
    }

    private void OnInitialDirectoryChanged()
    {
        if (_isAttached && !string.IsNullOrWhiteSpace(InitialDirectory))
            _ = NavigateAsync(InitialDirectory);
    }

    private void OnCultureChanged()
    {
        UpdateLocalizedText();
        UpdateSelectionState();
        if (_isAttached && CurrentDirectory is not null)
            _ = RefreshAsync();
    }

    private void UpdateLocalizedText()
    {
        UpdateLocalizedProperty(TitleProperty, ref _defaultTitle,
            Mode == MdFileSystemPickerMode.Folder ? "FileSystemChooseFolder" : "FileSystemChooseFile");
        UpdateLocalizedProperty(PathLabelProperty, ref _defaultPathLabel, "FileSystemLocation");
        UpdateLocalizedProperty(ConfirmTextProperty, ref _defaultConfirmText, "FileSystemChoose");
        UpdateLocalizedProperty(CancelTextProperty, ref _defaultCancelText, "Cancel");
        UpdateLocalizedProperty(EmptyTextProperty, ref _defaultEmptyText, "FileSystemEmpty");
        UpdateLocalizedProperty(QuickAccessTextProperty, ref _defaultQuickAccessText, "FileSystemQuickAccess");
        UpdateLocalizedProperty(HomeTextProperty, ref _defaultHomeText, "FileSystemHome");
        UpdateLocalizedProperty(RootTextProperty, ref _defaultRootText, "FileSystemRoot");
        UpdateLocalizedProperty(FilesTextProperty, ref _defaultFilesText, "FileSystemFiles");
        UpdateLocalizedProperty(FoldersTextProperty, ref _defaultFoldersText, "FileSystemFolders");
        UpdateLocalizedProperty(SelectionModeTextProperty, ref _defaultSelectionModeText, "FileSystemSelectionMode");
        UpdateLocalizedProperty(ShowHiddenTextProperty, ref _defaultShowHiddenText, "FileSystemShowHidden");
        UpdateLocalizedProperty(LoadingTextProperty, ref _defaultLoadingText, "FileSystemLoading");
        UpdateLocalizedProperty(ErrorHintTextProperty, ref _defaultErrorHintText, "FileSystemErrorHint");
        UpdateLocalizedProperty(ParentFolderTextProperty, ref _defaultParentFolderText, "FileSystemParentFolder");
        UpdateLocalizedProperty(RefreshTextProperty, ref _defaultRefreshText, "FileSystemRefresh");
        UpdateLocalizedProperty(EditPathTextProperty, ref _defaultEditPathText, "FileSystemEditPath");
        UpdateLocalizedProperty(EntriesTextProperty, ref _defaultEntriesText, "FileSystemEntries");
        UpdateLocalizedProperty(OpenFolderTextProperty, ref _defaultOpenFolderText, "FileSystemOpenFolder");
    }

    private void UpdateLocalizedProperty(
        StyledProperty<object?> property,
        ref object? defaultValue,
        string localizationKey)
    {
        if (!Equals(GetValue(property), defaultValue)) return;
        defaultValue = MdLocalization.GetString(localizationKey, this);
        SetCurrentValue(property, defaultValue);
    }

    private void ConfigureSelectionMode()
    {
        if (_itemsHost is null) return;
        _itemsHost.SelectionMode = AllowMultiple
            ? SelectionMode.Multiple | SelectionMode.Toggle
            : SelectionMode.Single | SelectionMode.Toggle;
        _itemsHost.UnselectAll();
    }

    private void UpdateBreadcrumbStyle()
    {
        SetAndRaise(
            IsPowerlineBreadcrumbProperty,
            ref _isPowerlineBreadcrumb,
            BreadcrumbStyle == MdFileSystemBreadcrumbStyle.Powerline);
        UpdatePseudoClasses();
        ApplyBreadcrumbStyleToTemplate();
    }

    private void ApplyBreadcrumbStyleToTemplate()
    {
        foreach (var panel in this.GetVisualDescendants().OfType<StackPanel>())
            if (panel.Classes.Contains("standard-segment"))
                panel.IsVisible = !IsPowerlineBreadcrumb;
        foreach (var grid in this.GetVisualDescendants().OfType<Grid>())
            if (grid.Classes.Contains("powerline-segment"))
                grid.IsVisible = IsPowerlineBreadcrumb;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":file-mode", Mode == MdFileSystemPickerMode.File);
        PseudoClasses.Set(":folder-mode", Mode == MdFileSystemPickerMode.Folder);
        PseudoClasses.Set(":multiple", AllowMultiple);
        PseudoClasses.Set(":compact", Bounds.Width > 0 && Bounds.Width < 760);
        PseudoClasses.Set(":powerline-breadcrumb", BreadcrumbStyle == MdFileSystemBreadcrumbStyle.Powerline);
        PseudoClasses.Set(":editing-path", _isEditingPath);
        PseudoClasses.Set(":loading", IsLoading);
        PseudoClasses.Set(":empty", !IsLoading && ErrorMessage is null && Entries.Count == 0);
        PseudoClasses.Set(":error", !string.IsNullOrWhiteSpace(ErrorMessage));
    }

    private void UpdateSelectionState()
    {
        IEnumerable<MdFileSystemEntry> selected = _itemsHost is null
            ? Array.Empty<MdFileSystemEntry>()
            : AllowMultiple
                ? _itemsHost.SelectedItems?.OfType<MdFileSystemEntry>() ?? Array.Empty<MdFileSystemEntry>()
                : _itemsHost.SelectedItem is MdFileSystemEntry item ? [item] : Array.Empty<MdFileSystemEntry>();

        var paths = selected
            .Where(entry => Mode == MdFileSystemPickerMode.Folder ? entry.IsDirectory : !entry.IsDirectory)
            .Select(entry => entry.FullPath)
            .Distinct(PathComparer)
            .ToArray();
        SelectedPaths = paths;
        CanConfirm = !IsLoading && ErrorMessage is null &&
                     (paths.Length > 0 || Mode == MdFileSystemPickerMode.Folder && CurrentDirectory is not null);
        SelectionSummary = paths.Length switch
        {
            > 1 => MdLocalization.Format("FileSystemItemsSelected", this, paths.Length),
            1 => paths[0],
            _ when Mode == MdFileSystemPickerMode.Folder && CurrentDirectory is not null =>
                MdLocalization.GetString("FileSystemChooseCurrentFolder", this),
            _ when Mode == MdFileSystemPickerMode.Folder =>
                MdLocalization.GetString("FileSystemNoFolderSelected", this),
            _ => MdLocalization.GetString("FileSystemNoFileSelected", this)
        };
    }

    private void ActivateSelectedEntry()
    {
        if (_itemsHost?.SelectedItem is not MdFileSystemEntry entry) return;
        if (entry.IsDirectory)
            _ = NavigateAsync(entry.FullPath);
        else if (Mode == MdFileSystemPickerMode.File)
            ConfirmSelection();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => UpdateSelectionState();

    private void OnItemDoubleTapped(object? sender, TappedEventArgs e)
    {
        ActivateSelectedEntry();
        e.Handled = true;
    }

    private void OnEntryOpenClicked(object? sender, RoutedEventArgs e)
    {
        if (e.Source is Button { Name: "PART_OpenEntryButton", Tag: MdFileSystemEntry entry } && entry.IsDirectory)
        {
            _ = NavigateAsync(entry.FullPath);
            e.Handled = true;
        }
    }

    private void OnPathSegmentClicked(object? sender, RoutedEventArgs e)
    {
        if (e.Source is Button { Name: "PART_PathSegmentButton", Tag: MdFileSystemPathSegment segment } &&
            !segment.IsCurrent)
        {
            _ = NavigateAsync(segment.FullPath);
            e.Handled = true;
        }
    }

    private void OnPathKeyDown(object? sender, KeyEventArgs e)
    {
        if (_pathBox is null) return;
        if (e.Key == Key.Escape)
        {
            _isEditingPath = false;
            _pathBox.Text = CurrentDirectory;
            UpdatePseudoClasses();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            _ = NavigateAsync(_pathBox.Text);
            e.Handled = true;
        }
    }

    private void OnPathToggleClick(object? sender, RoutedEventArgs e)
    {
        _isEditingPath = !_isEditingPath;
        if (!_isEditingPath && _pathBox is not null)
            _pathBox.Text = CurrentDirectory;
        UpdatePseudoClasses();
        if (!_isEditingPath || _pathBox is null) return;
        Dispatcher.UIThread.Post(() =>
        {
            _pathBox.Focus();
            _pathBox.SelectAll();
        }, DispatcherPriority.Input);
    }

    private void OnUpClick(object? sender, RoutedEventArgs e) => _ = GoUpAsync();
    private void OnHomeClick(object? sender, RoutedEventArgs e) => _ = GoHomeAsync();
    private void OnRootClick(object? sender, RoutedEventArgs e) => _ = GoToRootAsync();
    private void OnRefreshClick(object? sender, RoutedEventArgs e) => _ = RefreshAsync();
    private void OnConfirmClick(object? sender, RoutedEventArgs e) => ConfirmSelection();
    private void OnCancelClick(object? sender, RoutedEventArgs e) => CancelRequested?.Invoke(this, EventArgs.Empty);

    private void DetachTemplateParts()
    {
        if (_pathBox is not null) _pathBox.KeyDown -= OnPathKeyDown;
        if (_breadcrumbHost is not null) _breadcrumbHost.RemoveHandler(Button.ClickEvent, OnPathSegmentClicked);
        if (_itemsHost is not null)
        {
            _itemsHost.SelectionChanged -= OnSelectionChanged;
            _itemsHost.DoubleTapped -= OnItemDoubleTapped;
            _itemsHost.RemoveHandler(Button.ClickEvent, OnEntryOpenClicked);
        }
        if (_upButton is not null) _upButton.Click -= OnUpClick;
        if (_homeButton is not null) _homeButton.Click -= OnHomeClick;
        if (_rootButton is not null) _rootButton.Click -= OnRootClick;
        if (_compactHomeButton is not null) _compactHomeButton.Click -= OnHomeClick;
        if (_pathToggleButton is not null) _pathToggleButton.Click -= OnPathToggleClick;
        if (_refreshButton is not null) _refreshButton.Click -= OnRefreshClick;
        if (_confirmButton is not null) _confirmButton.Click -= OnConfirmClick;
        if (_cancelButton is not null) _cancelButton.Click -= OnCancelClick;
        _pathBox = null;
        _breadcrumbHost = null;
        _itemsHost = null;
        _upButton = null;
        _homeButton = null;
        _rootButton = null;
        _compactHomeButton = null;
        _pathToggleButton = null;
        _refreshButton = null;
        _confirmButton = null;
        _cancelButton = null;
    }

    private void CancelNavigation()
    {
        Interlocked.Increment(ref _navigationVersion);
        var navigation = Interlocked.Exchange(ref _navigationCancellation, null);
        navigation?.Cancel();
        navigation?.Dispose();
    }

    private async Task ReportNavigationFailureAsync(string path, Exception exception)
    {
        await RunOnUiThreadAsync(() =>
        {
            IsLoading = false;
            ErrorMessage = exception.Message;
            UpdateSelectionState();
            UpdatePseudoClasses();
            NavigationFailed?.Invoke(this, new MdFileSystemNavigationFailedEventArgs(path, exception));
        });
    }

    private static IReadOnlyList<MdFileSystemEntry> Enumerate(
        string directory,
        EnumerationOptionsSnapshot options,
        CancellationToken cancellationToken)
    {
        var entries = new List<MdFileSystemEntry>();
        foreach (var info in new DirectoryInfo(directory).EnumerateFileSystemInfos())
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool isDirectory;
            FileAttributes attributes;
            try
            {
                attributes = info.Attributes;
                isDirectory = (attributes & FileAttributes.Directory) != 0;
            }
            catch (Exception exception) when (IsExpectedFileSystemFailure(exception))
            {
                continue;
            }

            if (!options.ShowHiddenItems && IsHidden(info.Name, attributes)) continue;
            if (options.Mode == MdFileSystemPickerMode.Folder && !isDirectory) continue;
            if (!isDirectory && !MatchesPatterns(info.Name, options.Patterns)) continue;

            try
            {
                var size = info is FileInfo file ? file.Length : (long?)null;
                entries.Add(new MdFileSystemEntry(
                    info.Name,
                    Path.GetFullPath(info.FullName),
                    isDirectory,
                    info.LastWriteTime,
                    size,
                    options.Culture));
            }
            catch (Exception exception) when (IsExpectedFileSystemFailure(exception))
            {
                // A directory can change while it is being enumerated. Keep the rest of the list.
            }
        }

        return entries
            .OrderByDescending(entry => entry.IsDirectory)
            .ThenBy(entry => entry.Name, StringComparer.Create(options.Culture, ignoreCase: true))
            .ToArray();
    }

    private static bool IsHidden(string name, FileAttributes attributes) =>
        OperatingSystem.IsWindows()
            ? (attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0
            : name.Length > 0 && name[0] == '.';

    private static bool MatchesPatterns(string name, IReadOnlyList<string> patterns)
    {
        if (patterns.Count == 0) return true;
        var ignoreCase = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS();
        return patterns.Any(pattern => FileSystemName.MatchesSimpleExpression(pattern, name, ignoreCase));
    }

    private static IReadOnlyList<string> NormalizePatterns(IEnumerable<string>? patterns) =>
        patterns?
            .SelectMany(pattern => pattern.Split([';', ','], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Select(pattern => pattern.StartsWith('.') ? $"*{pattern}" : pattern)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
        ?? Array.Empty<string>();

    private static IReadOnlyList<MdFileSystemPathSegment> BuildPathSegments(string directory)
    {
        var fullPath = Path.GetFullPath(directory);
        var root = Path.GetPathRoot(fullPath) ?? string.Empty;
        var values = new List<(string Label, string FullPath)>();
        if (!string.IsNullOrEmpty(root))
        {
            var trimmedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            values.Add((string.IsNullOrEmpty(trimmedRoot) ? root : trimmedRoot, Path.GetFullPath(root)));
        }

        var relative = fullPath[root.Length..];
        var parts = relative.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);
        var current = root;
        foreach (var part in parts)
        {
            current = string.IsNullOrEmpty(current) ? part : Path.Combine(current, part);
            values.Add((part, Path.GetFullPath(current)));
        }

        if (values.Count == 0)
            values.Add((fullPath, fullPath));
        return values
            .Select((value, index) => new MdFileSystemPathSegment(
                value.Label,
                value.FullPath,
                index == values.Count - 1))
            .ToArray();
    }

    private string ResolveStartDirectory()
    {
        if (!string.IsNullOrWhiteSpace(InitialDirectory) && Directory.Exists(InitialDirectory))
            return InitialDirectory;
        return ResolveHomeDirectory();
    }

    private static string ResolveHomeDirectory()
    {
        string[] candidates =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Directory.GetCurrentDirectory()
        ];
        return candidates.FirstOrDefault(path => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
               ?? Path.GetPathRoot(Directory.GetCurrentDirectory())
               ?? Directory.GetCurrentDirectory();
    }

    private static bool IsExpectedFileSystemFailure(Exception exception) => exception is
        ArgumentException or NotSupportedException or IOException or
        UnauthorizedAccessException or SecurityException;

    private static async Task RunOnUiThreadAsync(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
            action();
        else
            await Dispatcher.UIThread.InvokeAsync(action);
    }

    private static StringComparer PathComparer =>
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private sealed record EnumerationOptionsSnapshot(
        MdFileSystemPickerMode Mode,
        bool ShowHiddenItems,
        IReadOnlyList<string> Patterns,
        CultureInfo Culture);
}

internal sealed class MdFileSystemPickerAutomationPeer(MdFileSystemPicker owner) : ControlAutomationPeer(owner)
{
    private MdFileSystemPicker PickerOwner => (MdFileSystemPicker)Owner;

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;
    protected override bool IsControlElementCore() => true;
    protected override bool IsContentElementCore() => true;
    protected override string? GetNameCore() =>
        AutomationProperties.GetName(PickerOwner) ?? PickerOwner.Title?.ToString() ?? base.GetNameCore();
}
