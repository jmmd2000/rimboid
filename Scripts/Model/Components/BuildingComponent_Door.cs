/// <summary>A door: open while a colonist is on it or beside it, shut otherwise. The view animates the swing.</summary>
public class BuildingComponent_Door : BuildingComponent
{
    /// <summary>True when a colonist is on the door cell or an orthogonal neighbour.</summary>
    public bool Open { get; private set; }

    public override bool Ticks => true;

    public override void Tick() => Open = AColonistIsNear();

    bool AColonistIsNear()
    {
        foreach (var guy in Game.Map.Guys)
            if (Grid.DistanceSquared(guy.Cell, Building.Cell) <= 1) return true; // on it (0) or orthogonal (1)
        return false;
    }
}