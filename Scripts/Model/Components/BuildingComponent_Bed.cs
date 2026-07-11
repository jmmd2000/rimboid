/// <summary>Runtime bed: its rest-refill multiplier, read off its component properties.</summary>
public class BuildingComponent_Bed : BuildingComponent
{
    /// <summary>How much faster this bed refills rest versus sleeping on the floor.</summary>
    public float RestFactor => ((ComponentProperties_Bed)Props).RestFactor;
}