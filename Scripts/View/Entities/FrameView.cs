using System;
using Godot;

/// <summary>Draws a construction frame, a translucent ghost that solidifies as materials and work arrive.</summary>
public partial class FrameView : Node2D
{
    public Frame Frame;
    int _tileSize;
    int _lastMaterials = -1;
    float _lastWork = -1f;
    Label _label;
    Vector2I _footprintMin;  // rotated footprint, cached at Init since Def/Rotation never change
    Vector2I _footprintSize;

    /// <summary>Positions this view at the frame's cell and adds the material count label.</summary>
    /// <param name="frame">The frame to display.</param>
    /// <param name="tileSize">Pixel size of one tile.</param>
    public void Init(Frame frame, int tileSize)
    {
        Frame = frame;
        _tileSize = tileSize;
        Position = new Vector2(frame.Cell.X * tileSize, frame.Cell.Y * tileSize);
        ZIndex = 1;

        _footprintMin = Footprint.MinOffset(frame.Def.Size, frame.Rotation);
        _footprintSize = Footprint.Rotated(frame.Def.Size, frame.Rotation);
        _label = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Position = new Vector2(_footprintMin.X * tileSize, _footprintMin.Y * tileSize),
            Size = new Vector2(_footprintSize.X * tileSize, _footprintSize.Y * tileSize)
        };
        _label.AddThemeColorOverride("font_color", Colors.White);
        _label.AddThemeFontSizeOverride("font_size", 8);
        AddChild(_label);
    }

    public override void _Process(double delta)
    {
        if (Frame == null) return;

        // only repaint and relabel when progress actually changes
        if (Frame.MaterialsDelivered != _lastMaterials || !Mathf.IsEqualApprox(Frame.WorkDone, _lastWork))
        {
            _lastMaterials = Frame.MaterialsDelivered;
            _lastWork = Frame.WorkDone;
            _label.Text = Frame.MaterialsComplete ? "" : $"{Frame.MaterialsDelivered}/{Frame.Def.MaterialCost}";
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (Frame == null) return;
        var rect = new Rect2(_footprintMin.X * _tileSize, _footprintMin.Y * _tileSize, _footprintSize.X * _tileSize, _footprintSize.Y * _tileSize);
        var colour = Frame.Def.Colour;

        if (!Frame.MaterialsComplete)
        {
            // faint ghost that darkens slightly as material arrives
            float fraction = Frame.Def.MaterialCost == 0 ? 1f
                : (float)Frame.MaterialsDelivered / Frame.Def.MaterialCost;
            DrawRect(rect, new Color(colour, Mathf.Lerp(0.15f, 0.4f, fraction)));
            DrawRect(rect, new Color(colour, 0.8f), filled: false, width: 1f);
            return;
        }

        // ready = solid fill + bright outline
        DrawRect(rect, new Color(colour, 0.7f));
        DrawRect(rect, Colors.White, filled: false, width: 2f);

        // work bar rises from the bottom as it's built
        if (Frame.Def.WorkToBuild > 0)
        {
            float workFraction = Mathf.Clamp(Frame.WorkDone / Frame.Def.WorkToBuild, 0f, 1f);
            float h = rect.Size.Y * workFraction;
            DrawRect(new Rect2(rect.Position.X, rect.Position.Y + rect.Size.Y - h, rect.Size.X, h), new Color(colour, 0.95f));
        }
    }
}