using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>Shared modal capture, containment, background isolation and focus-restoration logic.</summary>
internal sealed class MdModalFocusController
{
    private readonly Control _owner;
    private Control? _scope;
    private Control? _background;
    private Control? _focusBeforeOpen;
    private TopLevel? _topLevel;
    private readonly List<BackgroundState> _isolatedBackground = [];
    private KeyboardNavigationMode _scopeNavigationMode;
    private bool _scopeFocusable;
    private bool _active;
    private bool _redirectPending;
    private int _failedRedirects;
    private bool _redirectAbandoned;

    /// <summary>
    /// How many times in a row a redirect may fail to put focus inside the scope before the
    /// controller stops re-arming. Three leaves room for a surface that needs a layout pass
    /// before anything in it is focusable, without letting the dispatcher queue feed itself.
    /// </summary>
    private const int MaxFailedRedirects = 3;

    public MdModalFocusController(Control owner)
    {
        _owner = owner;
    }

    public bool IsActive => _active;

    /// <summary>
    /// True once the controller has stopped pulling focus back into the scope because the scope
    /// refuses it. Exposed so a test can prove the guard engaged rather than infer it from the
    /// suite not hanging.
    /// </summary>
    public bool HasAbandonedFocusRedirect => _redirectAbandoned;

    public void Update(bool active, Control? scope, Control? background = null, Control? initialFocus = null)
    {
        if (!active || scope is null || !_owner.IsAttachedToVisualTree())
        {
            Deactivate();
            return;
        }

        if (_active && ReferenceEquals(_scope, scope) && ReferenceEquals(_background, background))
        {
            if (!ContainsFocus(scope)) FocusInitial(initialFocus);
            return;
        }

        Deactivate(restoreFocus: false);
        _scope = scope;
        _background = background;
        _topLevel = TopLevel.GetTopLevel(_owner);
        _focusBeforeOpen = _topLevel?.FocusManager?.GetFocusedElement() as Control;
        _scopeNavigationMode = KeyboardNavigation.GetTabNavigation(scope);
        _scopeFocusable = scope.Focusable;
        KeyboardNavigation.SetTabNavigation(scope, KeyboardNavigationMode.Cycle);
        scope.SetCurrentValue(InputElement.FocusableProperty, true);

        if (background is not null)
            Isolate(background);
        else
            IsolateExternalSiblings();

        _topLevel?.AddHandler(InputElement.GotFocusEvent, OnTopLevelGotFocus,
            RoutingStrategies.Bubble, handledEventsToo: true);
        _active = true;
        _failedRedirects = 0;
        _redirectAbandoned = false;
        FocusInitial(initialFocus);
    }

    public void Deactivate() => Deactivate(restoreFocus: true);

    private void Deactivate(bool restoreFocus)
    {
        if (!_active)
        {
            if (restoreFocus) _focusBeforeOpen = null;
            return;
        }

        _topLevel?.RemoveHandler(InputElement.GotFocusEvent, OnTopLevelGotFocus);
        if (_scope is not null)
        {
            KeyboardNavigation.SetTabNavigation(_scope, _scopeNavigationMode);
            _scope.SetCurrentValue(InputElement.FocusableProperty, _scopeFocusable);
        }
        foreach (var state in _isolatedBackground)
        {
            state.Control.SetCurrentValue(InputElement.IsHitTestVisibleProperty, state.IsHitTestVisible);
            state.Control.SetCurrentValue(AutomationProperties.AccessibilityViewProperty, state.AccessibilityView);
        }
        _isolatedBackground.Clear();

        var restoreTarget = restoreFocus ? _focusBeforeOpen : null;
        _active = false;
        _redirectPending = false;
        _failedRedirects = 0;
        _redirectAbandoned = false;
        _scope = null;
        _background = null;
        _topLevel = null;
        _focusBeforeOpen = null;

        if (restoreTarget is { } target && target.IsAttachedToVisualTree() && target.IsEffectivelyEnabled)
            Dispatcher.UIThread.Post(() => target.Focus(NavigationMethod.Unspecified), DispatcherPriority.Input);
    }

