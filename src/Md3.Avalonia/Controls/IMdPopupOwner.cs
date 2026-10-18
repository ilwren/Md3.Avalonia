using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>Implemented by Material controls that own one temporary popup surface. 由拥有一个 Material 临时弹层的控件实现。</summary>
/// <remarks>
/// <para>Built-in popup controls in Core and Extra report their open state to an internal
/// coordinator. It closes the previously opened built-in popup on the same UI
/// <see cref="Dispatcher"/> and dismisses an open popup when the offset of an ancestor
/// <see cref="ScrollViewer"/> changes.</para>
/// <para>This interface exposes only the state property; the coordinator is internal. Implementing
/// <see cref="IMdPopupOwner"/> alone does not register a third-party control with the library's
/// mutual-exclusion behavior. Custom controls remain responsible for their own popup lifecycle.</para>
/// <para>Core 与 Extra 中的内置弹层控件会将打开状态通知库内协调器。协调器会在同一 UI
/// <see cref="Dispatcher"/> 上关闭先前打开的内置弹层；锚点祖先中的 <see cref="ScrollViewer"/> 偏移量变化时，也会关闭当前弹层。</para>
/// <para>此接口只公开状态属性，协调器是内部实现。第三方控件仅实现 <see cref="IMdPopupOwner"/> 并不会自动加入库内互斥行为；自定义控件仍需自行管理弹层生命周期。</para>
/// </remarks>
public interface IMdPopupOwner
{
    /// <summary>Gets or sets whether this Material control's temporary popup is open. 获取或设置该 Material 控件的临时弹层是否打开。</summary>
    bool IsMaterialPopupOpen { get; set; }
}

/// <summary>Internal hook used when another popup must replace this owner in the same UI turn.</summary>
internal interface IMdPopupPresenceOwner
{
    void ClosePopupImmediately();
}

internal static class MdPopupCoordinator
{
    private static WeakReference<IMdPopupOwner>? s_activeOwner;
    private static Dispatcher? s_activeDispatcher;
    private static int s_activeThreadId;

    public static void NotifyStateChanged(IMdPopupOwner owner)
    {
        var dispatcher = Dispatcher.UIThread;
        var threadId = Environment.CurrentManagedThreadId;
        if (!ReferenceEquals(s_activeDispatcher, dispatcher) || s_activeThreadId != threadId)
        {
            s_activeOwner = null;
            s_activeDispatcher = dispatcher;
            s_activeThreadId = threadId;
        }

        if (owner.IsMaterialPopupOpen)
        {
            if (s_activeOwner is not null &&
                s_activeOwner.TryGetTarget(out var previous) &&
                !ReferenceEquals(previous, owner))
            {
                previous.IsMaterialPopupOpen = false;
                if (previous is IMdPopupPresenceOwner presenceOwner)
                    presenceOwner.ClosePopupImmediately();
            }

            s_activeOwner = new WeakReference<IMdPopupOwner>(owner);
            WatchAncestorScrolling(owner);
        }
        else if (s_activeOwner is not null &&
                 s_activeOwner.TryGetTarget(out var active) &&
                 ReferenceEquals(active, owner))
        {
            s_activeOwner = null;
            StopWatchingScroll();
        }
    }

    // A desktop popup is a window placed in screen coordinates when it opens; it does not track
    // its anchor afterwards. Scrolling the page therefore slides the field out from under a
    // surface that stays put, which reads as the popup drifting away from its control. Android
    // renders into the TopLevel overlay and moves with the page, so the drift is desktop-only.
    // Every platform agrees that a temporary surface should not outlive the scroll, so dismiss.
    private static readonly List<ScrollViewer> s_watchedScrollers = new();
    private static EventHandler<ScrollChangedEventArgs>? s_scrollHandler;

    private static void WatchAncestorScrolling(IMdPopupOwner owner)
    {
        StopWatchingScroll();
        if (owner is not Visual visual) return;

        s_scrollHandler = (_, e) =>
        {
            // ScrollChanged also fires when the extent or viewport is remeasured - including the
            // relayout the popup itself can trigger as it opens. Only a real offset change counts.
            if (e.OffsetDelta == default) return;
            if (owner.IsMaterialPopupOpen) owner.IsMaterialPopupOpen = false;
        };

        foreach (var scroller in visual.GetVisualAncestors().OfType<ScrollViewer>())
        {
            scroller.ScrollChanged += s_scrollHandler;
            s_watchedScrollers.Add(scroller);
        }
    }

    private static void StopWatchingScroll()
    {
        if (s_scrollHandler is null) return;
        var handler = s_scrollHandler;
        var scrollers = s_watchedScrollers.ToArray();
        s_scrollHandler = null;
        s_watchedScrollers.Clear();
        foreach (var scroller in scrollers) scroller.ScrollChanged -= handler;
    }
}
