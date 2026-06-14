using System;
using System.Net.NetworkInformation;
using Godot;

/// <summary>Draws a construction frame, a translucent ghost that solidifies as materials and work arrive.</summary>
public partial class FrameView : Node2D
{
    public Frame Frame;
    int _tileSize;
    int _lastMaterials = -1;
    float _lastWork;

    /// <summary>Positions this view at the frame's cell.</summary>
    /// <param name="frame">The frame to display.</param>
    /// <param name="tileSize">Pixel size of one tile.</param>
    public void Init(Frame frame, int tileSize)
    {
        Frame = frame;
        _tileSize = tileSize;
        Position = new Vector2(frame.Cell.X * tileSize, frame.Cell.Y * tileSize);
        ZIndex = 1;
    }

    public override void _Process(double delta)
    {
        if (Frame == null) return;

        // only repaint when progress actually changes
        if (Frame.MaterialsDelivered != _lastMaterials || !Mathf.IsEqualApprox(Frame.WorkDone, _lastWork))
        {
            _lastMaterials = Frame.MaterialsDelivered;
            _lastWork = Frame.WorkDone;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (Frame == null) return;
        var rect = new Rect2(Vector2.Zero, new Vector2(_tileSize, _tileSize));
        var colour = Frame.Def.Colour;

        // fill gets more solid as materials arrive (faint when it's just a blueprint)
        float materialFraction = Frame.Def.MaterialCost == 0 ? 1f : (float)Frame.MaterialsDelivered / Frame.Def.MaterialCost;
        DrawRect(rect, new Color(colour, Mathf.Lerp(0.2f, 0.55f, materialFraction)));

        // outline so an empty blueprint still reads clearly
        DrawRect(rect, new Color(colour, 0.9f), filled: false, width: 1f);

        // work bar rises from the bottom once materials are complete
        if (Frame.MaterialsComplete && Frame.Def.WorkToBuild > 0)
        {
            float workFraction = Mathf.Clamp(Frame.WorkDone / Frame.Def.WorkToBuild, 0f, 1f);
            float h = _tileSize * workFraction;
            DrawRect(new Rect2(0, _tileSize - h, _tileSize, h), new Color(colour, 0.85f));
        }
    }
}