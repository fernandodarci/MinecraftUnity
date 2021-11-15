using UnityEngine;

public static class GameData
{
    public static int SectorSize => 16;
    public static int SectorHeight => 256;
    public static int ViewDistance => 8;
    public static int TextureAtlasSide => 4;

    public static Vector3Int AreaDimensions => new Vector3Int(ViewDistance, 1, ViewDistance);
    public static Vector3Int SectorDimensions => new Vector3Int(SectorSize, SectorHeight, SectorSize);
    public static Vector3Int AbsoluteDimensions => AreaDimensions * SectorDimensions;
    public static float BlockTextureSize => 1f / TextureAtlasSide;

    public static float minLightLevel => 0.1f;
    public static float maxLightLevel => 0.9f;
    public static float unitOfLight => 0.0625f;
}

   
public static class BlockData
{
    public static Vector3[] BlockVertices(Vector3 pos) => new Vector3[]
    {
        new Vector3(0.0f, 0.0f, 0.0f) + pos,
        new Vector3(1.0f, 0.0f, 0.0f) + pos,
        new Vector3(1.0f, 1.0f, 0.0f) + pos,
        new Vector3(0.0f, 1.0f, 0.0f) + pos,
        new Vector3(0.0f, 0.0f, 1.0f) + pos,
        new Vector3(1.0f, 0.0f, 1.0f) + pos,
        new Vector3(1.0f, 1.0f, 1.0f) + pos,
        new Vector3(0.0f, 1.0f, 1.0f) + pos,
    };

    public static Vector3[] BlockSides => new Vector3[]
    {
        Vector3Int.back, Vector3Int.forward, Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
    };

    public static Vector3[] BackFace(Vector3 pos)
        => new Vector3[] { BlockVertices(pos)[0], BlockVertices(pos)[3], BlockVertices(pos)[1], BlockVertices(pos)[2] };
    public static Vector3[] FrontFace(Vector3 pos)
        => new Vector3[] { BlockVertices(pos)[5], BlockVertices(pos)[6], BlockVertices(pos)[4], BlockVertices(pos)[7] };
    public static Vector3[] TopFace(Vector3 pos)
        => new Vector3[] { BlockVertices(pos)[3], BlockVertices(pos)[7], BlockVertices(pos)[2], BlockVertices(pos)[6] };
    public static Vector3[] BottomFace(Vector3 pos)
        => new Vector3[] { BlockVertices(pos)[1], BlockVertices(pos)[5], BlockVertices(pos)[0], BlockVertices(pos)[4] };
    public static Vector3[] LeftFace(Vector3 pos)
        => new Vector3[] { BlockVertices(pos)[4], BlockVertices(pos)[7], BlockVertices(pos)[0], BlockVertices(pos)[3] };
    public static Vector3[] RightFace(Vector3 pos)
        => new Vector3[] { BlockVertices(pos)[1], BlockVertices(pos)[2], BlockVertices(pos)[5], BlockVertices(pos)[6] };

    public static Vector3[] GetFace(Vector3 position, int i)
    {
        if (i < 0 || i > 5) return null;

        switch(i)
        {
            case 0: return BackFace(position);
            case 1: return FrontFace(position);
            case 2: return TopFace(position);
            case 3: return BottomFace(position);
            case 4: return LeftFace(position);
            case 5: return RightFace(position);
            default: return null;
        }
    }

    public static int[] TriangleIndices(int index)
        => new int[] { index, index + 1, index + 2, index + 2, index + 1, index + 3 };

     public static Vector2[] Uvs(int index) 
     {
         float x = index % GameData.TextureAtlasSide;
         float y = index / GameData.TextureAtlasSide;

         x *= GameData.BlockTextureSize;
         y *= GameData.BlockTextureSize;
         
        return new Vector2[]
        {
             new Vector2(x,y),
             new Vector2(x,y + GameData.BlockTextureSize),
             new Vector2(x + GameData.BlockTextureSize, y),
             new Vector2(x + GameData.BlockTextureSize, y + GameData.BlockTextureSize),
        };
     }

}
