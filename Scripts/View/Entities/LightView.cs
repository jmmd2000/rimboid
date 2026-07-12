using Godot;

/// <summary>The glow for a light-emitting building: a PointLight2D whose energy ramps up with the
/// night (fading out by day, never blowing out the daylight) and switches off entirely in full
/// daylight to spend nothing. Builds a smooth radial falloff itself if the def doesn't supply one.</summary>
public partial class LightView : PointLight2D
{
    static Texture2D _defaultTexture;
    BuildingComponent_Light _light;
    float _time;
    float _phase; // per-light offset so lights don't flicker in sync

    /// <summary>Configures the light from its component and centres it on the building's footprint
    /// (local to the building view, which sits at the origin cell).</summary>
    public void Init(BuildingComponent_Light light, int tileSize)
    {
        _light = light;
        Texture = light.Texture ?? DefaultTexture();
        Color = light.Colour;
        TextureScale = light.Scale;
        ShadowEnabled = light.CastsShadow;
        Position = FootprintCentreLocal(light.Building, tileSize);
        _phase = light.Building.Cell.X * 12.9f + light.Building.Cell.Y * 78.2f;
    }

    public override void _Process(double delta)
    {
        _time += (float)delta;
        float night = DayNight.NightFactor(GameTime.TimeOfDay);
        Enabled = night > 0f;                             // off in full daylight, no light/shadow work
        Energy = _light.Energy * night * FlickerFactor(); // night ramp + optional flicker
    }

    /// <summary>A gentle light wobble (1 = steady), layered sines offset per light.</summary>
    float FlickerFactor()
    {
        if (!_light.Flicker) return 1f;
        float s = _light.FlickerSpeed;
        float n = Mathf.Sin(_time * s + _phase) * 0.6f + Mathf.Sin(_time * s * 2.7f + _phase * 1.3f) * 0.4f;
        return 1f + _light.FlickerAmount * n;
    }


    static Vector2 FootprintCentreLocal(Building b, int tileSize)
    {
        var sum = Vector2.Zero;
        int n = 0;
        foreach (var c in b.OccupiedCells) { sum += new Vector2(c.X - b.Cell.X + 0.5f, c.Y - b.Cell.Y + 0.5f); n++; }
        return sum / n * tileSize;
    }

    /// <summary>A smooth white radial falloff, built once and shared. Per-pixel, so no banding.</summary>
    static Texture2D DefaultTexture()
    {
        if (_defaultTexture != null) return _defaultTexture;

        const int size = 128;
        float centre = size / 2f;
        var img = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = new Vector2(x - centre, y - centre).Length() / centre; // 0 centre .. 1 edge
                float a = 1f - (float)Mathf.SmoothStep(0f, 1f, d);               // smooth, 0 past the edge
                img.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        _defaultTexture = ImageTexture.CreateFromImage(img);
        return _defaultTexture;
    }
}