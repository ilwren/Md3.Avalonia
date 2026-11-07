using Avalonia;
using Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Components;

/// <summary>
/// The API note a component page carries above its examples: the control type, the members the
/// page actually exercises, and one sentence about how they fit together.
/// </summary>
/// <remarks>
/// Built imperatively rather than with a collection binding: the members are identifiers, not
/// content. A <c>DataTemplate</c> would put them through compiled bindings and through the
/// localization walker, and both would have to be talked out of translating them.
/// </remarks>
public partial class ApiNotes : UserControl
{
    public static readonly StyledProperty<string> ControlProperty =
        AvaloniaProperty.Register<ApiNotes, string>(nameof(Control), string.Empty);

    /// <summary>Comma- or semicolon-separated member names, e.g. <c>Variant, Size, IsChecked</c>.</summary>
    public static readonly StyledProperty<string> MembersProperty =
        AvaloniaProperty.Register<ApiNotes, string>(nameof(Members), string.Empty);

    /// <summary>The page's own sentence about the members. Authored in the page so it is translated there.</summary>
    public static readonly StyledProperty<object?> NotesProperty =
        AvaloniaProperty.Register<ApiNotes, object?>(nameof(Notes));

    private static readonly char[] Separators = [',', ';'];

    static ApiNotes()
    {
        ControlProperty.Changed.AddClassHandler<ApiNotes>((notes, _) => notes.UpdateControl());
        MembersProperty.Changed.AddClassHandler<ApiNotes>((notes, _) => notes.UpdateMembers());
        NotesProperty.Changed.AddClassHandler<ApiNotes>((notes, _) => notes.UpdateNotes());
    }

    public ApiNotes()
    {
        InitializeComponent();
        UpdateControl();
        UpdateMembers();
        UpdateNotes();
    }

    /// <summary>Gets or sets the control type the page demonstrates, e.g. <c>MdButton</c>.</summary>
    public string Control
    {
        get => GetValue(ControlProperty);
        set => SetValue(ControlProperty, value);
    }

    /// <summary>Gets or sets the comma- or semicolon-separated list of members to show.</summary>
    public string Members
    {
        get => GetValue(MembersProperty);
        set => SetValue(MembersProperty, value);
    }

    /// <summary>Gets or sets the page content shown under the member list.</summary>
    public object? Notes
    {
        get => GetValue(NotesProperty);
        set => SetValue(NotesProperty, value);
    }

    private void UpdateControl()
    {
        if (TypeLabel is not null) TypeLabel.Text = Control;
    }

    private void UpdateMembers()
    {
        if (MemberHost is null) return;
        MemberHost.Children.Clear();
        foreach (var member in (Members ?? string.Empty)
                     .Split(Separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var label = new TextBlock { Text = member };
            // Identifiers stay verbatim in both languages; the chips are code, not prose.
            label.Classes.Add("verbatim");
            MemberHost.Children.Add(new Border
            {
                Classes = { "api-member" },
                Child = label,
            });
        }
    }

    private void UpdateNotes()
    {
        if (NotesHost is not null) NotesHost.Content = Notes;
    }
}
