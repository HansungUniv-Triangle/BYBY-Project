using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class WorldManager : Singleton<WorldManager>
{
    [Header("Noise Setting")]
    [Space(5f)]
    public int MapWidth;
    public int MapHeight;

    public int Seed;
    public float NoiseScale;
    public Vector2 Offset;
    
    public int Octaves;
    [Range(0f, 1f)]
    public float Persistance;
    public float Lacunarity;

    /*
    [Header("3D Noise Setting")]
    [Space(5f)]
    public float Scale;
    public float CaveThreshold;
    */

    [Space(5f)]
    public bool AutoUpdate;

    [Header("Terrain Setting")]
    [Space(5f)]
    public int TerrainHeight = 10;
    public int TerrainBaseHeight = 5;

    [Header("Chunk Setting")]
    [Space(5f)]
    public Vector3Int ChunkSize;

    [Header("World Setting")]
    [Space(5f)]
    public int WorldChunkWidth;
    public int WorldChunkHeight;

    [Space(5f)]
    public Material Material;
    public GameObject BlockPrefab;
    public GameObject ChunkPrefab;

    [Space(5f)]
    public Block[] Blocks;

    private float[,] _noiseMap;
    private World _world;

    public void Start()
    {
        _world = new World();
    }

    public World GetWorld()
    {
        return _world;
    }

    public void GeneratorMap()
    {
        _noiseMap = Noise.GeneratePerlinNoise(MapWidth, MapHeight, Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);
        /*
        // 노이즈 맵 경계선 값들 보간
        Noise.MakeSeamlessNoiseHorizontally(_noiseMap, 30);
        Noise.MakeSeamlessNoiseVertically(_noiseMap, 30);
        */

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(_noiseMap);

        if (Application.isPlaying)
        {
            _world.DestroyWorld();
            _world.GenerateWorld(_noiseMap);
            _world.RenderWorld();
        }
    } 

    public Vector2Int CalculateChunkCoords(Vector3 pos)
    {
        return new Vector2Int(
            (int)((pos.x + 0.5f) / ChunkSize.x),
            (int)((pos.z + 0.5f) / ChunkSize.z)
        );
    }

    private void OnValidate()
    {
        if (MapWidth < 1)
        {
            MapWidth = 1;
        }
        if (MapHeight < 1)
        {
            MapHeight = 1;
        }
        if (Lacunarity < 1)
        {
            Lacunarity = 1;
        }
        if (Octaves < 0)
        {
            Octaves = 0;
        }     

        if (WorldChunkWidth < 1)
        {
            WorldChunkWidth = 1;
        }
        if (WorldChunkHeight < 1)
        {
            WorldChunkHeight = 1;
        }

        if (ChunkSize.x < 1)
        {
            ChunkSize.x = 1;
        }
        if (ChunkSize.x > MapWidth / WorldChunkWidth)
        {
            ChunkSize.x = MapWidth / WorldChunkWidth;
        }
        if (ChunkSize.y < 1)
        {
            ChunkSize.y = 1;
        }
        if (ChunkSize.z < 1)
        {
            ChunkSize.z = 1;
        }
        if (ChunkSize.z > MapHeight / WorldChunkHeight)
        {
            ChunkSize.z = MapHeight / WorldChunkHeight;
        }
    }
}
