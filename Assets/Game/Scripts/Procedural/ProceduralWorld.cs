using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralWorld : MonoBehaviour
{
    public Dictionary<Vector2Int, ProceduralSector> sectors = new Dictionary<Vector2Int, ProceduralSector>();
    public Dictionary<Vector2Int, int> biomeTable = new Dictionary<Vector2Int, int>();
    List<Vector2Int> activeSectors = new List<Vector2Int>();
    public Vector3Int PlayerPosition = Vector3Int.zero;

    public void InitializeWorld()
    {
        UpdateWorld();
        StartCoroutine(Operate());
    }

    public byte GetBlockInWorld(SectorCoord coord)
    {
        if (!sectors.ContainsKey(coord.sector)) return 0;
        return sectors[coord.sector].GetBlockInAreaMap(coord.block);
    }

    public void SetBlockInWorld(SectorCoord coord, byte block)
    {
        if (!sectors.ContainsKey(coord.sector)) return;

        sectors[coord.sector].SetBlockInMapArea(coord.block, block);
    }
   
    public void UpdateWorld()
    {
        List<Vector2Int> previousActive = new List<Vector2Int>(activeSectors.ToArray());
        activeSectors.Clear();
        Vector2Int r_pos = new Vector2Int(PlayerPosition.x.SectorScale(),PlayerPosition.z.SectorScale());

        for (int x = r_pos.x - GameData.ViewDistance; x <= r_pos.x + GameData.ViewDistance; x++)
            for (int y = r_pos.y - GameData.ViewDistance; y <= r_pos.y + GameData.ViewDistance; y++)
            {
                Vector2Int vector = new Vector2Int(x,y);
                
                if (!sectors.ContainsKey(vector))
                {
                     ProceduralSector newSector = Instantiate(MainGameManager.GM.sectorBase);
                     newSector.InitializeSector(vector,0);
                     sectors.Add(vector, newSector);
                }
                else
                {
                     sectors[vector].gameObject.SetActive(true);
                }
                    
                activeSectors.Add(vector);
                if (previousActive.Contains(vector)) previousActive.Remove(vector);
            }

        if (activeSectors.Count > 0)
        {
            foreach (Vector2Int sec in activeSectors)
            {
                sectors[sec].OptimizeMesh();
            }
        }
        if (previousActive.Count > 0)
        {
            foreach (Vector2Int v in previousActive) sectors[v].gameObject.SetActive(false);
        }
    }

    IEnumerator Operate()
    {
        while(true)
        {
            yield return null;
        }
    }
}

