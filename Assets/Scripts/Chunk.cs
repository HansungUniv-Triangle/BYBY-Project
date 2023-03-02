using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Chunk
{
    private GameObject _chunk;
    private World _world;

    private Vector2Int _chunkCoord;
    private Vector3Int _chunkSize;

    private Dictionary<Vector3Int, Block> _blockMap;
    
    private Mesh _mesh;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    private List<Vector3> _vertices;
    private List<int> _indices;
    private List<Vector2> _uvs;

    private readonly int textureAtlasWidth = 9;
    private readonly int textureAtlasHeight = 10;

    private float textureAtlasCellWidth;
    private float textureAtlasCellHeight;

    private Dictionary<Vector3Int, DamagedBlock> _damagedBlocks;

    public Chunk(World world, Vector2Int chunkCoord, Vector3Int chunkSize)
    {
        _world = world;
        _world.SetChunk(chunkCoord.x, chunkCoord.y, this);

        _chunkCoord = chunkCoord;
        _chunkSize = chunkSize;

        _chunk = UnityEngine.Object.Instantiate(WorldManager.Instance.ChunkPrefab, Vector3.zero, Quaternion.identity);
        _chunk.name = $"Chunk {chunkCoord.x} {chunkCoord.y}";
        _chunk.transform.SetParent(_world.gameObject.transform);

        _blockMap = new Dictionary<Vector3Int, Block>();

        _mesh = new Mesh();
        _meshRenderer = _chunk.GetComponent<MeshRenderer>();
        _meshFilter = _chunk.GetComponent<MeshFilter>();
        _meshCollider = _chunk.GetComponent<MeshCollider>();

        _vertices = new List<Vector3>();
        _indices = new List<int>();
        _uvs = new List<Vector2>();

        _damagedBlocks = new Dictionary<Vector3Int, DamagedBlock>();

        textureAtlasCellWidth = 1f / textureAtlasWidth;
        textureAtlasCellHeight = 1f / textureAtlasHeight;
    }

    public GameObject GetGameObject()
    {
        return _chunk;
    }

    public Vector2Int GetChunkCoord()
    {
        return _chunkCoord;
    }

    public Block GetBlock(Vector3Int pos)
    {
        if(_blockMap.ContainsKey(pos)) 
            return _blockMap[pos];
        return null;
    }

    public void SetBlock(Vector3Int pos, Block block)
    {
        _blockMap[pos] = block;
    }

    public void HitBlock(Vector3Int pos, int damage)
    {
        DamagedBlock damagedBlock;

        if (!_damagedBlocks.ContainsKey(pos))
        {
            if (!_blockMap.ContainsKey(pos) || 
                _blockMap[pos].GetBlockType() == Block.BlockType.Bedrock)
                return;

            damagedBlock = new DamagedBlock(this, _blockMap[pos], pos);
            damagedBlock.SetMeshRendererEnable(_meshRenderer.enabled);
            _damagedBlocks[pos] = damagedBlock;
            
            DestroyBlock(pos);
            CreateChunkMesh();
        }
        else
        {
            damagedBlock = _damagedBlocks[pos];
        }

        UpdateChunkMeshWithoutOneMesh(pos);
        damagedBlock.DecreaseHP(damage);

        if(damagedBlock.isBroken)
        {
            damagedBlock.DestroyGameObject();
            _damagedBlocks.Remove(pos);
        }        
    }

    public void DestroyBlock(Vector3Int pos)
    {
        if (_blockMap.ContainsKey(pos))
        {
            if (_blockMap[pos].GetBlockType() == Block.BlockType.Bedrock)
                return;
            _blockMap.Remove(pos);
        }
        else
        {
            if (_damagedBlocks.ContainsKey(pos))
            {
                _damagedBlocks[pos].DestroyGameObject();
                _damagedBlocks.Remove(pos);
            }
        }
    }

    public void SetSize(Vector3Int chunkSize)
    {
        _chunkSize = chunkSize;
    }

    public void ActivatesMesh()
    {
        _meshRenderer.enabled = true;
        foreach (var block in _damagedBlocks)
        {
            block.Value.SetMeshRendererEnable(true);
        }
    }

    public void DeactivatesMesh()
    {
        _meshRenderer.enabled = false;
        foreach(var block in _damagedBlocks)
        {
            block.Value.SetMeshRendererEnable(false);
        }
    }

    public void InitChunkMap()
    {
        _blockMap.Clear();
    }

    public void CreateChunkMesh()
    {
        _vertices.Clear();
        _indices.Clear();
        _uvs.Clear();

        var xStart = _chunkCoord.x * _chunkSize.x;
        var zStart = _chunkCoord.y * _chunkSize.z;

        for (var x = xStart; x < xStart + _chunkSize.x; x++)
        {
            for (var y = 0; y < _chunkSize.y; y++)
            {
                for (var z = zStart; z < zStart + _chunkSize.z; z++)
                {
                    var blockPos = new Vector3Int(x, y, z);
                    if (!_blockMap.ContainsKey(blockPos)) { continue; }

                    for(var dir = 0; dir < MeshBlockData.CheckDireactions.Length; dir++)
                    {
                        // 맨 아래 (y == 0) 블럭의 밑면의 경우, 그리지 않기
                        if (y == 0 && dir == 5) { continue; }

                        var checkBlockPos = blockPos + MeshBlockData.CheckDireactions[dir];

                        if(!_blockMap.ContainsKey(checkBlockPos) || !_blockMap[checkBlockPos].GetSolidType())
                        {
                            // 다음 청크에 있을 수도 있어 예외 처리
                            var checkChunkPos = new Vector2Int(_chunkCoord.x + MeshBlockData.CheckDireactions[dir].x,
                                                               _chunkCoord.y + MeshBlockData.CheckDireactions[dir].z);

                            // 다음 청크에서 블럭 존재할 경우, 해당 면 그리지 않기
                            if (_world.IsPositionInWorld(checkChunkPos) &&
                                _world.GetChunk(checkChunkPos).GetBlock(checkBlockPos) != null)
                                continue;

                            // Culling 방식
                            for(var idx = 0; idx < MeshBlockData.FaceNumber.GetLength(1); idx++)
                            {
                                var vIdx = MeshBlockData.FaceNumber[dir, idx];
                                var vPos = blockPos + MeshBlockData.Vertices[vIdx] - new Vector3(_chunkSize.x / 2f, 0, _chunkSize.z / 2f)
                                    + new Vector3Int(_chunkSize.x / 2, 0, _chunkSize.z / 2);
                                _vertices.Add(vPos);
                            }

                            for(var idx = 0; idx < MeshBlockData.Triangles.Length; idx++)
                            {
                                _indices.Add(_vertices.Count - 4 + MeshBlockData.Triangles[idx]);
                            }

                            AddTextureUV(_blockMap[blockPos].GetTextureID(dir));
                        }
                    }
                }
            }
        }   
    }

    private void AddTextureUV(int textureID)
    {
        int x = textureID % textureAtlasWidth;
        int y = textureAtlasHeight - (textureID / textureAtlasWidth) - 1;

        float uvX = x * textureAtlasCellWidth;
        float uvY = y * textureAtlasCellHeight;

        float offset = 0.01f;
        
        _uvs.Add(new Vector2(uvX + offset, uvY + textureAtlasCellHeight - offset));
        _uvs.Add(new Vector2(uvX + textureAtlasCellWidth - offset, uvY + textureAtlasCellHeight - offset));
        _uvs.Add(new Vector2(uvX + textureAtlasCellWidth - offset, uvY + offset));      
        _uvs.Add(new Vector2(uvX + offset, uvY + offset));
    }

    public void UpdateChunkMesh()
    {
        if(_mesh == null) { return; }

        _mesh.Clear();

        _mesh.SetVertices(_vertices);
        _mesh.SetIndices(_indices, MeshTopology.Triangles, 0);
        _mesh.SetUVs(0, _uvs);

        _mesh.RecalculateBounds();
        _mesh.RecalculateTangents();
        _mesh.RecalculateNormals();

        _meshFilter.mesh.Clear();
        _meshFilter.mesh = _mesh;

        CombineMeshes();

        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = _meshFilter.mesh;
    }

    public void UpdateChunkMeshWithoutOneMesh(Vector3Int notIncludePos)
    {
        if (_mesh == null) { return; }

        _mesh.Clear();

        _mesh.SetVertices(_vertices);
        _mesh.SetIndices(_indices, MeshTopology.Triangles, 0);
        _mesh.SetUVs(0, _uvs);

        _mesh.RecalculateBounds();
        _mesh.RecalculateTangents();
        _mesh.RecalculateNormals();

        _meshFilter.mesh.Clear();
        _meshFilter.mesh = _mesh;

        CombineMeshes(notIncludePos);

        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = _meshFilter.mesh;
    }

    public void CombineMeshes()
    {
        if (_damagedBlocks.Count == 0)
            return;

        MeshFilter[] meshFilters = _chunk.GetComponentsInChildren<MeshFilter>(true);
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int j = 0;
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i] != _meshFilter &&
                meshFilters[i].gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > 0)
                continue;

            combine[j].mesh = meshFilters[i].sharedMesh;
            combine[j].transform = meshFilters[i].transform.localToWorldMatrix;
            j++;
        }

        if (j != combine.Length)
            Array.Resize(ref combine, j);

        _meshFilter.mesh = new Mesh();
        _meshFilter.mesh.CombineMeshes(combine);
    }

    public void CombineMeshes(Vector3Int excludePos)
    {
        if (_damagedBlocks.Count == 0)
            return;

        MeshFilter excludeMeshFilter = _damagedBlocks[excludePos].GetMeshFilter();

        MeshFilter[] meshFilters = _chunk.GetComponentsInChildren<MeshFilter>(true);
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        int j = 0;
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i] == excludeMeshFilter)
                continue;

            if (meshFilters[i] != _meshFilter &&
                meshFilters[i].gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > 0)
                continue;

            combine[j].mesh = meshFilters[i].sharedMesh;
            combine[j].transform = meshFilters[i].transform.localToWorldMatrix;
            j++;
        }

       if (j != combine.Length)
            Array.Resize(ref combine, j);

        _meshFilter.mesh = new Mesh();
        _meshFilter.mesh.CombineMeshes(combine);
    }

    public void CombineOneMesh(Vector3Int position)
    {
        MeshFilter meshFilter = _damagedBlocks[position].GetMeshFilter();
        CombineInstance[] combine = new CombineInstance[2];

        combine[0].mesh = meshFilter.mesh;
        combine[0].transform = meshFilter.transform.localToWorldMatrix;

        combine[1].mesh = _meshFilter.mesh;
        combine[1].transform = _meshFilter.transform.localToWorldMatrix;

        _meshFilter.mesh = new Mesh();
        _meshFilter.mesh.CombineMeshes(combine);

        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = _meshFilter.mesh;
    }
}