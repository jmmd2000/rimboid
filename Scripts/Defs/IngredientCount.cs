using Godot;

/// <summary>One input line of a recipe: an item type and how many are needed.</summary>
[GlobalClass]
public partial class IngredientCount : Resource
{
    [Export] public ItemDef Item { get; set; }
    [Export] public int Count { get; set; } = 1;
}