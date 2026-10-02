using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

internal sealed class MdTimeDialAutomationPeer : ControlAutomationPeer, IRangeValueProvider
{
    private IReadOnlyList<AutomationPeer>? _valuePeers;

    public MdTimeDialAutomationPeer(MdTimeDial owner) : base(owner)
    {
        owner.PropertyChanged += OnOwnerPropertyChanged;
    }

    private MdTimeDial Dial => (MdTimeDial)Owner;

    public bool IsReadOnly => false;
    public double Minimum => Dial.ActivePart == MdTimeDialPart.Hour && !Dial.Is24Hour ? 1 : 0;
    public double Maximum => Dial.ActivePart == MdTimeDialPart.Hour ? (Dial.Is24Hour ? 23 : 12) : 59;
    public double Value => Dial.AutomationValue;
    public double LargeChange => Dial.ActivePart == MdTimeDialPart.Minute ? 5 : 1;
    public double SmallChange => 1;

    public void SetValue(double value)
    {
        EnsureEnabled();
        Dial.SetAutomationValue((int)Math.Round(Math.Clamp(value, Minimum, Maximum)));
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;

    protected override IReadOnlyList<AutomationPeer>? GetChildrenCore()
    {
        if (_valuePeers is not null) return _valuePeers;
        var values = Dial.ActivePart == MdTimeDialPart.Minute
            ? Enumerable.Range(0, 12).Select(index => index * 5).ToArray()
            : Dial.Is24Hour
                ? Enumerable.Range(1, 12).Concat(Enumerable.Range(13, 11)).Append(0).ToArray()
                : Enumerable.Range(1, 12).ToArray();
        _valuePeers = values.Select((value, index) =>
            (AutomationPeer)new MdTimeDialValueAutomationPeer(Dial, value, index, values.Length)).ToArray();
        return _valuePeers;
    }

    private void OnOwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MdTimeDial.ActivePartProperty || e.Property == MdTimeDial.Is24HourProperty)
        {
            _valuePeers = null;
            InvalidateChildren();
        }
    }
}

internal sealed class MdTimeDialValueAutomationPeer : ControlAutomationPeer, IInvokeProvider, IToggleProvider
{
    private readonly int _value;
    private readonly int _index;
    private readonly int _count;

    public MdTimeDialValueAutomationPeer(MdTimeDial owner, int value, int index, int count) : base(owner)
    {
        _value = value;
        _index = index;
        _count = count;
    }

    private MdTimeDial Dial => (MdTimeDial)Owner;

    ToggleState IToggleProvider.ToggleState => IsSelected ? ToggleState.On : ToggleState.Off;

    public void Invoke()
    {
        EnsureEnabled();
        Dial.SetAutomationValue(_value);
    }

    void IToggleProvider.Toggle() => Invoke();

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.RadioButton;

    protected override string? GetNameCore()
    {
        var part = Dial.ActivePart == MdTimeDialPart.Hour ? "hour" : "minute";
        var selected = IsSelected ? ", selected" : string.Empty;
        return $"{_value:00} {part}, {_index + 1} of {_count}{selected}";
    }

    protected override string? GetAutomationIdCore() =>
        $"{(Dial.ActivePart == MdTimeDialPart.Hour ? "Hour" : "Minute")}_{_value}";

    protected override bool HasKeyboardFocusCore() => Dial.IsFocused && IsSelected;

    protected override void SetFocusCore()
    {
        Dial.SetAutomationValue(_value);
        Dial.Focus();
    }

    protected override Rect GetBoundingRectangleCore()
    {
        var local = GetLocalBounds();
        if (Dial.GetVisualRoot() is not Visual root) return default;
        var transform = Dial.TransformToVisual(root);
        return transform.HasValue ? local.TransformToAABB(transform.Value) : default;
    }

    private bool IsSelected => Dial.AutomationValue == _value ||
        (Dial.ActivePart == MdTimeDialPart.Hour && !Dial.Is24Hour &&
         Dial.AutomationValue % 12 == _value % 12);

    private Rect GetLocalBounds()
    {
        var size = Math.Min(Dial.Bounds.Width, Dial.Bounds.Height);
        var center = new Point(Dial.Bounds.Width / 2, Dial.Bounds.Height / 2);
        var radius = Math.Max(0, size / 2 - 2);
        var outerRadius = Math.Max(18, radius - 27);
        var innerRadius = Math.Max(12, radius - 64);
        var inner = Dial.ActivePart == MdTimeDialPart.Hour && Dial.Is24Hour && _value is 0 or >= 13;
        var slot = Dial.ActivePart == MdTimeDialPart.Minute ? _value / 5 : _value % 12;
        var angle = slot / 12d * Math.Tau;
        var labelCenter = new Point(
            center.X + Math.Sin(angle) * (inner ? innerRadius : outerRadius),
            center.Y - Math.Cos(angle) * (inner ? innerRadius : outerRadius));
        return new Rect(labelCenter.X - 24, labelCenter.Y - 24, 48, 48);
    }
}
