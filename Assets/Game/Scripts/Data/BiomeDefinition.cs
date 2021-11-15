using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Biome
{
    public string Name;
    public int TerrainLevel;
    public int TerrainOffset;
    public int TerrainAmplitude;
    public BlockSpawner MainBlockSurface;
    public BlockSpawner TerrainSurfaceBlock;
    public BlockSpawner InnerBlocks;
}

[Serializable]
public class BlockSpawner
{
    public byte MainBlock;
    public Color[] color;
}