using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

internal sealed class MdRangeSliderAutomationPeer : ControlAutomationPeer
{
    private IReadOnlyList<AutomationPeer>? _thumbs;

    public MdRangeSliderAutomationPeer(MdRangeSlider owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;

    protected override IReadOnlyList<AutomationPeer>? GetChildrenCore() => _thumbs ??=
    [
        new MdRangeSliderThumbAutomationPeer((MdRangeSlider)Owner, lower: true),
        new MdRangeSliderThumbAutomationPeer((MdRangeSlider)Owner, lower: false)
    ];
}

internal sealed class MdRangeSliderThumbAutomationPeer : ControlAutomationPeer, IRangeValueProvider
{
    private readonly bool _lower;

    public MdRangeSliderThumbAutomationPeer(MdRangeSlider owner, bool lower) : base(owner)
    {
        _lower = lower;
    }

    private MdRangeSlider Slider => (MdRangeSlider)Owner;

    public bool IsReadOnly => false;
    public double Minimum => _lower ? Slider.Minimum : Slider.LowerValue;
    public double Maximum => _lower ? Slider.UpperValue : Slider.Maximum;
    public double Value => _lower ? Slider.LowerValue : Slider.UpperValue;
    public double LargeChange => Math.Max(Slider.Step, double.Epsilon) * 10;
    public double SmallChange => Math.Max(Slider.Step, double.Epsilon);

    public void SetValue(double value)
    {
        EnsureEnabled();
        Slider.SetThumbValue(_lower, Math.Clamp(value, Minimum, Maximum));
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Thumb;

    protected override string? GetNameCore() => _lower ? Slider.LowerThumbName : Slider.UpperThumbName;

    protected override string? GetAutomationIdCore() => _lower ? "LowerThumb" : "UpperThumb";

    protected override bool HasKeyboardFocusCore() => Slider.IsFocused && Slider.IsThumbActive(_lower);

    protected override void SetFocusCore() => Slider.ActivateThumb(_lower);

    protected override Rect GetBoundingRectangleCore()
    {
        var local = new Rect(Slider.GetThumbCenterX(_lower) - 24, Slider.ThumbCenterY - 24, 48, 48);
        if (TopLevel.GetTopLevel(Slider) is not Visual root) return default;
        var transform = Slider.TransformToVisual(root);
        return transform.HasValue ? local.TransformToAABB(transform.Value) : default;
    }
}
