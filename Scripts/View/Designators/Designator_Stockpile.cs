using Godot;

/// <summary>Adds or removes cells of a stockpile zone.</summary>
public class Designator_Stockpile : Designator
{
    readonly Stockpile _stockpile;

    public Designator_Stockpile(Stockpile stockpile) => _stockpile = stockpile;

    public override void Apply(Vector2I cell)
    {
        if (Game.Map.Terrain[cell.X, cell.Y].Walkable && _stockpile.Cells.Add(cell))
            Game.MapView.MarkStockpile(cell);
    }

    public override void Cancel(Vector2I cell)
    {
        if (_stockpile.Cells.Remove(cell))
            Game.MapView.ClearStockpile(cell);
    }
}