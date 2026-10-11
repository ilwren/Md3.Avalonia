using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Md3.Avalonia.Chat;
using Md3.Avalonia.Extra.Infrastructure;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>Phase 1 (Markdown provider and streaming) and Phase 2 (attachments and image viewer) of the chat plan.</summary>
public sealed class MdChatPhase12Tests
{
    private sealed class MarkerMarkdownProvider : IMdMarkdownProvider
    {
        public int Calls { get; private set; }

        public Control Render(string text, bool isStreaming)
        {
            Calls++;
            return new TextBlock { Text = (isStreaming ? "streaming:" : "final:") + text };
        }
    }

    [AvaloniaFact]
    public void Message_Defaults_Are_Not_Streaming_And_Have_No_Attachments()
    {
        var message = new MdChatMessage("1", MdChatMessageRole.Assistant, "hello", DateTimeOffset.Now);

        Assert.False(message.IsStreaming);
        Assert.Null(message.Attachments);
    }

    [AvaloniaFact]
    public void Presenter_Without_Provider_Keeps_Plain_Text_Content()
    {
        var presenter = new MdChatMessagePresenter
        {
            Message = new MdChatMessage("1", MdChatMessageRole.Assistant, "plain", DateTimeOffset.Now),
        };

        Assert.Equal<object?>("plain", presenter.RichContent);
        Assert.Equal("plain", presenter.TextContent);
    }

    [AvaloniaFact]
    public void Presenter_With_Provider_Renders_Text_And_Passes_Streaming_State()
    {
        var provider = new MarkerMarkdownProvider();
        var presenter = new MdChatMessagePresenter { MarkdownProvider = provider };

        presenter.Message = new MdChatMessage("1", MdChatMessageRole.Assistant, "partial", DateTimeOffset.Now, IsStreaming: true);
        var streaming = Assert.IsType<TextBlock>(presenter.RichContent);
        Assert.Equal("streaming:partial", streaming.Text);

        presenter.Message = new MdChatMessage("1", MdChatMessageRole.Assistant, "partial text", DateTimeOffset.Now);
        var final = Assert.IsType<TextBlock>(presenter.RichContent);
        Assert.Equal("final:partial text", final.Text);
        Assert.True(provider.Calls >= 2);
    }

    [AvaloniaFact]
    public void Provider_Is_Not_Called_For_Non_Text_Content()
    {
        var provider = new MarkerMarkdownProvider();
        var presenter = new MdChatMessagePresenter { MarkdownProvider = provider };
        var control = new Border();

        presenter.Message = new MdChatMessage("2", MdChatMessageRole.Assistant, control, DateTimeOffset.Now);

        Assert.Same(control, presenter.RichContent);
        Assert.Equal(0, provider.Calls);
    }

