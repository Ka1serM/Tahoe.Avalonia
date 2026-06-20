using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace Avalonia.Themes.Tahoe.Controls;

public sealed class EdgeHighlight : Control
{
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<EdgeHighlight, CornerRadius>(nameof(CornerRadius));

    public static readonly StyledProperty<Thickness> EdgeThicknessProperty =
        AvaloniaProperty.Register<EdgeHighlight, Thickness>(nameof(EdgeThickness), new Thickness(0.5));

    public static readonly StyledProperty<double> HighlightOpacityProperty =
        AvaloniaProperty.Register<EdgeHighlight, double>(nameof(HighlightOpacity), 0.5);

    public static readonly StyledProperty<double> HighlightBlurRadiusProperty =
        AvaloniaProperty.Register<EdgeHighlight, double>(nameof(HighlightBlurRadius), 0.75);

    public static readonly StyledProperty<double> HighlightAngleProperty =
        AvaloniaProperty.Register<EdgeHighlight, double>(nameof(HighlightAngle), 45.0);

    public static readonly StyledProperty<double> HighlightFalloffProperty =
        AvaloniaProperty.Register<EdgeHighlight, double>(nameof(HighlightFalloff), 1.0);

    static EdgeHighlight()
    {
        AffectsRender<EdgeHighlight>(
            CornerRadiusProperty,
            EdgeThicknessProperty,
            HighlightOpacityProperty,
            HighlightBlurRadiusProperty,
            HighlightAngleProperty,
            HighlightFalloffProperty);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Thickness EdgeThickness
    {
        get => GetValue(EdgeThicknessProperty);
        set => SetValue(EdgeThicknessProperty, value);
    }

    public double HighlightOpacity
    {
        get => GetValue(HighlightOpacityProperty);
        set => SetValue(HighlightOpacityProperty, value);
    }

    public double HighlightBlurRadius
    {
        get => GetValue(HighlightBlurRadiusProperty);
        set => SetValue(HighlightBlurRadiusProperty, value);
    }

    public double HighlightAngle
    {
        get => GetValue(HighlightAngleProperty);
        set => SetValue(HighlightAngleProperty, value);
    }

    public double HighlightFalloff
    {
        get => GetValue(HighlightFalloffProperty);
        set => SetValue(HighlightFalloffProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = new Rect(Bounds.Size);
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var thickness = EdgeThickness;
        var stroke = Math.Max(Math.Max(thickness.Left, thickness.Top), Math.Max(thickness.Right, thickness.Bottom));
        if (stroke <= 0.001 || HighlightOpacity <= 0.001)
            return;

        context.Custom(new EdgeHighlightDrawOperation(
            bounds,
            CornerRadius,
            stroke,
            HighlightOpacity,
            HighlightBlurRadius,
            HighlightAngle,
            HighlightFalloff));
    }
}

internal sealed class EdgeHighlightDrawOperation : ICustomDrawOperation
{
    private const string ShaderUri = "avares://Avalonia.Themes.Tahoe/Assets/Shaders/TahoeEdgeHighlight.sksl";
    private static SKRuntimeEffect? s_effect;
    private static bool s_loaded;

    private readonly Rect _bounds;
    private readonly CornerRadius _cornerRadius;
    private readonly double _edgeThickness;
    private readonly double _opacity;
    private readonly double _blurRadius;
    private readonly double _angle;
    private readonly double _falloff;

    public EdgeHighlightDrawOperation(
        Rect bounds,
        CornerRadius cornerRadius,
        double edgeThickness,
        double opacity,
        double blurRadius,
        double angle,
        double falloff)
    {
        _bounds = bounds;
        _cornerRadius = cornerRadius;
        _edgeThickness = edgeThickness;
        _opacity = opacity;
        _blurRadius = blurRadius;
        _angle = angle;
        _falloff = falloff;
    }

    public Rect Bounds => _bounds;

    public void Dispose()
    {
    }

    public bool Equals(ICustomDrawOperation? other) => false;

    public bool HitTest(Point p) => false;

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature is null)
            return;

        LoadShader();
        if (s_effect is null)
            return;

        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;
        var size = new SKSize((float)_bounds.Width, (float)_bounds.Height);
        if (size.Width <= 0 || size.Height <= 0)
            return;

        var maxRadius = Math.Min(size.Width, size.Height) * 0.5f;
        var cornerRadii = GetCornerRadii(_cornerRadius, maxRadius);

        using var uniforms = new SKRuntimeEffectUniforms(s_effect);
        uniforms["size"] = new[] { size.Width, size.Height };
        uniforms["cornerRadii"] = cornerRadii;
        uniforms["color"] = new[] { 1.0f, 1.0f, 1.0f, (float)Math.Clamp(_opacity, 0.0, 1.0) };
        uniforms["angle"] = (float)(_angle * (Math.PI / 180.0));
        uniforms["falloff"] = (float)Math.Clamp(_falloff, 0.0, 8.0);

        using var children = new SKRuntimeEffectChildren(s_effect);
        using var shader = s_effect.ToShader(uniforms, children);
        if (shader is null)
            return;

        var blurRadius = (float)Math.Clamp(_blurRadius, 0.0, 20.0);
        using var maskFilter = blurRadius > 0.001f
            ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blurRadius)
            : null;

