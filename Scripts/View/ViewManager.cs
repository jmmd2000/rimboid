using System.Collections.Generic;
using Godot;

/// <summary>Manages the visual nodes for runtime objects (items, frames, buildings, plants) and keeps them in sync with the model.</summary>
public partial class ViewManager : Node2D
{
    readonly Dictionary<Item, ItemView> _itemViews = new();
    readonly Dictionary<Frame, FrameView> _frameViews = new();
    readonly Dictionary<Building, BuildingView> _buildingViews = new();
    readonly Dictionary<Plant, PlantView> _plantViews = new();

    public override void _Ready() => YSortEnabled = true;

    // ---------- items ----------

    /// <summary>Creates a visual node for a loose item on the map.</summary>
    public void SpawnItemView(Item item)
    {
        var view = new ItemView
        {
            Texture = item.Def.Texture
        };
        view.Init(item, Game.TileSize);
        AddChild(view);
        _itemViews[item] = view;
    }

    /// <summary>Removes the visual node for an item that's been picked up.</summary>
    public void RemoveItemView(Item item)
    {
        if (_itemViews.TryGetValue(item, out var view))
        {
            view.QueueFree();
            _itemViews.Remove(item);
        }
    }

    /// <summary>Drops items on the map (capping and spilling as needed) and creates their views.</summary>
    public void DropItems(ItemDef def, Vector2I cell, int count)
    {
        foreach (var pile in Game.Map.DropItems(def, cell, count))
        {
            SpawnItemView(pile);
        }
    }

    // ---------- frames ----------

    /// <summary>Creates a visual node for a construction frame.</summary>
    public void SpawnFrameView(Frame frame)
    {
        var view = new FrameView();
        view.Init(frame, Game.TileSize);
        AddChild(view);
        _frameViews[frame] = view;
    }

    /// <summary>Removes the visual node for a frame that's been cancelled or built.</summary>
    public void RemoveFrameView(Frame frame)
    {
        if (_frameViews.TryGetValue(frame, out var view))
        {
            view.QueueFree();
            _frameViews.Remove(frame);
        }
    }

    // ---------- buildings ----------

    /// <summary>Creates a visual node for a finished building.</summary>
    public void SpawnBuildingView(Building building)
    {
        var view = new BuildingView();
        view.Init(building, Game.TileSize);
        AddChild(view);
        _buildingViews[building] = view;
    }

    // ------------ guys ------------

    readonly Dictionary<Guy, Node2D[]> _guyViews = new();

    /// <summary>Creates a guy's visual nodes: sprite, path line, and sleep particles.</summary>
    public void SpawnGuyViews(Guy guy)
    {
        var sprite = new GuyView { Texture = GD.Load<Texture2D>("res://Assets/guy.png") };
        sprite.Init(guy, Game.TileSize);

        var path = new PathLine();
        path.Init(guy, Game.TileSize);

        var zzz = new SleepZZZ();
        zzz.Init(guy, Game.TileSize);

        var views = new Node2D[] { sprite, path, zzz };
        foreach (var v in views) AddChild(v);
        _guyViews[guy] = views;
    }

    /// <summary>Removes a guy's visual nodes</summary>
    public void RemoveGuyViews(Guy guy)
    {
        if (_guyViews.TryGetValue(guy, out var views))
        {
            foreach (var v in views) v.QueueFree();
            _guyViews.Remove(guy);
        }
    }

    // ---------- plants ----------

    /// <summary>Creates a visual node for a plant.</summary>
    public void SpawnPlantView(Plant plant)
    {
        var view = new PlantView();
        view.Init(plant, Game.TileSize);
        AddChild(view);
        _plantViews[plant] = view;
    }

}