using Godot;

/// <summary>Marks a building as a light source: a glow that ramps in as night falls.</summary>
[GlobalClass]
public partial class ComponentProperties_Light : ComponentProperties
{
    [Export] public Texture2D Texture { get; set; }  // the soft radial falloff
    [Export] public Color Colour { get; set; } = new(1f, 0.85f, 0.6f); // warm
    [Export] public float Energy { get; set; } = 1f; // brightness at deepest night
    [Export] public float Scale { get; set; } = 1f; // light size (PointLight2D texture scale)
    [Export] public bool CastsShadow { get; set; } // opt-in; keep few (perf)

    public override BuildingComponent MakeComponent() => new BuildingComponent_Light();
}