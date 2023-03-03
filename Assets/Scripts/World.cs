using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class World
{
    public GameObject gameObject;
    private Chunk[,] _worldMap;

    private readonly Vector3[] checkOffsetBlock = {
        new Vector3(0, 0, -1),
        new Vector3(0, 0, 1),
        new Vector3(-1, 0, 0),
        new Vector3(1, 0, 0)
    };

    private readonly Vector2Int[] checkOffsetChunk = {
        new Vector2Int(0, 0),
        new Vector2Int(0, -1),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(1, 1)
    };

    public World()
    {
        gameObject = new GameObject("World", new System.Type[] { });
    }

    public void Init(int worldChunkWidth, int worldChunkHeight)
    {
        _worldMap = new Chunk[worldChunkWidth, worldChunkHeight];
    }

    public Chunk GetChunk(int x, int z)
    {
        return _worldMap[x, z];
    }
    
    public Chunk GetChunk(Vector2Int pos)
    {
        return _worldMap[pos.x, pos.y];
    }

    public Chunk GetChunk(Vector3 pos)
    {
        var chunkPos = WorldManager.Instance.CalculateChunkCoords(pos);
        if (IsPositionInWorld(chunkPos))
            return _worldMap[chunkPos.x, chunkPos.y];
        else
            return null;
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
                // 산 같은 지형, 값 증폭
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

    private Block GetBlock(int noiseHeight, Vector3Int pos)
    {
        var Blocks = WorldManager.Instance.Blocks;

        // 블럭이 나타나는 조건들
        if (pos.y == 0)
            return Blocks[(int)Block.BlockType.Bedrock];
        if (pos.y == noiseHeight)
            return Blocks[(int)Block.BlockType.Grass];
        else if (pos.y > noiseHeight - 4)
            return Blocks[(int)Block.BlockType.Dirt];
        else
        {
            return Blocks[(int)Block.BlockType.Stone];
            /*
            if (Noise.Get3DNoiseValue(pos.x, pos.y, pos.z, Scale, Seed) > CaveThreshold)
            {
                return Blocks[2];
            }
            */
        }

        //return null;
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
        updateChunks(chunk, pos);
    }

    public void ExplodeBlocks(Vector3 center, int radius)
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
        updateChunks(center);   // 일괄 처리
    }

    public Vector3Int GetBlockCoords(Vector3 pos)
    {
        return new Vector3Int((int)(pos.x + 0.5f), (int)(pos.y + 0.5f), (int)(pos.z + 0.5f));
        //return new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
    }

    public Vector3Int GetBlockCoords(float x, float y, float z)
    {
        return new Vector3Int((int)(x + 0.5f), (int)(y + 0.5f), (int)(z + 0.5f));
        //return new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(z));
    }

    private void updateChunks(Vector3 pos)
    {
        // 해당 위치의 청크에서 상하좌우대각선 청크 업데이트
        var currentChunkPos = WorldManager.Instance.CalculateChunkCoords(pos);
        for (int i = 0; i < checkOffsetChunk.Length; i++)
        {
            var nextChunkPos = currentChunkPos + checkOffsetChunk[i];
            if(IsPositionInWorld(nextChunkPos))
            {
                var chunk = _worldMap[nextChunkPos.x, nextChunkPos.y];
                chunk.CreateChunkMesh();
                chunk.UpdateChunkMesh();
            }
        }
    }

    private void updateChunks(Chunk currentChunk, Vector3 pos)
    {
        // 해당 위치의 블럭에서 상하좌우 한 칸에 다른 청크가 있을 경우 업데이트
        var chunkPos = currentChunk.GetChunkCoord();

        //currentChunk.CreateChunkMesh();
        //currentChunk.UpdateChunkMesh();

        for (int i = 0; i < checkOffsetBlock.Length; i++)
        {
            var nextChunkPos = WorldManager.Instance.CalculateChunkCoords(pos + checkOffsetBlock[i]);
            if (chunkPos != nextChunkPos && IsPositionInWorld(nextChunkPos))
            {
                var chunk = _worldMap[nextChunkPos.x, nextChunkPos.y];
                chunk.CreateChunkMesh();
                chunk.UpdateChunkMesh();
            }
        }
    }

    public int GetWidth()
    {
        return _worldMap.GetLength(0);
    }

    public int GetHeight()
    {
        return _worldMap.GetLength(1);
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
            Object.Destroy(c.gameObject);
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