using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>One commit in a <see cref="MdCommitGraph"/>.</summary>
/// <remarks>
/// Commits are supplied in display order, newest first, like the output of <c>git log</c>.
/// Parents are referenced by <see cref="Id"/>; parents that are not present in the list
/// (a truncated history) simply start a fresh lane. A commit with two or more parents is
/// rendered as a merge: it keeps its first parent's lane and the other parents' lanes end
/// at the merge with a curved connector.
/// </remarks>
public sealed class MdCommitNode
{
    /// <summary>Gets or sets the commit identifier (a short or full SHA) used to resolve parents.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the identifier abbreviated to seven characters, for row display.</summary>
    public string ShortId => Id.Length >= 7 ? Id[..7] : Id;

    /// <summary>Gets or sets the commit message, shown as the primary row text.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets or sets the author name, shown as secondary row text.</summary>
    public string? Author { get; init; }

    /// <summary>Gets or sets the commit date, shown as secondary row text.</summary>
    public DateTimeOffset? Date { get; init; }

    /// <summary>Gets or sets the parent commit identifiers, mainline first.</summary>
    public IReadOnlyList<string> ParentIds { get; init; } = Array.Empty<string>();

    /// <summary>Gets or sets an optional tag (for example <c>v1.4.0</c>) shown beside the row.</summary>
    public string? Tag { get; init; }

    /// <summary>
    /// Gets or sets an optional color override for the node. When null the node uses the
    /// deterministic lane color from the <c>Md.CommitGraph.Lane.Brush.N</c> tokens.
    /// </summary>
    public IBrush? Color { get; init; }
}
