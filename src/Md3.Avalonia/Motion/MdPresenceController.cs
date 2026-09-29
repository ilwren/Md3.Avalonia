using Avalonia.Threading;

namespace Md3.Avalonia.Motion;

/// <summary>
/// Keeps transient content mounted while its exit transition completes. Reopening cancels the
/// pending removal, which makes quick open/close/open sequences reversible instead of flickering.
/// </summary>
public sealed class MdPresenceController
{
    private readonly Action<bool> _setPresence;
    private readonly DispatcherTimer _exitTimer;
    private bool _isPresent;

    public MdPresenceController(Action<bool> setPresence)
    {
        _setPresence = setPresence;
        _exitTimer = new DispatcherTimer();
        _exitTimer.Tick += OnExitTimerTick;
    }

    public bool IsPresent => _isPresent;

    public void Update(bool isOpen, TimeSpan exitDuration)
    {
        _exitTimer.Stop();
        if (isOpen)
        {
            SetPresence(true);
            return;
        }

        if (!_isPresent) return;
        if (exitDuration <= TimeSpan.Zero)
        {
            SetPresence(false);
            return;
        }

        _exitTimer.Interval = exitDuration;
        _exitTimer.Start();
    }

    public void Initialize(bool isOpen)
    {
        _exitTimer.Stop();
        SetPresence(isOpen);
    }

    public void Stop() => _exitTimer.Stop();

    private void OnExitTimerTick(object? sender, EventArgs e)
    {
        _exitTimer.Stop();
        SetPresence(false);
    }

    private void SetPresence(bool value)
    {
        if (_isPresent == value) return;
        _isPresent = value;
        _setPresence(value);
    }
}
