using Godot;

/// <summary>The glow for a light-emitting building: a PointLight2D whose energy ramps up with the
/// night (fading out by day, never blowing out the daylight) and switches off entirely in full
/// daylight to spend nothing. Builds a smooth radial falloff itself if the def doesn't supply one.</summary>
public partial class LightView : PointLight2D
{
    static Texture2D _defaultTexture;
    BuildingComponent_Light _light;

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
    }

    public override void _Process(double delta)
    {
        float night = DayNight.NightFactor(GameTime.TimeOfDay);
        Enabled = night > 0f;           // off in full daylight, no light/shadow work
        Energy = _light.Energy * night; // ramps in at dusk
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