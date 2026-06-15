/// <summary>Static global access to shared game systems.</summary>
public static class Game
{
    /// <summary>Pixel size of one map cell</summary>
    public const int TileSize = 16;

    public static GameMap Map;
    public static Pathing Pathing;
    public static MapView MapView;
    public static ViewManager Views;
}