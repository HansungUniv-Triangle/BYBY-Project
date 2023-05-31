using Fusion;
using System;
using UnityEngine;

public class WorldManager : Singleton<WorldManager>
{
    [Space(5f)]
    public bool AutoUpdate;

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

    [Header("3D Noise Setting")]
    [Space(5f)]
    public Vector3 Offset3D;
    public float Scale = 0.07f;
    public float NoneThreshold = 0.38f;
    public float SandThreshold = 0.6f;
    public float BlockThreshold = 0.47f;
    public float TreeThreshold = 0.04f;

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
    public int WoodHeight = 6;
    public int LeafLength = 3;
    public int LeafHeight = 3;

    [Space(5f)]
    public GameObject BlockPrefab;
    public GameObject ChunkPrefab;
    public GameObject BarrierPrefab;
    
    [Space(5f)]
    public Block[] Blocks;

    private float[,] _noiseMap;
    private World _world;

    public void Start()
    {
        //Seed = DateTime.Now.Millisecond;
        Seed = 230412;
    }

    public World GetWorld()
    {
        return _world;
    }

    public void GeneratorMap(int seed)
    {
        _noiseMap = Noise.GeneratePerlinNoise(MapWidth, MapHeight, seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);
        Seed = seed;
        
        if (Application.isPlaying)
        {
            _world = new World();
            _world.DestroyWorld();
            _world.GenerateWorld(_noiseMap);
            _world.RenderWorld();

            GenerateBarrier();
        }
    }

    private void GenerateBarrier()
    {
        var mapWidth = ChunkSize.x * WorldChunkWidth;
        var mapHeight = ChunkSize.z * WorldChunkHeight;
        
        // 하드 코딩
        var a = Instantiate(BarrierPrefab, _world.gameObject.transform).transform;
        a.position = new Vector3(mapWidth / 2f - 0.5f, ChunkSize.y / 2f - 0.5f, -0.5f);
        a.localScale = new Vector3(mapWidth, ChunkSize.y, 1);
        a.rotation = Quaternion.Euler(0, 180, 0);
        
        var b = Instantiate(BarrierPrefab, _world.gameObject.transform).transform;
        b.position = new Vector3(mapWidth / 2f - 0.5f, ChunkSize.y / 2f - 0.5f, mapHeight - 0.5f);
        b.localScale = new Vector3(mapWidth, ChunkSize.y, 1);
        b.rotation = Quaternion.Euler(0, 0, 0);
        
        var c = Instantiate(BarrierPrefab, _world.gameObject.transform).transform;
        c.position = new Vector3(mapWidth - 0.5f, ChunkSize.y / 2f - 0.5f, mapHeight / 2f - 0.5f);
        c.localScale = new Vector3(mapHeight, ChunkSize.y, 1);
        c.rotation = Quaternion.Euler(0, 90, 0);
        
        var d = Instantiate(BarrierPrefab, _world.gameObject.transform).transform;
        d.position = new Vector3(-0.5f, ChunkSize.y / 2f - 0.5f, mapHeight / 2f - 0.5f);
        d.localScale = new Vector3(mapHeight, ChunkSize.y, 1);
        d.rotation = Quaternion.Euler(0, -90, 0);
        
        var e = Instantiate(BarrierPrefab, _world.gameObject.transform).transform;
        e.position = new Vector3(mapWidth / 2f - 0.5f, ChunkSize.y - 0.5f, mapHeight / 2f - 0.5f);
        e.localScale = new Vector3(mapWidth, mapHeight, 1);
        e.rotation = Quaternion.Euler(-90, 0, 0);
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
    public void SetWorldValues(GameMode gameMode)
    {
        switch (gameMode)
        {
            case GameMode.Shared:
                Scale = 0.04f;
                NoneThreshold = 0.3f;
                TreeThreshold = 0.024f;
                TerrainHeight = 13;
                TerrainBaseHeight = 2;

                break;

            case GameMode.Single:
                TreeThreshold = 0.02f;
                TerrainHeight = 6;
                TerrainBaseHeight = 2;

                break;

            default:
                break;
        }
    }
    protected override void Initiate()
    {
        
    }
}
