using System.Collections.Generic;
using UnityEngine;

/*
 * Procedural Sectors are the "Chunks" of "Minecraft", but I prefer change the name because the ECS system,
 * and because the word "Chunk" sounds weird to me.
 * 
 * The objectives of this class in particular are:
 * 
 * 1 - Build a mesh from a map, where only the visible sides of the blocks mapped to it are rendered;
 */

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class ProceduralSector : MonoBehaviour
{
    //First we will label the sector to know where the sector is
    private Vector2Int sectorPosition;

    //Then we set all the relevant information
    private Material material;
    public int maxAltitude;
    public int minAltitude;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private List<Vector3> vertices;
    private List<Vector2> uvs;
    private List<int> triangles;
    
    private int verticesIndex;
    private byte currentBiome;
    private byte[,,] areaMap;
    private byte[,,] temperatureMap;
    private List<BlockBuilder> blocksToRender;
    private bool IsAlreadyRendered = false;


    //So we will delay the initialization from the moment we need it.
    public void InitializeSector(Vector2Int pos, byte _biome)
    {
        sectorPosition = pos;
        transform.position = new Vector3(pos.x, 0, pos.y) * (GameData.SectorSize - 1);
        name = "Sector" + pos.ToString();
        gameObject.layer = LayerMask.NameToLayer("Ground");
        BuildAreaMap(_biome);

    }

    //We use Optimize to make the rendering AFTER the ProceduralWorld build all sectors.
    public void OptimizeMesh()
    {
        if (IsAlreadyRendered == false)
        {
            FilterMesh();
            CalculateMesh();
            BuildMesh();
            IsAlreadyRendered = true;
        }
    }

    public void ResetRender() => IsAlreadyRendered = false;

    //First, we build a "Area Map" to get the heights of sector
    public void BuildAreaMap(byte currentBiome)
    {

        areaMap = new byte[GameData.SectorSize, GameData.SectorHeight, GameData.SectorSize];
        maxAltitude = MainGameManager.GM.Biomes[currentBiome].TerrainLevel;
        minAltitude = GameData.SectorHeight;

        FastNoiseSIMD f_noise = new FastNoiseSIMD(MainGameManager.GM.Seed);
        float[] f = f_noise.GetSampledNoiseSet
            (sectorPosition.x.BlockScale(), 0, sectorPosition.y.BlockScale(), GameData.SectorSize, 1, GameData.SectorSize, 1);

        for (int x = 0; x < GameData.SectorSize; x++)
            for (int z = 0; z < GameData.SectorSize; z++)
            {
                int height = MainGameManager.GM.Biomes[currentBiome].TerrainLevel
                    + (MainGameManager.GM.Biomes[currentBiome].TerrainAmplitude *
                    (f[(x * GameData.SectorSize) + z] + 1f) / 2f).ToInt();
                if (height > maxAltitude) maxAltitude = height;
                if (height < minAltitude) minAltitude = height;

                for (int y = 0; y < height + 1; y++)
                {
                    Vector3Int vec = new Vector3Int(x, y, z);

                    if (y == 0) areaMap[x, y, z] = 1; //Bedrock
                    else if (y > height) areaMap[x, y, z] = 0; //Air
                    else if (y == height) areaMap[x, y, z] = MainGameManager.GM.Biomes[currentBiome].MainBlockSurface.MainBlock;
                    else if (y >= height - MainGameManager.GM.Biomes[currentBiome].TerrainOffset)
                        areaMap[x, y, z] = MainGameManager.GM.Biomes[currentBiome].TerrainSurfaceBlock.MainBlock;
                    else areaMap[x, y, z] = MainGameManager.GM.Biomes[currentBiome].InnerBlocks.MainBlock;

                }
            }
    }




    public byte GetBlockInAreaMap(Vector3Int pos) => (IsInRange(pos)) ? areaMap[pos.x, pos.y, pos.z] : (byte)0;

    public void SetBlockInMapArea(Vector3Int pos, byte block)
    {
        if (IsInRange(pos)) areaMap[pos.x, pos.y, pos.z] = block;
        ResetRender();
        OptimizeMesh();
    }

    private bool IsInRange(Vector3Int pos)
    => pos.x >= 0 && pos.x < GameData.SectorSize && pos.y >= 0
    && pos.y < GameData.SectorHeight && pos.z >= 0 && pos.z < GameData.SectorSize;

    //Let´s so reduce the mesh render to the blocks that really have at least one face to render
    public void FilterMesh()
    {
        blocksToRender = new List<BlockBuilder>();
        int minHeightToRender = (MainGameManager.GM.World.PlayerPosition.y - GameData.SectorSize < 0)
            ? 0 : MainGameManager.GM.World.PlayerPosition.y - GameData.SectorSize;
        for (int x = 0; x < GameData.SectorSize; x++)
            for (int z = 0; z < GameData.SectorSize; z++)
                for (int y = minHeightToRender; y < maxAltitude + 1; y++)
                {
                    //Debug.Log(blocksToRender.Count);
                    if (blocksToRender.Count > GameData.SectorSize * GameData.SectorSize * maxAltitude + 1)
                    {
                        Application.Quit();
                    }
                    if (areaMap[x, y, z] != 0)
                    {
                        Vector3Int vector = new Vector3Int(x, y, z);
                        BlockBuilder block = new BlockBuilder();
                        block.xyz = vector;

                        //Finally we verify if the blocks around are solid. For awhile, we deduce that the surrounding sectors
                        //are only air;
                        block.xprev = (x == 0) ? Verify(new Vector3Int(x - 1, y, z)) : areaMap[x - 1, y, z] == 0;
                        block.yprev = (y == 0) ? true : areaMap[x, y - 1, z] == 0;
                        block.zprev = (z == 0) ? Verify(new Vector3Int(x, y, z - 1)) : areaMap[x, y, z - 1] == 0;
                        block.xnext = (x == GameData.SectorSize - 1) 
                            ? Verify(new Vector3Int(x + 1, y, z)) : areaMap[x + 1, y, z] == 0;
                        block.ynext = areaMap[x, y + 1, z] == 0;
                        block.znext = (z == GameData.SectorSize - 1) ? Verify(new Vector3Int(x, y, z + 1)) : areaMap[x,y,z + 1] == 0;

                        if (block.xprev || block.xnext || block.yprev || block.ynext || block.zprev || block.znext)
                        {
                            blocksToRender.Add(block);
                        }
                    }
                }

    }

    private bool Verify(Vector3Int vector)
    {

        if (vector.x == -1)
        {
            Vector2Int v = new Vector2Int(sectorPosition.x - 1, sectorPosition.y);
            if (!MainGameManager.GM.World.sectors.ContainsKey(v)) return true;
            else return MainGameManager.GM.World.sectors[v].
                    GetBlockInAreaMap(new Vector3Int(GameData.SectorSize - 1, vector.y, vector.z)) == 0;
        }
        
        if (vector.x == GameData.SectorSize)
        {
            Vector2Int v = new Vector2Int(sectorPosition.x + 1, sectorPosition.y);
            if (!MainGameManager.GM.World.sectors.ContainsKey(v)) return true;
            else return MainGameManager.GM.World.sectors[v].GetBlockInAreaMap(new Vector3Int(0, vector.y, vector.z)) == 0;
        }
        
        if (vector.z == -1)
        {
            Vector2Int v = new Vector2Int(sectorPosition.x, sectorPosition.y - 1);
            if (!MainGameManager.GM.World.sectors.ContainsKey(v)) return true;
            else return MainGameManager.GM.World.sectors[v].
                    GetBlockInAreaMap(new Vector3Int(vector.x, vector.y, GameData.SectorSize - 1)) == 0;
        }
        
        if (vector.z == GameData.SectorSize)
        {
            Vector2Int v = new Vector2Int(sectorPosition.x, sectorPosition.y + 1);
            if (!MainGameManager.GM.World.sectors.ContainsKey(v)) return true;
            else return MainGameManager.GM.World.sectors[v].GetBlockInAreaMap(new Vector3Int(vector.x, vector.y, 0)) == 0;
        }

        return true;
    } 

    //Next Step, let´s build the mesh
    public void CalculateMesh()
    {
        if (blocksToRender == null) return;
        if (blocksToRender.Count == 0) return;

        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        triangles = new List<int>();

        foreach(BlockBuilder block in blocksToRender)
        {
            Block type = MainGameManager.BlockType[areaMap[block.xyz.x, block.xyz.y, block.xyz.z]];
            Vector3 offset = new Vector3(sectorPosition.x, 0, sectorPosition.y);
           
            if (block.xprev)
            {
                vertices.AddRange(BlockData.LeftFace(offset + block.xyz));
                uvs.AddRange(BlockData.Uvs(type.LeftFace));
                triangles.AddRange(BlockData.TriangleIndices(verticesIndex));
                verticesIndex += 4;
            }

            if (block.xnext)
            {
                vertices.AddRange(BlockData.RightFace(offset + block.xyz));
                uvs.AddRange(BlockData.Uvs(type.RightFace));
                triangles.AddRange(BlockData.TriangleIndices(verticesIndex));
                verticesIndex += 4;
            
            }

            if (block.yprev)
            {
                vertices.AddRange(BlockData.BottomFace(offset + block.xyz));
                uvs.AddRange(BlockData.Uvs(type.BottomFace));
                triangles.AddRange(BlockData.TriangleIndices(verticesIndex));
                verticesIndex += 4;
            }

            if (block.ynext)
            {
                vertices.AddRange(BlockData.TopFace(offset + block.xyz));
                uvs.AddRange(BlockData.Uvs(type.TopFace));
                triangles.AddRange(BlockData.TriangleIndices(verticesIndex));
                verticesIndex += 4;
            }

            if (block.zprev)
            {
                vertices.AddRange(BlockData.BackFace(offset + block.xyz));
                uvs.AddRange(BlockData.Uvs(type.BackFace));
                triangles.AddRange(BlockData.TriangleIndices(verticesIndex));
                verticesIndex += 4;
       
            }

            if (block.znext)
            {
                vertices.AddRange(BlockData.FrontFace(offset + block.xyz));
                uvs.AddRange(BlockData.Uvs(type.FrontFace));
                triangles.AddRange(BlockData.TriangleIndices(verticesIndex));
                verticesIndex += 4;
            }

        }

        blocksToRender.Clear();
    }




    //We will get the components only when we build the mesh
    public void BuildMesh()
    {
        if (vertices == null) return;
        if (uvs == null) return;
        if (triangles == null) return;

        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        Mesh mesh = new Mesh();
        mesh.name = name;
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshRenderer.material = MainGameManager.GM.OpaqueMaterial;
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
        verticesIndex = 0;
    }
}

public struct BlockBuilder
{
   public Vector3Int xyz;

    public bool xprev;
    public bool xnext;
    public bool yprev;
    public bool ynext;
    public bool zprev;
    public bool znext;
}