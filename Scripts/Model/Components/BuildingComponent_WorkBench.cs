using System.Collections.Generic;
using Godot;

/// <summary>Runtime workbench: the bills queued on this building. Its recipes come from its properties.</summary>
public class BuildingComponent_WorkBench : BuildingComponent
{
    public List<Bill> Bills { get; } = new();

    /// <summary>The recipes this bench can run, read off its component properties.</summary>
    public Godot.Collections.Array<RecipeDef> Recipes => ((ComponentProperties_WorkBench)Props).Recipes;
}