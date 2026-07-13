/// <summary>A door: open while a colonist is on it or beside it, shut otherwise. The view animates the swing.</summary>
public class BuildingComponent_Door : BuildingComponent
{
    /// <summary>True when a colonist is on the door cell or an orthogonal neighbour.</summary>
    public bool Open { get; private set; }

    public override bool Ticks => true;

    public override void Tick() => Open = AColonistIsNear();

    bool AColonistIsNear()
    {
        var cell = Building.Cell;
        if (Game.Map.GuyOnCell(cell)) return true;          // on the door
        foreach (var d in Grid.Cardinal4)
            if (Game.Map.GuyOnCell(cell + d)) return true;  // orthogonal neighbour
        return false;
    }
}