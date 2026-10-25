using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using DotNetCampus.Inking;
using DotNetCampus.Inking.Contexts;
using SkiaSharp;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A Material signature pad: a handwriting surface with the chrome a signature needs around it - a
/// baseline to sign on, a hint that disappears once there is ink, a clear button, and an eraser
/// mode - all filled from Material tokens rather than from the ink canvas's own defaults.
/// 基于 Material 设计令牌的手写签名控件，提供签名基线、提示、清除操作与橡皮擦模式。
/// </summary>
/// <remarks>
/// <para>
/// The strokes themselves are drawn by <c>DotNetCampus.AvaloniaInkCanvas</c>, which renders with
/// Skia and handles pressure, smoothing and erasing. What this control adds is the part upstream
/// leaves to the host: it defaults to red ink at 10 units thick, has no baseline, no empty state and
/// no way to start over, and none of that is a Material decision. <see cref="InkBrush"/> and
/// <see cref="InkThickness"/> are pushed into the canvas's own settings, so an application that uses
/// a bare <c>InkCanvas</c> can take the same two lines from
/// <c>MaterialInkCanvasTheme</c> instead of adopting this control.
/// </para>
/// <para>
/// Upstream refuses to switch <c>EditingMode</c> while a stroke is in progress and keeps that state
/// private, so <see cref="IsErasing"/> is only safe to change between strokes - which is what a
/// toolbar button does anyway. Setting it mid-stroke propagates upstream's
/// <see cref="InvalidOperationException"/> rather than swallowing it, because silently ignoring a
/// mode change is worse than reporting one.
/// </para>
/// </remarks>
[PseudoClasses(":empty", ":erasing")]
public sealed class MdSignaturePad : TemplatedControl
{
    private InkCanvas? _canvas;
    private Button? _clearButton;
    private EventHandler<AvaloniaSkiaInkCanvasStrokeCollectedEventArgs>? _strokeCollected;
    private EventHandler<RoutedEventArgs>? _clearClicked;

    /// <summary>
    /// The ink colour. Only a solid brush can be expressed as a Skia colour, so a gradient or an
    /// image brush falls back to black rather than throwing.
    /// </summary>
    public static readonly StyledProperty<IBrush?> InkBrushProperty =
        AvaloniaProperty.Register<MdSignaturePad, IBrush?>(nameof(InkBrush));

    /// <summary>Stroke width in device-independent pixels, coerced to a positive number.</summary>
    public static readonly StyledProperty<double> InkThicknessProperty =
        AvaloniaProperty.Register<MdSignaturePad, double>(nameof(InkThickness), 3,
            coerce: static (_, value) => double.IsNaN(value) || value <= 0 ? 1 : value);

    /// <summary>Switches the canvas to point erasing. Only safe to change between strokes.</summary>
    public static readonly StyledProperty<bool> IsErasingProperty =
        AvaloniaProperty.Register<MdSignaturePad, bool>(nameof(IsErasing));

    /// <summary>Whether the title row offers a clear button.</summary>
    public static readonly StyledProperty<bool> ShowClearButtonProperty =
        AvaloniaProperty.Register<MdSignaturePad, bool>(nameof(ShowClearButton), true);

    /// <summary>Whether the signature baseline is drawn.</summary>
    public static readonly StyledProperty<bool> ShowBaselineProperty =
        AvaloniaProperty.Register<MdSignaturePad, bool>(nameof(ShowBaseline), true);

    /// <summary>Text shown while the pad is empty. Null hides the hint.</summary>
    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<MdSignaturePad, string?>(nameof(Placeholder));

    /// <summary>
    /// Raised after a stroke is committed, which is the moment a signature form becomes valid. The
    /// canvas reports the stroke itself upstream; what a host usually needs is only that one
    /// finished, so this carries no stroke data.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> StrokeCompletedEvent =
        RoutedEvent.Register<MdSignaturePad, RoutedEventArgs>(nameof(StrokeCompleted), RoutingStrategies.Bubble);

    /// <inheritdoc cref="StrokeCompletedEvent"/>
    public event EventHandler<RoutedEventArgs>? StrokeCompleted
    {
        add => AddHandler(StrokeCompletedEvent, value);
        remove => RemoveHandler(StrokeCompletedEvent, value);
    }

    /// <summary>Raised after <see cref="Clear"/> has removed every stroke.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClearedEvent =
        RoutedEvent.Register<MdSignaturePad, RoutedEventArgs>(nameof(Cleared), RoutingStrategies.Bubble);

    /// <inheritdoc cref="ClearedEvent"/>
    public event EventHandler<RoutedEventArgs>? Cleared
    {
        add => AddHandler(ClearedEvent, value);
        remove => RemoveHandler(ClearedEvent, value);
    }

