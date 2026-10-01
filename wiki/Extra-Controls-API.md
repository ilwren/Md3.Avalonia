# Extra Controls API Reference (`Md3.Avalonia.Extra`)

The `Md3.Avalonia.Extra` package provides advanced UI controls inspired by popular open-source Flutter ecosystem widgets (such as `flutter_quill`, `flutter_chat_ui`, `flutter_slidable`, etc.).

To use these controls, add the XML namespace:
```xml
xmlns:extra="using:Md3.Avalonia.Extra.Controls"
```

---

## 1. `MdRichEditor` (Rich Text Editor)
Inspired by `flutter_quill`, `MdRichEditor` provides a complete Material Design 3 formatting toolbar and live WYSIWYG Markdown document renderer.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Adapter` | `IMdRichEditorAdapter?` | Document engine adapter (e.g. `MdTextBoxRichEditorAdapter`) |
| `ToolbarCommands` | `IEnumerable<MdRichEditorCommand>?` | Collection of active toolbar formatting commands |
| `EditorStateChanged` | `event EventHandler?` | Raised whenever formatting command or text mutates |

### Commands Supported (`MdRichEditorCommand`):
`Bold`, `Italic`, `Underline`, `StrikeThrough`, `Heading` (H1, H2, H3), `Quote`, `Code` (Inline & Block), `Link`, `BulletedList`, `NumberedList`, `HorizontalRule`, `ClearFormatting`, `Undo`, `Redo`.

### Usage Example:
```xml
<extra:MdRichEditor x:Name="RichEditor" MinHeight="240">
    <Grid ColumnDefinitions="*,*" ColumnSpacing="16">
        <md:MdTextBox x:Name="EditorText" AcceptsReturn="True" TextWrapping="Wrap"
                      Label="Source Markdown" Variant="Outlined"
                      Text="## Release Notes&#10;Try **bold**, _italic_, lists, and quotes." />
        <Border Grid.Column="1" Padding="16" CornerRadius="12"
                Background="{DynamicResource Md.Sys.Color.SurfaceContainerLow.Brush}"
                BorderBrush="{DynamicResource Md.Sys.Color.OutlineVariant.Brush}" BorderThickness="1">
            <StackPanel x:Name="RichPreview" Spacing="6" />
        </Border>
    </Grid>
</extra:MdRichEditor>
```
```csharp
// Wire adapter in C#
RichEditor.Adapter = new MdTextBoxRichEditorAdapter(EditorText, RichPreview);
```

---

## 2. `MdChatView` & `MdChatMessagePresenter` (Chat UI)
Inspired by `flutter_chat_ui` and `dash_chat_2`, `MdChatView` provides a provider-neutral chat shell with asymmetric message bubbles, avatars, delivery status, quoted reply card, quick suggestions, and floating pill composer.

### `MdChatMessage` Record
```csharp
public sealed record MdChatMessage(
    string Id,
    MdChatMessageRole Role,        // User, Assistant, System
    object? Content,
    DateTimeOffset Timestamp,
    string? Sender = null,
    MdAsyncRequestState State = MdAsyncRequestState.Data, // Data, Busy, Error
    string? ReplyToId = null,
    object? ReplyPreview = null,
    string? ErrorText = null,
    string? AvatarGlyph = null,
    string? Initials = null);
```

### `MdChatView` Properties & Events
| Member | Type | Description |
| :--- | :--- | :--- |
| `MessagesSource` | `IEnumerable?` | Message collection binding (`ObservableCollection<MdChatMessage>`) |
| `ComposerText` | `string?` | Two-way text binding for message composer |
| `SuggestionsSource` | `IEnumerable?` | Quick reply assist chips displayed above composer |
| `QuotedMessage` | `MdChatMessage?` | Currently active quoted message for replies |
| `MessageSubmitted` | `event EventHandler<string>?` | Raised when user presses Enter or Send |
| `AttachmentRequested`| `event EventHandler?` | Raised when user clicks the attachment button |
| `QuoteRequested` | `event EventHandler<MdChatMessage>?` | Raised when user selects a message to quote |
| `DeleteRequested`| `event EventHandler<IReadOnlyList<MdChatMessage>>?` | Raised when user deletes selected messages |
| `RetryRequested` | `event EventHandler<MdChatMessage>?` | Raised when user taps retry on a failed bubble |

### Usage Example:
```xml
<extra:MdChatView x:Name="Chat" Height="500"
                  MessageSubmitted="ChatSubmitted"
                  AttachmentRequested="ChatAttachmentRequested"
                  QuoteRequested="ChatQuoteRequested"
                  DeleteRequested="ChatDeleteRequested"
                  RetryRequested="ChatRetryRequested" />
```

---

## 3. `MdDataGrid` (Enterprise Data Grid)
High-performance virtualized grid with column sorting, inline cell editing, and clipboard copy.

```xml
<extra:MdDataGrid x:Name="DataGrid" Height="300" />
```
```csharp
DataGrid.Columns.Add(new MdDataGridColumn { Header = "Name", PropertyName = "Name", Width = new GridLength(2, GridUnitType.Star) });
DataGrid.Columns.Add(new MdDataGridColumn { Header = "Score", PropertyName = "Score", IsEditable = true });
DataGrid.DataSource = employeesList;
```

---

## 4. `MdBreadcrumb` (Path Navigation)
Cross-platform breadcrumb supporting icons, custom separators, overflow collapsing, and command binding.

```xml
<extra:MdBreadcrumb ItemsSource="{Binding BreadcrumbPath}" Separator="›" />
```

---

## 5. `MdPinInput` (Verification & OTP Input)
Segmented PIN / SMS verification code input with focus forwarding, automatic paste handling, masking, and keyboard navigation.

```xml
<extra:MdPinInput Length="6" Completed="OnPinCompleted" />
```

---

## 6. `MdTreeView` (Virtual Tree Hierarchy)
Expandable hierarchy view with expansion angle transitions, node invocation, and RTL mirroring.

```xml
<extra:MdTreeView x:Name="Tree" NodeInvoked="OnNodeInvoked" />
```

---

## 7. `MdTimeline` (Process & Event Timeline)
Vertical and horizontal event timeline with neutral, active, completed, and error item states.

```xml
<extra:MdTimeline ItemsSource="{Binding TimelineEvents}" Orientation="Vertical" />
```
