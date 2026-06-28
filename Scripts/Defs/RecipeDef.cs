using Godot;

/// <summary>Definition resource for a workbench recipe.</summary>
[GlobalClass]
public partial class RecipeDef : Def
{
    [Export] public Godot.Collections.Array<IngredientCount> Ingredients { get; set; } = new();
    [Export] public ItemDef Output { get; set; }
    [Export] public int OutputCount { get; set; } = 1;
    [Export] public float WorkAmount { get; set; } = 120f;
}