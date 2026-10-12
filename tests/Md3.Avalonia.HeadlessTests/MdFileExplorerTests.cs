using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Extra.Themes;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdFileExplorerTests
{
    [Fact]
    public void Relative_Path_Normalization_Rejects_Traversal_And_Absolute_Paths()
    {
        Assert.Equal("Docs/Notes", MdFileLocationBar.NormalizeRelativePath("Docs\\Notes/./"));
        Assert.Equal(string.Empty, MdFileLocationBar.NormalizeRelativePath("./"));
        Assert.Null(MdFileLocationBar.NormalizeRelativePath("../outside"));
        Assert.Null(MdFileLocationBar.NormalizeRelativePath("Docs/../../outside"));
        Assert.Null(MdFileLocationBar.NormalizeRelativePath(Path.GetFullPath(Path.GetTempPath())));
        Assert.Null(MdFileLocationBar.NormalizeRelativePath(@"C:\Users\Public"));
    }

    [Fact]
    public async Task Local_Provider_Lists_Hidden_Items_And_Enforces_The_Configured_Root()
    {
        using var directory = new TemporaryDirectory();
        Directory.CreateDirectory(Path.Combine(directory.Path, "Docs"));
        File.WriteAllText(Path.Combine(directory.Path, "Docs", "notes.txt"), "sample");
        File.WriteAllText(Path.Combine(directory.Path, ".private"), "hidden");
        var provider = new MdLocalFileSystemProvider([new MdFileLocation("home", "Home", directory.Path)]);

        var ordinary = await provider.GetEntriesAsync("home", string.Empty,
            cancellationToken: TestContext.Current.CancellationToken);
        var withHidden = await provider.GetEntriesAsync("home", string.Empty, includeHidden: true,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Contains(ordinary, entry => entry.Name == "Docs" && entry.CanNavigate);
        Assert.DoesNotContain(ordinary, entry => entry.Name == ".private");
        Assert.Contains(withHidden, entry => entry.Name == ".private" && entry.IsHidden);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await provider.GetEntriesAsync("home", "../outside",
                cancellationToken: TestContext.Current.CancellationToken);
        });
        var rootDelete = await provider.ExecuteAsync(new MdFileOperationRequest(
            MdFileOperationKind.Delete, "home", "."),
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(rootDelete.Succeeded);
        Assert.True(Directory.Exists(directory.Path));
    }

    [Fact]
    public async Task KeepBoth_Conflict_Policy_Creates_An_Unique_Folder_And_Copy()
    {
        using var directory = new TemporaryDirectory();
        File.WriteAllText(Path.Combine(directory.Path, "notes.txt"), "hello file browser");
        var provider = new MdLocalFileSystemProvider([new MdFileLocation("home", "Home", directory.Path)]);

        var firstFolder = await provider.ExecuteAsync(new MdFileOperationRequest(
            MdFileOperationKind.CreateDirectory, "home", string.Empty, NewName: "New folder"),
            cancellationToken: TestContext.Current.CancellationToken);
        var secondFolder = await provider.ExecuteAsync(new MdFileOperationRequest(
            MdFileOperationKind.CreateDirectory, "home", string.Empty, NewName: "New folder",
            ConflictPolicy: MdFileConflictPolicy.KeepBoth),
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(firstFolder.Succeeded);
        Assert.True(secondFolder.Succeeded);
        Assert.Equal("New folder (2)", secondFolder.ResultPath);
        Assert.True(Directory.Exists(Path.Combine(directory.Path, "New folder (2)")));

        var collision = await provider.ExecuteAsync(new MdFileOperationRequest(
            MdFileOperationKind.Copy, "home", "notes.txt", "home", string.Empty),
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(collision.HasConflict);
        Assert.Equal("notes.txt", collision.ResultPath);

        var copy = await provider.ExecuteAsync(new MdFileOperationRequest(
            MdFileOperationKind.Copy, "home", "notes.txt", "home", string.Empty,
            ConflictPolicy: MdFileConflictPolicy.KeepBoth),
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(copy.Succeeded);
        Assert.Equal("notes (2).txt", copy.ResultPath);
        Assert.Equal("hello file browser", File.ReadAllText(Path.Combine(directory.Path, "notes (2).txt")));

        var failedReplacement = await provider.ExecuteAsync(new MdFileOperationRequest(
            MdFileOperationKind.Copy, "home", "missing.txt", "home", string.Empty,
            ConflictPolicy: MdFileConflictPolicy.Overwrite),
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(failedReplacement.Succeeded);
        Assert.Equal("hello file browser", File.ReadAllText(Path.Combine(directory.Path, "notes.txt")));
        Assert.DoesNotContain(Directory.GetFiles(directory.Path), path => Path.GetFileName(path).EndsWith(".md3backup", StringComparison.Ordinal));
    }

    [AvaloniaFact]
    public async Task Local_Thumbnail_Provider_Decodes_A_Bounded_Image_Thumbnail()
    {
        using var directory = new TemporaryDirectory();
        var imagePath = Path.Combine(directory.Path, "landscape.png");
        using (var image = AssetLoader.Open(new Uri("avares://Md3.Avalonia.Gallery/Assets/Media/file-explorer-sample.png")))
        using (var output = File.Create(imagePath))
            image.CopyTo(output);
        var provider = new MdLocalFileSystemProvider([new MdFileLocation("photos", "Photos", directory.Path)]);
        var entry = (await provider.GetEntriesAsync("photos", string.Empty,
            cancellationToken: TestContext.Current.CancellationToken)).Single();

        var thumbnail = Assert.IsType<Bitmap>(await provider.GetThumbnailAsync("photos", entry,
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(256, thumbnail.PixelSize.Width);
        Assert.True(thumbnail.PixelSize.Height < 200);
        thumbnail.Dispose();
    }

    [Fact]
    public async Task Local_Delete_Reports_Item_Progress_And_Stops_When_Cancelled()
    {
        using var directory = new TemporaryDirectory();
        using var cancellation = new CancellationTokenSource();
        var source = Path.Combine(directory.Path, "Source");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "first.txt"), "first");
        File.WriteAllText(Path.Combine(source, "second.txt"), "second");
        var provider = new MdLocalFileSystemProvider([new MdFileLocation("home", "Home", directory.Path)]);
        var progressReports = new List<MdFileOperationProgress>();
        var progress = new InlineProgress<MdFileOperationProgress>(report =>
        {
            progressReports.Add(report);
            if (report.CompletedItems == 1) cancellation.Cancel();
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await provider.ExecuteAsync(new MdFileOperationRequest(MdFileOperationKind.Delete, "home", "Source"),
                progress, cancellation.Token);
        });

        var firstReport = Assert.Single(progressReports);
        Assert.Equal(1L, firstReport.CompletedItems);
        Assert.Equal(3L, firstReport.TotalItems);
        Assert.Single(Directory.GetFiles(source));
    }

    [Fact]
    public async Task Local_Delete_Cancellation_Between_Last_Child_And_Parent_Removal_Is_Observed()
    {
        using var directory = new TemporaryDirectory();
        using var cancellation = new CancellationTokenSource();
        var source = Path.Combine(directory.Path, "Source");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "only.txt"), "keep parent on cancellation");
        var provider = new MdLocalFileSystemProvider([new MdFileLocation("home", "Home", directory.Path)]);
        var progress = new InlineProgress<MdFileOperationProgress>(_ => cancellation.Cancel());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await provider.ExecuteAsync(new MdFileOperationRequest(MdFileOperationKind.Delete, "home", "Source"),
                progress, cancellation.Token);
        });

        Assert.True(Directory.Exists(source));
        Assert.Empty(Directory.GetFiles(source));
    }

    [Fact]
    public async Task Local_Provider_Does_Not_Follow_Symbolic_Links_Outside_The_Root()
    {
        if (OperatingSystem.IsWindows()) return;
        using var directory = new TemporaryDirectory();
        using var external = new TemporaryDirectory();
        var source = Path.Combine(directory.Path, "Source");
        var destination = Path.Combine(directory.Path, "Destination");
        Directory.CreateDirectory(source);
        Directory.CreateDirectory(destination);
        File.WriteAllText(Path.Combine(source, "note.txt"), "safe file");
        File.WriteAllText(Path.Combine(external.Path, "secret.txt"), "outside the root");
        Directory.CreateSymbolicLink(Path.Combine(source, "outside"), external.Path);
        var removableLink = Path.Combine(source, "linked-secret.txt");
        File.CreateSymbolicLink(removableLink, Path.Combine(external.Path, "secret.txt"));
        var danglingTarget = Path.Combine(external.Path, "created-outside-root");
        Directory.CreateSymbolicLink(Path.Combine(directory.Path, "dangling"), danglingTarget);
        var provider = new MdLocalFileSystemProvider([new MdFileLocation("home", "Home", directory.Path)]);

        var entries = await provider.GetEntriesAsync("home", "Source",
            cancellationToken: TestContext.Current.CancellationToken);
        var link = Assert.Single(entries, entry => entry.Name == "outside");
        Assert.True(link.IsSymbolicLink);
        Assert.False(link.CanNavigate);
        var unlinkResult = await provider.ExecuteAsync(new MdFileOperationRequest(
            MdFileOperationKind.Delete, "home", "Source/linked-secret.txt"),
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(unlinkResult.Succeeded);
        Assert.False(File.Exists(removableLink));
        Assert.Equal("outside the root", File.ReadAllText(Path.Combine(external.Path, "secret.txt")));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await provider.GetEntriesAsync("home", "Source/outside",
                cancellationToken: TestContext.Current.CancellationToken);
        });

        var copy = await provider.ExecuteAsync(new MdFileOperationRequest(
            MdFileOperationKind.Copy, "home", "Source", "home", "Destination"),
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(copy.Succeeded);
        Assert.Contains("symbolic link", copy.ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.False(Directory.Exists(Path.Combine(destination, "Source")));
        Assert.Equal("outside the root", File.ReadAllText(Path.Combine(external.Path, "secret.txt")));

        var deleteLinkContainer = await provider.ExecuteAsync(new MdFileOperationRequest(
            MdFileOperationKind.Delete, "home", "Source"),
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(deleteLinkContainer.Succeeded);
        Assert.False(Directory.Exists(source));
        Assert.Equal("outside the root", File.ReadAllText(Path.Combine(external.Path, "secret.txt")));

        var createThroughDanglingLink = await provider.ExecuteAsync(new MdFileOperationRequest(
            MdFileOperationKind.CreateDirectory, "home", string.Empty, NewName: "dangling"),
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(createThroughDanglingLink.Succeeded);
        Assert.False(Directory.Exists(danglingTarget));
    }

    [Fact]
    public async Task Local_Provider_Rejects_Copying_A_Folder_Into_Its_Own_Descendant()
    {
        using var directory = new TemporaryDirectory();
        var source = Path.Combine(directory.Path, "Source");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "note.txt"), "do not recurse");
        var provider = new MdLocalFileSystemProvider([new MdFileLocation("home", "Home", directory.Path)]);

        var result = await provider.ExecuteAsync(new MdFileOperationRequest(
            MdFileOperationKind.Copy, "home", "Source", "home", "Source"),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Contains("descendant", result.ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.False(Directory.Exists(Path.Combine(source, "Source")));
    }

    [Fact]
    public async Task Text_Preview_Is_Bounded_And_Move_Reports_A_Relative_Result()
    {
        using var source = new TemporaryDirectory();
        using var destination = new TemporaryDirectory();
        var text = new string('x', 32_000);
        File.WriteAllText(Path.Combine(source.Path, "large.txt"), text);
        var provider = new MdLocalFileSystemProvider(
        [
            new MdFileLocation("source", "Source", source.Path),
            new MdFileLocation("destination", "Destination", destination.Path)
        ]);
        var entry = (await provider.GetEntriesAsync("source", string.Empty,
            cancellationToken: TestContext.Current.CancellationToken)).Single();
        var preview = await provider.GetPreviewAsync("source", entry,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(preview?.Text);
        Assert.True(preview!.Text!.Length < 16_500);
        Assert.EndsWith("…", preview!.Text!);

        var result = await provider.ExecuteAsync(new MdFileOperationRequest(
            MdFileOperationKind.Move, "source", "large.txt", "destination", string.Empty),
            new InlineProgress<MdFileOperationProgress>(_ => throw new InvalidOperationException("Progress observer failure.")),
            TestContext.Current.CancellationToken);
        Assert.True(result.Succeeded);
        Assert.Equal("large.txt", result.ResultPath);
        Assert.False(File.Exists(Path.Combine(source.Path, "large.txt")));
        Assert.Equal(text, File.ReadAllText(Path.Combine(destination.Path, "large.txt")));
    }

    [AvaloniaFact]
    public async Task Switching_Providers_Discards_An_Entry_Load_That_Completes_Late()
    {
        var oldResult = new TaskCompletionSource<IReadOnlyList<MdFileEntry>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var oldProvider = new ControlledProvider("Old", oldResult);
        var newEntry = new MdFileEntry("new.txt", "new.txt", "new.txt", MdFileEntryKind.File);
        var newProvider = new ControlledProvider("New", entries: [newEntry]);
        var explorer = new MdFileExplorer();

        explorer.Provider = oldProvider;
        await oldProvider.EntriesRequested.Task.WaitAsync(TimeSpan.FromSeconds(5));
        explorer.Provider = newProvider;
        await newProvider.EntriesRequested.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("New", explorer.CurrentLocation?.Name);
        Assert.False(explorer.IsLoading);
        Assert.Contains(explorer.Entries, entry => entry.Name == "new.txt");

        oldResult.SetResult([new MdFileEntry("old.txt", "old.txt", "old.txt", MdFileEntryKind.File)]);
        await Task.Yield();
        Dispatcher.UIThread.RunJobs();

        Assert.False(explorer.IsLoading);
        Assert.Single(explorer.Entries, entry => entry.Name == "new.txt");
        Assert.DoesNotContain(explorer.Entries, entry => entry.Name == "old.txt");
    }

    [AvaloniaFact]
    public async Task Explorer_Loads_Provider_Entries_And_Uses_Adaptive_Layout()
    {
        using var directory = new TemporaryDirectory();
        Directory.CreateDirectory(Path.Combine(directory.Path, "Documents"));
        File.WriteAllText(Path.Combine(directory.Path, "Welcome.md"), "# Welcome");
        var explorer = new MdFileExplorer
        {
            Provider = new MdLocalFileSystemProvider([new MdFileLocation("sample", "Sample files", directory.Path)])
        };
        var window = new Window { Width = 1040, Height = 700, Content = explorer };
        window.Styles.Add(new ExtraTheme());
        window.Show();
        try
        {
            await explorer.InitializeAsync();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Sample files", explorer.CurrentLocation?.Name);
            Assert.Contains(explorer.Entries, entry => entry.Name == "Welcome.md");
            Assert.Contains(explorer.GetVisualDescendants().OfType<MdFileItemsView>(), view => view.ViewMode == MdFileViewMode.List);
            var preview = explorer.GetVisualDescendants().OfType<MdFilePreviewPane>().Single();
            Assert.True(preview.IsVisible);
            window.Width = 420;
            Dispatcher.UIThread.RunJobs();
            Assert.False(preview.IsVisible);
            Assert.Contains(explorer.GetVisualDescendants().OfType<MdFileItemsView>(), view => view.IsCompact);
            window.Width = 1040;
            Dispatcher.UIThread.RunJobs();
            Assert.True(preview.IsVisible);
            var itemsView = explorer.GetVisualDescendants().OfType<MdFileItemsView>().Single();
            Assert.False(itemsView.IsCompact);
            var welcome = explorer.Entries.Single(entry => entry.Name == "Welcome.md");
            itemsView.SelectedItem = welcome;
            Dispatcher.UIThread.RunJobs();
            var openButton = preview.GetVisualDescendants().OfType<MdButton>()
                .Single(button => AutomationProperties.GetName(button) == "Open selected file");
            Assert.True(openButton.IsVisible);
            var opened = new TaskCompletionSource<MdFileEntry>(TaskCreationOptions.RunContinuationsAsynchronously);
            explorer.EntryOpened += (_, entry) => opened.TrySetResult(entry);
            PointerInput.Click(openButton);
            Assert.Same(welcome, await opened.Task.WaitAsync(TimeSpan.FromSeconds(5)));

            explorer.SetViewMode(MdFileViewMode.Details);
            var ascendingName = explorer.GetVisualDescendants().OfType<MdButton>()
                .Single(button => AutomationProperties.GetName(button) == "Name, sorted ascending");
            Assert.Contains("↑", ascendingName.Content?.ToString());
            explorer.SortBy(MdFileSortKey.Name);
            var descendingName = explorer.GetVisualDescendants().OfType<MdButton>()
                .Single(button => AutomationProperties.GetName(button) == "Name, sorted descending");
            Assert.Contains("↓", descendingName.Content?.ToString());
            Assert.NotNull(window.CaptureRenderedFrame());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Explorer_Navigation_Tracks_Back_Forward_Up_And_Clears_Forward_History_On_New_Path()
    {
        using var directory = new TemporaryDirectory();
        Directory.CreateDirectory(Path.Combine(directory.Path, "Documents", "Notes"));
        Directory.CreateDirectory(Path.Combine(directory.Path, "Pictures"));
        var explorer = new MdFileExplorer
        {
            Provider = new MdLocalFileSystemProvider([new MdFileLocation("home", "Home", directory.Path)])
        };
        await explorer.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.False(explorer.CanNavigateBack);
        Assert.False(explorer.CanNavigateForward);
        Assert.False(explorer.CanNavigateUp);

        await explorer.NavigateToAsync("home", "Documents", cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(explorer.CanNavigateBack);
        Assert.True(explorer.CanNavigateUp);
        await explorer.NavigateToAsync("home", "Documents/Notes", cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(explorer.CanNavigateBack);

        Assert.True(await explorer.NavigateBackAsync(TestContext.Current.CancellationToken));
        Assert.Equal("Documents", explorer.CurrentPath);
        Assert.True(explorer.CanNavigateForward);
        Assert.True(await explorer.NavigateForwardAsync(TestContext.Current.CancellationToken));
        Assert.Equal("Documents/Notes", explorer.CurrentPath);
        Assert.False(explorer.CanNavigateForward);

        Assert.True(await explorer.NavigateUpAsync(TestContext.Current.CancellationToken));
        Assert.Equal("Documents", explorer.CurrentPath);
        await explorer.NavigateToAsync("home", "Pictures", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Pictures", explorer.CurrentPath);
        Assert.False(explorer.CanNavigateForward);
        Assert.True(explorer.CanNavigateUp);
    }

    private sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    private sealed class ControlledProvider : IMdFileSystemProvider
    {
        private readonly IReadOnlyList<MdFileEntry> _entries;
        private readonly TaskCompletionSource<IReadOnlyList<MdFileEntry>>? _pendingEntries;

        public ControlledProvider(
            string name,
            TaskCompletionSource<IReadOnlyList<MdFileEntry>>? pendingEntries = null,
            IReadOnlyList<MdFileEntry>? entries = null)
        {
            _pendingEntries = pendingEntries;
            _entries = entries ?? Array.Empty<MdFileEntry>();
            Location = new MdFileLocation(name.ToLowerInvariant(), name, $"virtual:{name}");
        }

        public MdFileLocation Location { get; }
        public TaskCompletionSource<bool> EntriesRequested { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ValueTask<IReadOnlyList<MdFileLocation>> GetLocationsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<IReadOnlyList<MdFileLocation>>([Location]);
        }

        public ValueTask<IReadOnlyList<MdFileEntry>> GetEntriesAsync(
            string locationId,
            string relativePath,
            bool includeHidden = false,
            CancellationToken cancellationToken = default)
        {
            EntriesRequested.TrySetResult(true);
            return _pendingEntries is null
                ? ValueTask.FromResult(_entries)
                : new ValueTask<IReadOnlyList<MdFileEntry>>(_pendingEntries.Task);
        }

        public ValueTask<MdFileOperationResult> ExecuteAsync(
            MdFileOperationRequest request,
            IProgress<MdFileOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(MdFileOperationResult.Failure("Not supported by this test provider."));

        public ValueTask<MdFilePreview?> GetPreviewAsync(
            string locationId,
            MdFileEntry entry,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<MdFilePreview?>(null);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"md3-file-explorer-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
