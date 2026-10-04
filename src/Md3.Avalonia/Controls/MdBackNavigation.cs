using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Routes the platform's back request to the top-most dismissible Material surface.
/// </summary>
/// <remarks>
/// <para>
/// On desktop a dialog, sheet, drawer or menu is closed with Escape. Android has no Escape key:
/// the system back button and, from API 33, the predictive back gesture arrive as
/// <see cref="TopLevel.BackRequestedEvent"/>. Without this every modal surface in the library was
/// unreachable by the one gesture Android users actually use to go back.
/// </para>
/// <para>
/// Surfaces register while they are open and unregister when they close. The most recently
/// registered handler is offered the request first, so nested surfaces unwind in the order the
/// user opened them. A handler returns <c>true</c> once it has consumed the request, which marks
/// the routed event handled and stops the platform from also popping the activity.
/// </para>
/// </remarks>
public static class MdBackNavigation
{
    private static readonly ConditionalWeakTable<TopLevel, BackRegistry> Registries = new();

    /// <summary>
    /// Registers <paramref name="handler"/> for the top level hosting <paramref name="anchor"/>.
    /// Dispose the result to unregister; disposing twice is safe.
    /// </summary>
    /// <returns>
    /// A registration. When <paramref name="anchor"/> is not attached to a top level there is
    /// nothing to listen to, and an inert registration is returned rather than throwing.
    /// </returns>
    public static IDisposable Register(Visual anchor, Func<bool> handler)
    {
        ArgumentNullException.ThrowIfNull(anchor);
        ArgumentNullException.ThrowIfNull(handler);

        var topLevel = TopLevel.GetTopLevel(anchor);
        if (topLevel is null) return InertRegistration.Instance;
        return Registries.GetValue(topLevel, static level => new BackRegistry(level)).Add(handler);
    }

    /// <summary>
    /// Runs the registered handlers for the top level hosting <paramref name="anchor"/>, most
    /// recently registered first.
    /// </summary>
    /// <returns>True when a handler consumed the request.</returns>
    public static bool RequestBack(Visual anchor)
    {
        ArgumentNullException.ThrowIfNull(anchor);
        var topLevel = TopLevel.GetTopLevel(anchor);
        return topLevel is not null && RequestBack(topLevel);
    }

    /// <summary>Runs the registered handlers for <paramref name="topLevel"/>.</summary>
    /// <returns>True when a handler consumed the request.</returns>
    public static bool RequestBack(TopLevel topLevel)
    {
        ArgumentNullException.ThrowIfNull(topLevel);
        return Registries.TryGetValue(topLevel, out var registry) && registry.Invoke();
    }

    /// <summary>Handlers currently registered for <paramref name="topLevel"/>.</summary>
    public static int GetHandlerCount(TopLevel topLevel)
    {
        ArgumentNullException.ThrowIfNull(topLevel);
        return Registries.TryGetValue(topLevel, out var registry) ? registry.Count : 0;
    }

    private sealed class BackRegistry
    {
        private readonly List<Func<bool>> _handlers = [];

        internal BackRegistry(TopLevel topLevel) =>
            topLevel.AddHandler(TopLevel.BackRequestedEvent, OnBackRequested, RoutingStrategies.Bubble);

        internal int Count => _handlers.Count;

        internal IDisposable Add(Func<bool> handler)
        {
            _handlers.Add(handler);
            return new Registration(this, handler);
        }

        internal bool Invoke()
        {
            // A handler closes its surface, which unregisters it while this loop is running.
            // Walk a snapshot and skip anything that has since gone, so one back request can
            // never skip a surface or run a handler that has already been torn down.
            var snapshot = _handlers.ToArray();
            for (var index = snapshot.Length - 1; index >= 0; index--)
            {
                var handler = snapshot[index];
                if (!_handlers.Contains(handler)) continue;
                if (handler()) return true;
            }

            return false;
        }

        private void Remove(Func<bool> handler) => _handlers.Remove(handler);

        private void OnBackRequested(object? sender, RoutedEventArgs e)
        {
            if (Invoke()) e.Handled = true;
        }

        private sealed class Registration(BackRegistry registry, Func<bool> handler) : IDisposable
        {
            private BackRegistry? _registry = registry;

            public void Dispose()
            {
                _registry?.Remove(handler);
                _registry = null;
            }
        }
    }

    private sealed class InertRegistration : IDisposable
    {
        internal static readonly InertRegistration Instance = new();
        public void Dispose() { }
    }
}

/// <summary>
/// Keeps a <see cref="MdBackNavigation"/> handler registered for exactly as long as a surface is
/// open. Call <see cref="Update"/> whenever the surface's open state changes.
/// </summary>
/// <remarks>
/// Registration is deferred until the owner is attached to a top level, because a control can be
/// constructed — and even opened — before it has a window to register against.
/// </remarks>
public sealed class MdBackScope : IDisposable
{
    private readonly Visual _owner;
    private readonly Func<bool> _handler;
    private IDisposable? _registration;
    private bool _isActive;

    /// <summary>Creates a scope for <paramref name="owner"/> that invokes <paramref name="handler"/>.</summary>
    public MdBackScope(Visual owner, Func<bool> handler)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _owner.AttachedToVisualTree += OnAttached;
        _owner.DetachedFromVisualTree += OnDetached;
    }

    /// <summary>True while the handler is registered with a top level.</summary>
    public bool IsRegistered => _registration is not null;

    /// <summary>Registers or unregisters the handler to match the surface's open state.</summary>
    public void Update(bool isActive)
    {
        _isActive = isActive;
        Sync();
    }

    /// <summary>Unregisters the handler and stops tracking the owner.</summary>
    public void Dispose()
    {
        _isActive = false;
        _owner.AttachedToVisualTree -= OnAttached;
        _owner.DetachedFromVisualTree -= OnDetached;
        Unregister();
    }

    private void OnAttached(object? sender, VisualTreeAttachmentEventArgs e) => Sync();

    private void OnDetached(object? sender, VisualTreeAttachmentEventArgs e) => Unregister();

    private void Sync()
    {
        if (_isActive && _registration is null && _owner.GetVisualRoot() is not null)
            _registration = MdBackNavigation.Register(_owner, _handler);
        else if (!_isActive) Unregister();
    }

    private void Unregister()
    {
        _registration?.Dispose();
        _registration = null;
    }
}
