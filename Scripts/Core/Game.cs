/// <summary>Static global access to shared game systems.</summary>
public static class Game
{
    /// <summary>Pixel size of one map cell</summary>
    public const int TileSize = 16;

    public static GameMap Map;
    public static Pathing Pathing;
    public static MapView MapView;
    public static ViewManager Views;

    static Guy _selectedGuy;
    /// <summary>The player-selected colonist, or null. Assigning a different value raises SelectedGuyChanged.</summary>
    public static Guy SelectedGuy
    {
        get => _selectedGuy;
        set { if (_selectedGuy == value) return; _selectedGuy = value; SelectedGuyChanged?.Invoke(value); }
    }
    /// <summary>Fires when the selected colonist changes (including to/from null).</summary>
    public static event System.Action<Guy> SelectedGuyChanged;

    public static Building SelectedBuilding;
    public static BuildingDef SelectedBuildable; // non-null = build placement mode
    public static ToolDef SelectedTool; // non-null = tool mode
    public static bool CreativeMode;
}