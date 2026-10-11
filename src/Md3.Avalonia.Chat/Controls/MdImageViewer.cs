using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Chat;

/// <summary>
/// Zoomable, pannable surface for a chat picture. The mouse wheel and the plus and minus keys zoom,
/// dragging pans while zoomed, 0 resets the view, and Escape raises <see cref="CloseRequested"/> so
/// the host can dismiss the viewer.
/// </summary>
public sealed class MdImageViewer : Decorator
{
    /// <summary>Smallest zoom factor; the picture is fitted to the viewer at this level.</summary>
    public const double MinZoom = 1.0;

    /// <summary>Largest zoom factor.</summary>
    public const double MaxZoom = 8.0;

    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<MdImageViewer, IImage?>(nameof(Source));

    public static readonly StyledProperty<double> ZoomProperty =
        AvaloniaProperty.Register<MdImageViewer, double>(nameof(Zoom), MinZoom,
            coerce: (_, zoom) => Math.Clamp(zoom, MinZoom, MaxZoom));

    private readonly Image _image = new() { Stretch = Stretch.Uniform };
    private Point _dragOrigin;
    private Vector _offset;
    private bool _dragging;

    static MdImageViewer()
    {
        SourceProperty.Changed.AddClassHandler<MdImageViewer>((viewer, _) => viewer._image.Source = viewer.Source);
        ZoomProperty.Changed.AddClassHandler<MdImageViewer>((viewer, _) => viewer.OnZoomChanged());
    }

    public MdImageViewer()
    {
        Child = _image;
        ClipToBounds = true;
        Focusable = true;
        AutomationProperties.SetName(this, MdLocalization.GetString("ImageViewer", this));
        UpdateTransform();
    }

    /// <summary>The picture to show.</summary>
    public IImage? Source { get => GetValue(SourceProperty); set => SetValue(SourceProperty, value); }

    /// <summary>Current zoom factor, clamped to <see cref="MinZoom"/> through <see cref="MaxZoom"/>.</summary>
    public double Zoom { get => GetValue(ZoomProperty); set => SetValue(ZoomProperty, value); }

    /// <summary>Raised when the user presses Escape.</summary>
    public event EventHandler? CloseRequested;

    /// <summary>Raised when the user presses Left; the application shows the previous picture.</summary>
    public event EventHandler? PreviousRequested;

    /// <summary>Raised when the user presses Right; the application shows the next picture.</summary>
    public event EventHandler? NextRequested;

    /// <summary>Restores the fitted view at the default zoom.</summary>
    public void ResetView()
    {
        _offset = default;
        Zoom = MinZoom;
        UpdateTransform();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (e.Delta.Y == 0) return;
        Zoom *= Math.Pow(1.1, e.Delta.Y);
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || Zoom <= MinZoom) return;
        _dragging = true;
        _dragOrigin = e.GetPosition(this);
        e.Pointer.Capture(this);
        Focus();
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_dragging) return;
        var position = e.GetPosition(this);
        _offset += position - _dragOrigin;
        _dragOrigin = position;
        UpdateTransform();
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_dragging) return;
        _dragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.Key)
        {
            case Key.Add:
            case Key.OemPlus:
                Zoom *= 1.25;
                break;
            case Key.Subtract:
            case Key.OemMinus:
                Zoom /= 1.25;
                break;
            case Key.D0:
            case Key.NumPad0:
                ResetView();
                break;
            case Key.Escape:
                CloseRequested?.Invoke(this, EventArgs.Empty);
                break;
            case Key.Left:
                PreviousRequested?.Invoke(this, EventArgs.Empty);
                break;
            case Key.Right:
                NextRequested?.Invoke(this, EventArgs.Empty);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    private void OnZoomChanged()
    {
        if (Zoom <= MinZoom) _offset = default;
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        var zoom = Zoom;
        var group = new TransformGroup();
        group.Children.Add(new ScaleTransform(zoom, zoom));
        group.Children.Add(new TranslateTransform(_offset.X, _offset.Y));
        _image.RenderTransform = group;
    }
}
