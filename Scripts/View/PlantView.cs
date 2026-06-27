using Godot;

/// <summary>Sprite for a plant, anchored at the base of its cell and scaled to the plant's
/// rolled draw size, so tall trees rise upward and overhang neighbouring cells visually.</summary>
public partial class PlantView : Sprite2D
{

    Plant _plant;
    int _tileSize;
    int _lastStage;

    /// <summary>Positions and scales this view for a plant.</summary>
    /// <param name="plant">The plant to display.</param>
    /// <param name="tileSize">Pixel size of one tile.</param>
    public void Init(Plant plant, int tileSize)
    {
        _plant = plant;
        _tileSize = tileSize;

        // node sits on the bottom-centre of the footprint cell — this is its y-sort point
        Position = new Vector2(plant.Cell.X * tileSize + tileSize / 2f,
                               plant.Cell.Y * tileSize + tileSize);
        Centered = false;

        _lastStage = plant.StageIndex;
        ApplyTexture();

        // only multi-stage plants change appearance, so only they need to poll
        SetProcess(plant.Def.GrowthStages.Length > 1);
    }

    public override void _Process(double delta)
    {
        if (_plant.StageIndex == _lastStage) return;
        _lastStage = _plant.StageIndex;
        ApplyTexture();
    }

    /// <summary>Picks the correct texture and rescales to the correct draw size.</summary>
    void ApplyTexture()
    {
        var tex = _plant.CurrentTexture;
        Texture = tex;
        if (tex == null) return;

        Offset = new Vector2(-tex.GetWidth() / 2f, -tex.GetHeight());
        Scale = new Vector2(_plant.StageDrawWidth * _tileSize / tex.GetWidth(),
                            _plant.StageDrawHeight * _tileSize / tex.GetHeight());
    }

    /// <summary>Plays a fall animation (pivoting at the base), then frees the node.</summary>
    public void Topple()
    {
        SetProcess(false);
        if (Texture == null) { QueueFree(); return; }

        float cutPx = _tileSize;
        float cropTexture = cutPx / Scale.Y;

        // crop off the bottom of the tree texture for the stump
        RegionEnabled = true;
        RegionRect = new Rect2(0, 0, Texture.GetWidth(), Texture.GetHeight() - cropTexture);
        Offset = new Vector2(-Texture.GetWidth() / 2f, -(Texture.GetHeight() - cropTexture));

        // raise the rotation pivot to the cut line
        Position -= new Vector2(0, cutPx);

        // rotate flat, slide in the fall direction, drop to ground level
        int direction = GD.Randf() < 0.5f ? 1 : -1;
        var landing = Position + new Vector2(direction * cutPx, cutPx);

        var tween = CreateTween().SetParallel(true);

        tween.TweenProperty(this, "rotation", direction * Mathf.Pi / 2f, 1.2f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);

        tween.TweenProperty(this, "position", landing, 1.2f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);

        tween.Chain().TweenCallback(Callable.From(QueueFree));
    }
}