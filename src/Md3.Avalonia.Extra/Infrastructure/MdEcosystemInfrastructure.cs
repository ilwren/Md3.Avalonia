using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Extra.Infrastructure;

public enum MdEcosystemDensity { Comfortable, Compact, Touch }
public enum MdAsyncRequestState { Idle, Loading, Data, Empty, Error, Completed }
public enum MdOverlayAlignment { Start, Center, End }

/// <summary>Inherited density metadata for ecosystem controls without changing core Material identity.</summary>
public sealed class MdDensity : AvaloniaObject
{
    public static readonly AttachedProperty<MdEcosystemDensity> ModeProperty = AvaloniaProperty.RegisterAttached<MdDensity, StyledElement, MdEcosystemDensity>(
        "Mode", MdEcosystemDensity.Comfortable, inherits: true);
    private MdDensity() { }
    public static MdEcosystemDensity GetMode(AvaloniaObject target) => target.GetValue(ModeProperty);
    public static void SetMode(AvaloniaObject target, MdEcosystemDensity value) => target.SetValue(ModeProperty, value);
}

/// <summary>Provider-neutral async page request used by paging, search, chat, and calendar adapters.</summary>
public sealed record MdPageRequest(int PageKey, int PageSize, string? Query = null, CancellationToken CancellationToken = default);
public sealed record MdPageResult<T>(IReadOnlyList<T> Items, int? NextPageKey, bool IsLastPage = false);
public interface IMdPageProvider<T> { ValueTask<MdPageResult<T>> LoadPageAsync(MdPageRequest request); }

/// <summary>Shared shortcut descriptor suitable for ICommand/CommunityToolkit.MVVM.</summary>
public sealed record MdShortcutBinding(KeyGesture Gesture, ICommand Command, object? Parameter = null, string? Description = null);

/// <summary>Placement metadata consumed by popover/hover-card hosts; no platform popup type leaks into view models.</summary>
public sealed record MdOverlayPlacement(MdOverlayAlignment Horizontal = MdOverlayAlignment.Start, MdOverlayAlignment Vertical = MdOverlayAlignment.End, double OffsetX = 0, double OffsetY = 4, bool FlipToFit = true);

/// <summary>Captures a weak focus target before opening transient content and restores it after close.</summary>
public sealed class MdFocusReturnScope
{
    private WeakReference<InputElement>? _target;
    public void Capture(InputElement? target) { if (target is not null) _target = new WeakReference<InputElement>(target); }
    public bool Restore()
    {
        if (_target?.TryGetTarget(out var target) != true) return false;
        return target?.Focus() == true;
    }
}
