public class GameMap
{
    public int Width;
    public int Height;
    public TerrainDef[,] Terrain;

    public GameMap(int width, int height)
    {
        Width = width;
        Height = height;
        Terrain = new TerrainDef[width, height];
    }
}