    static MdSignaturePad()
    {
        InkBrushProperty.Changed.AddClassHandler<MdSignaturePad>((pad, _) => pad.ApplyInkSettings());
        InkThicknessProperty.Changed.AddClassHandler<MdSignaturePad>((pad, _) => pad.ApplyInkSettings());
        IsErasingProperty.Changed.AddClassHandler<MdSignaturePad>((pad, _) => pad.ApplyEditingMode());
    }

    /// <inheritdoc cref="InkBrushProperty"/>
    public IBrush? InkBrush { get => GetValue(InkBrushProperty); set => SetValue(InkBrushProperty, value); }

    /// <inheritdoc cref="InkThicknessProperty"/>
    public double InkThickness { get => GetValue(InkThicknessProperty); set => SetValue(InkThicknessProperty, value); }

    /// <inheritdoc cref="IsErasingProperty"/>
    public bool IsErasing { get => GetValue(IsErasingProperty); set => SetValue(IsErasingProperty, value); }

    /// <inheritdoc cref="ShowClearButtonProperty"/>
    public bool ShowClearButton { get => GetValue(ShowClearButtonProperty); set => SetValue(ShowClearButtonProperty, value); }

    /// <inheritdoc cref="ShowBaselineProperty"/>
    public bool ShowBaseline { get => GetValue(ShowBaselineProperty); set => SetValue(ShowBaselineProperty, value); }

    /// <inheritdoc cref="PlaceholderProperty"/>
    public string? Placeholder { get => GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }

    /// <summary>How many committed strokes the pad holds.</summary>
    public int StrokeCount => _canvas?.Strokes.Count ?? 0;

    /// <summary>Whether nothing has been drawn yet, which is what shows the placeholder.</summary>
    public bool IsEmpty => StrokeCount == 0;

    /// <summary>
    /// Removes every stroke and raises <see cref="Cleared"/>. Upstream exposes no clear of its own -
    /// only add and remove - so each stroke is removed and then disposed, and the bitmap cache is
    /// dropped because removing a stroke does not invalidate it.
    /// </summary>
    public void Clear()
    {
        if (_canvas?.AvaloniaSkiaInkCanvas is not { } canvas) return;

        // Snapshot first: removing mutates the very list being walked.
        foreach (var stroke in canvas.StaticStrokeList.ToArray())
        {
            canvas.RemoveStaticStroke(stroke);
            stroke.Dispose();
        }

        canvas.InvalidateBitmapCache();
        canvas.InvalidateVisual();
        UpdateEmptyState();
        RaiseEvent(new RoutedEventArgs(ClearedEvent, this));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachParts();
        base.OnApplyTemplate(e);

        _canvas = e.NameScope.Find<InkCanvas>("PART_Canvas");
        _clearButton = e.NameScope.Find<Button>("PART_ClearButton");

        if (_canvas is { } canvas)
        {
            _strokeCollected = (_, _) =>
            {
                UpdateEmptyState();
                RaiseEvent(new RoutedEventArgs(StrokeCompletedEvent, this));
            };
            canvas.StrokeCollected += _strokeCollected;
        }

        if (_clearButton is { } clear)
        {
            _clearClicked = (_, args) =>
            {
                Clear();
                args.Handled = true;
            };
            clear.Click += _clearClicked;
        }

        ApplyInkSettings();
        ApplyEditingMode();
        UpdateEmptyState();
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        DetachParts();
        base.OnDetachedFromVisualTree(e);
    }

    private void DetachParts()
    {
        if (_canvas is { } canvas && _strokeCollected is { } collected) canvas.StrokeCollected -= collected;
        if (_clearButton is { } clear && _clearClicked is { } clicked) clear.Click -= clicked;
        _canvas = null;
        _clearButton = null;
        _strokeCollected = null;
        _clearClicked = null;
    }

    private void ApplyInkSettings()
    {
        if (_canvas?.AvaloniaSkiaInkCanvas.Settings is not { } settings) return;
        settings.InkColor = ToSkColor(InkBrush);
        settings.InkThickness = (float)InkThickness;
    }

    private void ApplyEditingMode()
    {
        PseudoClasses.Set(":erasing", IsErasing);
        if (_canvas is null) return;
        _canvas.EditingMode = IsErasing ? InkCanvasEditingMode.EraseByPoint : InkCanvasEditingMode.Ink;
    }

    private void UpdateEmptyState() => PseudoClasses.Set(":empty", IsEmpty);

    private static SKColor ToSkColor(IBrush? brush) =>
        brush is ISolidColorBrush solid
            ? new SKColor(solid.Color.R, solid.Color.G, solid.Color.B, solid.Color.A)
            : SKColors.Black;
}
