using Godot;

/// <summary>Runtime light source, reading its colour/energy/scale/shadow off its properties.</summary>
public class BuildingComponent_Light : BuildingComponent
{
    public Texture2D Texture => ((ComponentProperties_Light)Props).Texture;
    public Color Colour => ((ComponentProperties_Light)Props).Colour;
    public float Energy => ((ComponentProperties_Light)Props).Energy;
    public float Scale => ((ComponentProperties_Light)Props).Scale;
    public bool CastsShadow => ((ComponentProperties_Light)Props).CastsShadow;
    public bool Flicker => ((ComponentProperties_Light)Props).Flicker;
    public float FlickerAmount => ((ComponentProperties_Light)Props).FlickerAmount;
    public float FlickerSpeed => ((ComponentProperties_Light)Props).FlickerSpeed;
}