    private void OnTopLevelGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (!_active || _scope is null || e.Source is not Visual source) return;
        if (IsWithin(source, _scope))
        {
            // Containment is working again, so a scope that was briefly unfocusable — one still
            // waiting for its first layout pass, say — gets its redirects back.
            _failedRedirects = 0;
            _redirectAbandoned = false;
            return;
        }

        // The redirect stays pending until the focus attempt itself has run. Clearing it when
        // this Input-priority job ran, as it used to, let the attempt's own focus traffic arm a
        // second redirect, which armed a third: Dispatcher.RunJobs() then never drained because
        // the queue kept refilling itself.
        if (_redirectPending || _redirectAbandoned) return;
        _redirectPending = true;
        Dispatcher.UIThread.Post(() =>
        {
            if (!_active)
            {
                _redirectPending = false;
                return;
            }
            FocusInitial(null, isRedirect: true);
        }, DispatcherPriority.Input);
    }

    private void IsolateExternalSiblings()
    {
        Visual branch = _owner;
        while (branch.GetVisualParent() is Visual parent && parent is not TopLevel)
        {
            foreach (var sibling in parent.GetVisualChildren().OfType<Control>())
            {
                if (!ReferenceEquals(sibling, branch)) Isolate(sibling);
            }
            branch = parent;
        }
    }

    private void Isolate(Control control)
    {
        IsolateOne(control);
        foreach (var descendant in control.GetVisualDescendants().OfType<Control>())
            IsolateOne(descendant);
    }

    private void IsolateOne(Control control)
    {
        if (_isolatedBackground.Any(state => ReferenceEquals(state.Control, control))) return;
        _isolatedBackground.Add(new BackgroundState(
            control,
            control.IsHitTestVisible,
            AutomationProperties.GetAccessibilityView(control)));
        control.SetCurrentValue(InputElement.IsHitTestVisibleProperty, false);
        control.SetCurrentValue(AutomationProperties.AccessibilityViewProperty, AccessibilityView.Raw);
    }

    private void FocusInitial(Control? preferred, bool isRedirect = false)
    {
        var scope = _scope;
        if (scope is null)
        {
            if (isRedirect) _redirectPending = false;
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                if (!_active || !ReferenceEquals(scope, _scope)) return;
                if (preferred is not null && IsWithin(preferred, scope) && CanFocus(preferred) && preferred.Focus()) return;
                var target = scope.GetVisualDescendants().OfType<Control>()
                    .Where(CanFocus)
                    .OrderBy(KeyboardNavigation.GetTabIndex)
                    .FirstOrDefault();
                if (target?.Focus(NavigationMethod.Tab) != true) scope.Focus(NavigationMethod.Tab);
            }
            finally
            {
                if (isRedirect) CompleteRedirect(scope);
            }
        }, DispatcherPriority.Loaded);
    }

    private void CompleteRedirect(Control scope)
    {
        _redirectPending = false;
        if (!_active || !ReferenceEquals(scope, _scope)) return;
        if (ContainsFocus(scope))
        {
            _failedRedirects = 0;
            return;
        }

        // Nothing in the scope would take focus. A modal opened before it was attached, an empty
        // scope, or one whose only focusable children are disabled all land here, and re-arming
        // would keep the dispatcher busy forever without ever succeeding. Give up instead; the
        // scope still has its containment, isolation and Escape handling.
        if (++_failedRedirects >= MaxFailedRedirects) _redirectAbandoned = true;
    }

    private static bool ContainsFocus(Control scope) =>
        scope.IsFocused || scope.GetVisualDescendants().OfType<Control>().Any(control => control.IsFocused);

    private static bool CanFocus(Control control) =>
        control.Focusable && control.IsVisible && control.IsEffectivelyEnabled &&
        KeyboardNavigation.GetIsTabStop(control);

    private static bool IsWithin(Visual source, Visual ancestor) =>
        ReferenceEquals(source, ancestor) || source.GetVisualAncestors().Contains(ancestor);

    private readonly record struct BackgroundState(
        Control Control,
        bool IsHitTestVisible,
        AccessibilityView AccessibilityView);
}
