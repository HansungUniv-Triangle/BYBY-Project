using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class World
{
    public GameObject gameObject;
    private Chunk[,] _worldMap;

    private System.Random rng;
    private Dictionary<Chunk, List<Vector3Int>> _chunkBlockPos;

    private readonly Vector3[] _checkOffsetBlock = {
        Vector3.back, 
        Vector3.forward,
        Vector3.left,
        Vector3.right
    };

    private readonly Vector2Int[] _checkOffsetChunk = {
        Vector2Int.zero,
        Vector2Int.down,
        Vector2Int.up,
        Vector2Int.left,
        Vector2Int.right,
        Vector2Int.left + Vector2Int.down,
        Vector2Int.left + Vector2Int.up,
        Vector2Int.right + Vector2Int.down,
        Vector2Int.right + Vector2Int.up,
    };

    #region GetMethod

    public Chunk GetChunk(int x, int z) => _worldMap[x, z];
    public Chunk GetChunk(Vector2Int pos) => _worldMap[pos.x, pos.y];
    public Chunk GetChunk(Vector3 pos)
    {
        var chunkPos = WorldManager.Instance.CalculateChunkCoords(pos);
        if (IsPositionInWorld(chunkPos))
            return _worldMap[chunkPos.x, chunkPos.y];
        else
            return null;
    }
    public Chunk[,] GetChunkAll => _worldMap;
    public int GetWidth() => _worldMap.GetLength(0);
    public int GetHeight() => _worldMap.GetLength(1);
    public Vector3Int GetBlockCoords(Vector3 pos) => new((int)(pos.x + 0.5f), (int)(pos.y + 0.5f), (int)(pos.z + 0.5f));
    public Vector3Int GetBlockCoords(float x, float y, float z) => new((int)(x + 0.5f), (int)(y + 0.5f), (int)(z + 0.5f));
    public double GetRandomValue() => rng.NextDouble();

    #endregion
    
    public World()
    {
        gameObject = new GameObject("World", new System.Type[] { });
        _chunkBlockPos = new Dictionary<Chunk, List<Vector3Int>>();
    }

    public void Init(int worldChunkWidth, int worldChunkHeight)
    {
        _worldMap = new Chunk[worldChunkWidth, worldChunkHeight];
        rng = new System.Random(WorldManager.Instance.Seed);
    }

    public void SetChunk(int x, int z, Chunk chunk)
    {
        _worldMap[x, z] = chunk;
    }

    public void GenerateWorld(float[,] noiseMap)
    {
        var WorldChunkWidth = WorldManager.Instance.WorldChunkWidth;
        var WorldChunkHeight = WorldManager.Instance.WorldChunkHeight;

        Init(WorldChunkWidth, WorldChunkHeight);

        for (var x = 0; x < WorldChunkWidth; x++)
        {
            for (var z = 0; z < WorldChunkHeight; z++)
            {
                GenerateChunk(noiseMap, x, z);
            }
        }

        SetNatureEnvironment();
    }

    private void GenerateChunk(float[,] noiseMap, int xWorld, int zWorld)
    {
        var MapWidth = WorldManager.Instance.MapWidth;
        var MapHeight = WorldManager.Instance.MapHeight;

        var TerrainHeight = WorldManager.Instance.TerrainHeight;
        var TerrainBaseHeight = WorldManager.Instance.TerrainBaseHeight;

        var ChunkSize = WorldManager.Instance.ChunkSize;

        if (noiseMap == null) { return; }
        if (ChunkSize.x > MapWidth || ChunkSize.z > MapHeight) { return; }

        var chunkCoord = new Vector2Int(xWorld, zWorld);
        var chunk = new Chunk(this, chunkCoord, ChunkSize);

        chunk.InitChunkMap();
        chunk.SetSize(ChunkSize);

        var xStart = xWorld * ChunkSize.x;
        var zStart = zWorld * ChunkSize.z;

        for (var x = xStart; x < xStart + ChunkSize.x; x++)
        {
            for (var z = zStart; z < zStart + ChunkSize.z; z++)
            {
                var noiseHeight = (int)(noiseMap[x, z] * TerrainHeight) + TerrainBaseHeight - 1;

                /*
                // 산 같은 지형
                if (noiseHeight > 45)
                {
                    var offset = noiseHeight - 45;
                    noiseHeight += offset;
                }
                */

                for (var y = ChunkSize.y; y >= 0; y--)
                {
                    if (y <= noiseHeight)
                    {
                        var pos = new Vector3Int(x, y, z);
                        var block = GetBlock(noiseHeight, pos);
                        if (block != null)
                            chunk.SetBlock(pos, block);
                    }
                }
            }
        }
    }

    private void SetNatureEnvironment()
    {
        for (var x = 0; x < _worldMap.GetLength(0); x++)
        {
            for (var z = 0; z < _worldMap.GetLength(1); z++)
            {
                _worldMap[x, z].SetGrassBlock();
            }
        }
    }

    private Block GetBlock(int noiseHeight, Vector3Int pos)
    {
        float Scale = WorldManager.Instance.Scale;
        float Seed = WorldManager.Instance.Seed;
        float BlockThreshold = WorldManager.Instance.BlockThreshold;
        float SandThreshold = WorldManager.Instance.SandThreshold;
        float NoneThreshold = WorldManager.Instance.NoneThreshold;
        Vector3 Offset3D = WorldManager.Instance.Offset3D;

        var Blocks = WorldManager.Instance.Blocks;

        if (pos.y == 0)
            return Blocks[(int)Block.BlockType.Bedrock];

        var probability = Noise.Get3DNoiseValue(pos.x, pos.y, pos.z, Scale, Seed, Offset3D) - noiseHeight * 0.01f * Scale;

        if (probability < NoneThreshold)
            return null;
        else if (probability < BlockThreshold)
            return Blocks[(int)Block.BlockType.Dirt];
        else if (probability < SandThreshold)
            return Blocks[(int)Block.BlockType.Sand];
        else
            return Blocks[(int)Block.BlockType.Stone];
    }

    public void RenderWorld()
    {
        var WorldChunkWidth = WorldManager.Instance.WorldChunkWidth;
        var WorldChunkHeight = WorldManager.Instance.WorldChunkHeight;

        for (var x = 0; x < WorldChunkWidth; x++)
        {
            for (var z = 0; z < WorldChunkHeight; z++)
            {
                var chunk = GetChunk(x, z);
                chunk.CreateChunkMesh();
                chunk.UpdateChunkMesh();
            }
        }
    }

    public void DestroyBlock(Vector3 pos)
    {
        var chunk = GetChunk(pos);
        if (chunk == null) return;

        var blockPos = GetBlockCoords(pos);
       
        chunk.DestroyBlock(blockPos);
    }

    public void HitBlock(Vector3 pos, int damage)
    {
        var chunk = GetChunk(pos);
        if (chunk == null) return;

        var blockPos = GetBlockCoords(pos);

        chunk.HitBlock(blockPos, damage);
    }

    public void ExplodeBlocks(Vector3 center, int radius, int damage)
    {
        _chunkBlockPos.Clear();

        for (var x = center.x - radius; x < center.x + radius; x++)
        {
            for (var y = center.y - radius; y < center.y + radius; y++)
            {
                for (var z = center.z - radius; z < center.z + radius; z++)
                {
                    var blockPos = GetBlockCoords(x, y, z);
                    var distSqr = (blockPos.x - center.x) * (blockPos.x - center.x)
                        + (blockPos.y - center.y) * (blockPos.y - center.y)
                        + (blockPos.z - center.z) * (blockPos.z - center.z);

                    if (distSqr < radius * radius)
                    {
                        var chunk = GetChunk(blockPos);
                        if (chunk == null) continue;

                        if (!_chunkBlockPos.ContainsKey(chunk))
                        {
                            _chunkBlockPos[chunk] = new List<Vector3Int>();
                        }
                        else
                        {
                            _chunkBlockPos[chunk].Add(blockPos);
                        }
                    }
                }
            }
        }

        var keys = _chunkBlockPos.Keys.ToList();
        for (int i = 0; i < _chunkBlockPos.Keys.Count; i++)
        {
            var chunk = keys[i];
            chunk.HitBlocks(_chunkBlockPos[chunk], damage);
        }

        UpdateAroundChunks(center);   // 일괄 처리
    }

    public void ExplodeBlocksNoAnimation(Vector3 center, int radius)
    {
        for (var x = center.x - radius; x < center.x + radius; x++)
        {
            for (var y = center.y - radius; y < center.y + radius; y++)
            {
                for (var z = center.z - radius; z < center.z + radius; z++)
                {
                    var blockPos = GetBlockCoords(x, y, z);
                    var distSqr = (blockPos.x - center.x) * (blockPos.x - center.x)
                        + (blockPos.y - center.y) * (blockPos.y - center.y)
                        + (blockPos.z - center.z) * (blockPos.z - center.z);

                    if (distSqr < radius * radius)
                    {
                        DestroyBlock(blockPos);
                    }
                }
            }
        }
        UpdateAroundChunks(center);   // 일괄 처리
    }

    private void UpdateAroundChunks(Vector3 pos)
    {
        // 해당 위치의 청크에서 상하좌우대각선 청크 업데이트
        var currentChunkPos = WorldManager.Instance.CalculateChunkCoords(pos);
       
        for (int i = 0; i < _checkOffsetChunk.Length; i++)
        {
            var nextChunkPos = currentChunkPos + _checkOffsetChunk[i];
            if(IsPositionInWorld(nextChunkPos))
            {
                var chunk = _worldMap[nextChunkPos.x, nextChunkPos.y];
                chunk.CreateChunkMesh();
                chunk.UpdateChunkMesh();
            }
        }
    }

    public void UpdateAroundChunks(Chunk currentChunk, Vector3 pos)
    {
        // 해당 위치의 블럭에서 상하좌우 한 칸에 다른 청크가 있을 경우 업데이트
        var chunkPos = currentChunk.GetChunkCoord();

        for (int i = 0; i < _checkOffsetBlock.Length; i++)
        {
            var nextChunkPos = WorldManager.Instance.CalculateChunkCoords(pos + _checkOffsetBlock[i]);
            if (chunkPos != nextChunkPos && IsPositionInWorld(nextChunkPos))
            {
                var chunk = _worldMap[nextChunkPos.x, nextChunkPos.y];
                chunk.CreateChunkMesh();
                chunk.UpdateChunkMesh();
            }
        }
    }

    public bool IsPositionInWorld(Vector2Int pos)
    {
        return (pos.x >= 0 && pos.x < _worldMap.GetLength(0)) && (pos.y >= 0 && pos.y < _worldMap.GetLength(1));
    }

    public void DestroyWorld()
    {
        if (_worldMap == null)
            return;
        
        var child = gameObject.transform.GetComponentsInChildren<Transform>(true);
        foreach (var c in child)
        {
            if (c == gameObject.transform) continue;
            c.parent = null;
            UnityEngine.Object.Destroy(c.gameObject);
        }

        ClearWorldMap();
    }

    public void ClearWorldMap()
    {
        for (var x = 0; x < _worldMap.GetLength(0); x++)
        {
            for (var y = 0; y < _worldMap.GetLength(1); y++)
            {
                _worldMap[x, y] = null;
            }
        }
        _worldMap = null;
    }
}