    [AvaloniaFact]
    public void Streamed_Replacement_Keeps_The_Selection_On_The_New_Record()
    {
        var messages = new ObservableCollection<MdChatMessage>
        {
            new("1", MdChatMessageRole.User, "question", DateTimeOffset.Now),
            new("2", MdChatMessageRole.Assistant, "a", DateTimeOffset.Now, IsStreaming: true),
        };
        var chat = new MdChatView { MessagesSource = messages };
        var window = new Window { Width = 480, Height = 320, Content = chat };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            chat.ToggleMessageSelection(messages[1]);
            var statusBefore = chat.StatusText;

            messages[1] = messages[1] with { Content = "a longer streamed answer", IsStreaming = true };
            Dispatcher.UIThread.RunJobs();

            var selected = Assert.Single(chat.SelectedMessages);
            Assert.Equal("2", selected.Id);
            Assert.Equal("a longer streamed answer", selected.Content);
            Assert.Same(messages[1], selected);
            // A same-Id replacement is streaming progress, not a new message: the status text must not change.
            Assert.Equal(statusBefore, chat.StatusText);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Attachment_Without_Image_Is_A_File_Card_With_A_Readable_Size()
    {
        var file = new MdChatAttachment("f1", MdChatAttachmentKind.File, "notes.pdf", 2048);

        Assert.False(file.IsImage);
        Assert.True(file.IsFile);
        Assert.Equal("2 KB", file.SizeText);
        Assert.Equal("\ue66d", file.Glyph);
    }

    [AvaloniaFact]
    public void Attachment_Size_Is_Empty_When_Unknown()
    {
        var file = new MdChatAttachment("f2", MdChatAttachmentKind.Audio, "voice.m4a");

        Assert.Equal(string.Empty, file.SizeText);
        Assert.Equal("\ueb82", file.Glyph);
    }

    [AvaloniaFact]
    public void Image_Viewer_Clamps_Zoom_To_The_Supported_Range()
    {
        var viewer = new MdImageViewer();

        viewer.Zoom = 100;
        Assert.Equal(MdImageViewer.MaxZoom, viewer.Zoom);

        viewer.Zoom = 0.1;
        Assert.Equal(MdImageViewer.MinZoom, viewer.Zoom);

        viewer.Zoom = 2;
        viewer.ResetView();
        Assert.Equal(MdImageViewer.MinZoom, viewer.Zoom);
    }

    [AvaloniaFact]
    public void Image_Viewer_Escape_Requests_Close()
    {
        var viewer = new MdImageViewer();
        var requests = 0;
        viewer.CloseRequested += (_, _) => requests++;
        var window = new Window { Width = 240, Height = 240, Content = viewer };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            viewer.Focus();
            window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            window.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, requests);
        }
        finally { window.Close(); }
    }

    private sealed class FakeImageProvider : IMdImageProvider
    {
        private readonly Func<int, Task<IImage?>> _load;

        public FakeImageProvider(Func<int, Task<IImage?>> load) => _load = load;

        public int Calls { get; private set; }

        public Task<IImage?> LoadImageAsync(string uri, CancellationToken cancellationToken) => _load(++Calls);
    }

    [AvaloniaFact]
    public void Image_Attachment_Is_An_Image_Before_It_Loads()
    {
        var picture = new MdChatAttachment("i1", MdChatAttachmentKind.Image, "pic.png", null, null, "gallery://x");

        Assert.True(picture.IsImage);
        Assert.False(picture.IsFile);
    }

    [AvaloniaFact]
    public void Attachment_Uri_Is_Loaded_Through_The_Image_Provider()
    {
        var picture = new DrawingImage();
        var provider = new FakeImageProvider(_ => Task.FromResult<IImage?>(picture));
        var control = new MdAttachmentImage { Provider = provider };

        control.Attachment = new MdChatAttachment("i2", MdChatAttachmentKind.Image, "pic.png", null, null, "gallery://pic");

        Assert.Equal(1, provider.Calls);
        Assert.Equal(MdImageLoadState.Loaded, control.LoadState);
        Assert.Same(picture, control.Source);
    }

    [AvaloniaFact]
    public void Failed_Load_Offers_A_Retry_That_Reloads()
    {
        var picture = new DrawingImage();
        var provider = new FakeImageProvider(call => Task.FromResult<IImage?>(call == 1 ? null : picture));
        var control = new MdAttachmentImage { Provider = provider };

        control.Attachment = new MdChatAttachment("i3", MdChatAttachmentKind.Image, "pic.png", null, null, "gallery://flaky");
        Assert.Equal(MdImageLoadState.Failed, control.LoadState);
        Assert.False(string.IsNullOrEmpty(control.StatusText));

        control.Retry();

        Assert.Equal(2, provider.Calls);
        Assert.Equal(MdImageLoadState.Loaded, control.LoadState);
        Assert.Same(picture, control.Source);
    }

    [AvaloniaFact]
    public void Provider_Exception_Is_Reported_As_Failed()
    {
        var provider = new FakeImageProvider(_ => Task.FromException<IImage?>(new InvalidOperationException("offline")));
        var control = new MdAttachmentImage { Provider = provider };

        control.Attachment = new MdChatAttachment("i4", MdChatAttachmentKind.Image, "pic.png", null, null, "gallery://down");

        Assert.Equal(MdImageLoadState.Failed, control.LoadState);
    }

    [AvaloniaFact]
    public void Fenced_Code_Renders_As_A_Code_Block_Between_Prose()
    {
        var panel = Assert.IsType<StackPanel>(new MdFencedCodeMarkdownProvider().Render("Intro\n```csharp\nvar x = 1;\n```\nOutro", isStreaming: false));

        Assert.Equal(3, panel.Children.Count);
        Assert.Equal("Intro", Assert.IsType<SelectableTextBlock>(panel.Children[0]).Text);
        var code = Assert.IsType<MdCodeBlock>(panel.Children[1]);
        Assert.Equal("csharp", code.Language);
        Assert.Equal("var x = 1;", code.Code);
        Assert.Equal("Outro", Assert.IsType<SelectableTextBlock>(panel.Children[2]).Text);
    }

    [AvaloniaFact]
    public void Unclosed_Fence_While_Streaming_Runs_To_The_End()
    {
        var panel = Assert.IsType<StackPanel>(new MdFencedCodeMarkdownProvider().Render("```py\nprint(1)\nprint(2)", isStreaming: true));

        var block = Assert.IsType<MdCodeBlock>(Assert.Single(panel.Children));
        Assert.Equal("py", block.Language);
        Assert.Equal("print(1)\nprint(2)", block.Code);
    }
}
