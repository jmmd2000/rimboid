/// <summary>Static global access to shared game systems.</summary>
public static class Game
{
    /// <summary>Pixel size of one map cell</summary>
    public const int TileSize = 16;

    public static GameMap Map;
    public static Pathing Pathing;
    public static MapView MapView;
    public static ViewManager Views;
    public static Guy SelectedGuy;
    public static Building SelectedBuilding;
    public static BuildingDef SelectedBuildable; // non-null = build placement mode
    public static ToolDef SelectedTool; // non-null = tool mode
    public static bool CreativeMode;
}