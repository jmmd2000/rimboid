using Godot;

public partial class CameraController : Camera2D
{
    [Export] public float ZoomSpeed = 0.1f;
    [Export] public float MinZoom = 0.5f;
    [Export] public float MaxZoom = 4f;

    bool _dragging;
    Vector2 _dragStart;

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Middle)
                _dragging = mb.Pressed;
            else if (mb.Pressed && mb.ButtonIndex == MouseButton.WheelUp)
                Zoom *= 1 + ZoomSpeed;
            else if (mb.Pressed && mb.ButtonIndex == MouseButton.WheelDown)
                Zoom *= 1 - ZoomSpeed;

            Zoom = Zoom.Clamp(new Vector2(MinZoom, MinZoom), new Vector2(MaxZoom, MaxZoom));
        }

        if (e is InputEventMouseMotion mm && _dragging)
            Position -= mm.Relative / Zoom;
    }
}