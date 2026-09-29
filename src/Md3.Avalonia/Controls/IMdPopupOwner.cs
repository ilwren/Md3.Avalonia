using Avalonia.Threading;

namespace Md3.Avalonia.Controls;

/// <summary>Implemented by Material controls that own one temporary popup surface.</summary>
public interface IMdPopupOwner
{
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
        }
        else if (s_activeOwner is not null &&
                 s_activeOwner.TryGetTarget(out var active) &&
                 ReferenceEquals(active, owner))
        {
            s_activeOwner = null;
        }
    }
}
