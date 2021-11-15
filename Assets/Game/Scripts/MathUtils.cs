using UnityEngine;

public static class MathExtensions
{
    public static int ToInt(this float f) => (f >= 0 ? (int)f : (int)f - 1);

    public static Vector3Int ToVector3Int(this Vector3 vector)
    => new Vector3Int(vector.x.ToInt(), vector.y.ToInt(), vector.z.ToInt());

    public static RectTransform RotateUIElement(this RectTransform transf, float angle)
    {
        Vector2 v = transf.anchoredPosition;
        transf.anchoredPosition = v.Rotate(angle);
        transf.localRotation *= Quaternion.Euler(Vector3.forward * angle);
        return transf;
    }

    public static Vector2 Rotate(this Vector2 position, float angle, float amplitude)
    {
        float currentRadian = angle * Mathf.Deg2Rad;

        position.x = amplitude * (float)Mathf.Cos(currentRadian);
        position.y = amplitude * (float)Mathf.Sin(currentRadian);
        return position;
    }

    public static Vector2 Rotate(this Vector2 position, float angle)
    {
        float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
        float cos = Mathf.Cos(angle * Mathf.Deg2Rad);

        float tx = position.x;
        float ty = position.y;
        position.x = (cos * tx) - (sin * ty);
        position.y = (sin * tx) + (cos * ty);
        
        return position;
    }

    /*
                             vec.x = index / (sizes.x * sizes.y);
                             vec.y = (index / sizes.x) % sizes.y;
                             vec.z = index % (sizes.x * sizes.y) % sizes.x;

                             result = 
                         */


   
}

public static class MapUtils
{
    public static int SectorScale(this int scalar)
         => scalar / GameData.SectorSize;

    public static int BlockScale(this int scalar)
        => scalar * GameData.SectorSize;

   public static SectorCoord FromAbsolute(this SectorCoord coord,Vector3Int absoluteCoord)
   {
        coord.sector = new Vector2Int(absoluteCoord.x.SectorScale(),absoluteCoord.z.SectorScale());
        coord.block = absoluteCoord;
        coord.block.x = coord.block.x - coord.sector.x.BlockScale();
        coord.block.z = coord.block.z - coord.sector.y.BlockScale();

        if(coord.block.x < 0)
        {
            coord.block.x += GameData.SectorSize;
            coord.sector.x--;
        }
        
        if(coord.block.x > GameData.SectorSize)
        {
            coord.block.x -= GameData.SectorSize;
            coord.sector.x++;
        }

        if (coord.block.z < 0)
        {
            coord.block.z += GameData.SectorSize;
            coord.sector.y--;
        }
        
        if (coord.block.z > GameData.SectorSize)
        {
            coord.block.z -= GameData.SectorSize;
            coord.sector.y++;
        }

        return coord;
   }

    public static int FlatIndex(this Vector3Int v)
   => (v.x * GameData.SectorHeight * GameData.SectorSize) + (v.y * GameData.SectorSize) + v.z;

    public static Vector3Int ExpandIndex(this int index)
    {
        Vector3Int vec = new Vector3Int();
        vec.x = index / (GameData.SectorSize * GameData.SectorHeight);
        vec.y = (index / GameData.SectorSize) % GameData.SectorHeight;
        vec.z = index % GameData.SectorSize;
        return vec;

    }
}
   
public struct SectorCoord
{
    public Vector2Int sector;
    public Vector3Int block;

    public override string ToString()
    {
        return sector.ToString() + "::" + block.ToString();
    }
}

