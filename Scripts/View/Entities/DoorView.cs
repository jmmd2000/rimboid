using Godot;

/// <summary>Draws a door as a leaf that swings on its hinge, lerping toward the model's open state.</summary>
public partial class DoorView : BuildingView
{
    const float OpenSpeed = 6f; // swing fraction per second
    BuildingComponent_Door _door;
    float _visualOpen; // 0 = shut, 1 = fully open

    public override void _Process(double delta)
    {
        base._Process(delta); // keeps the selection-highlight logic
        _door ??= Building.GetComponent<BuildingComponent_Door>();
        float target = _door != null && _door.Open ? 1f : 0f;
        float next = Mathf.MoveToward(_visualOpen, target, (float)delta * OpenSpeed);
        if (!Mathf.IsEqualApprox(next, _visualOpen)) { _visualOpen = next; QueueRedraw(); }
    }

    public override void _Draw()
    {
        DrawLeaf(this, Vector2.Zero, _tileSize, Building.Rotation, _visualOpen, Building.Def.Colour);
        if (_selected) DrawRect(new Rect2(0f, 0f, _tileSize, _tileSize), Colors.White, filled: false, width: 1f);
    }

    /// <summary>Draws a door leaf hinged on a cell edge, spanning the doorway, swung open by `open` (0-1).
    /// Shared by the live door and the placement ghost. cellOrigin is the pixel top-left of the door's cell.</summary>
    public static void DrawLeaf(CanvasItem canvas, Vector2 cellOrigin, int t, int rotation, float open, Color colour)
    {
        float baseAngle = rotation * (Mathf.Pi / 2f);
        var centre = cellOrigin + new Vector2(t / 2f, t / 2f);
        var hinge = centre + (cellOrigin + new Vector2(0f, t / 2f) - centre).Rotated(baseAngle);
        float thick = t * 0.2f;
        canvas.DrawSetTransform(hinge, baseAngle - open * (Mathf.Pi / 2f), Vector2.One);
        canvas.DrawRect(new Rect2(0f, -thick / 2f, t, thick), colour);
        canvas.DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
    }
}