        using var paint = new SKPaint
        {
            Shader = shader,
            IsAntialias = true,
            BlendMode = SKBlendMode.Plus,
            Style = SKPaintStyle.Stroke,
            StrokeJoin = SKStrokeJoin.Round,
            StrokeCap = SKStrokeCap.Round,
            StrokeWidth = Math.Max(0.5f, (float)Math.Ceiling(_edgeThickness) * 2.0f),
            MaskFilter = maskFilter
        };

        var rect = SKRect.Create(0, 0, size.Width, size.Height);
        using var path = CreateRoundRectPath(rect, cornerRadii);

        const float safePad = 1.0f;
        canvas.Save();
        canvas.Translate(-safePad, -safePad);
        var layerBounds = SKRect.Create(0, 0, size.Width + safePad * 2.0f, size.Height + safePad * 2.0f);
        canvas.SaveLayer(layerBounds, null);

        canvas.Translate(safePad, safePad);
        canvas.ClipPath(path, SKClipOperation.Intersect, true);
        canvas.DrawPath(path, paint);

        canvas.Restore();
        canvas.Restore();
    }

    private static void LoadShader()
    {
        if (s_loaded)
            return;

        s_loaded = true;
        try
        {
            using var stream = AssetLoader.Open(new Uri(ShaderUri));
            using var reader = new StreamReader(stream);
            s_effect = SKRuntimeEffect.CreateShader(reader.ReadToEnd(), out _);
        }
        catch
        {
            s_effect = null;
        }
    }

    private static float[] GetCornerRadii(CornerRadius cornerRadius, float maxRadius)
    {
        return
        [
            (float)Math.Clamp(cornerRadius.TopLeft, 0.0, maxRadius),
            (float)Math.Clamp(cornerRadius.TopRight, 0.0, maxRadius),
            (float)Math.Clamp(cornerRadius.BottomRight, 0.0, maxRadius),
            (float)Math.Clamp(cornerRadius.BottomLeft, 0.0, maxRadius)
        ];
    }

    private static SKPath CreateRoundRectPath(SKRect rect, float[] cornerRadii)
    {
        var path = new SKPath();
        using var roundRect = new SKRoundRect();
        roundRect.SetRectRadii(rect,
        [
            new SKPoint(cornerRadii[0], cornerRadii[0]),
            new SKPoint(cornerRadii[1], cornerRadii[1]),
            new SKPoint(cornerRadii[2], cornerRadii[2]),
            new SKPoint(cornerRadii[3], cornerRadii[3])
        ]);
        path.AddRoundRect(roundRect, SKPathDirection.Clockwise);
        return path;
    }
}
