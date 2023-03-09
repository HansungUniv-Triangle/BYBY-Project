using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class DamagedBlock
{
    private GameObject _gameObject;
    private float _hp;
    
    public bool isBroken = false;

    private Mesh _mesh;
    private MeshFilter _meshFilter;

    private List<Vector3> _vertices;
    private List<int> _indices;
    private List<Vector2> _uvs;

    private Chunk _chunk;

    private readonly int textureAtlasWidth = 9;
    private readonly int textureAtlasHeight = 10;

    private float textureAtlasCellWidth;
    private float textureAtlasCellHeight;

    private Vector3Int _position;

    private DG.Tweening.Sequence _shakingSequence;

    public DamagedBlock(Chunk chunk, Block block, Vector3Int position)
    {
        _position = position;
        _chunk = chunk;
        _hp = block.GetMaxHP();

        _gameObject = Object.Instantiate(WorldManager.Instance.BlockPrefab, position, Quaternion.identity);
        _gameObject.name = $"Block {position.x} {position.y} {position.z}";
        _gameObject.transform.SetParent(_chunk.GetGameObject().transform);

        _mesh = new Mesh();
        _meshFilter = _gameObject.GetComponent<MeshFilter>();

        _shakingSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Append(_gameObject.transform.DOShakePosition(0.5f, 0.25f, 20, 90))
            .OnComplete(() =>
            {
                ShakingEndEvent();
            });

        textureAtlasCellWidth = 1f / textureAtlasWidth;
        textureAtlasCellHeight = 1f / textureAtlasHeight;

        _vertices = new List<Vector3>(24);
        _indices = new List<int>(36);
        _uvs = new List<Vector2>(24);

        CreateMesh(block, Vector3Int.zero);
        UpdateMesh();
    }

    public MeshFilter GetMeshFilter() { return _meshFilter; }

    public void DecreaseHP(float damage)
    {
        _gameObject.SetActive(true);
        _shakingSequence.Restart();

        _hp -= damage;

        if (_hp <= 0)
        {
            _hp = 0;
            isBroken = true;
        }
    }

    public bool IsShaking()
    {
        return _shakingSequence.IsPlaying();
    }

    public void ShakingEndEvent()
    {
        _chunk.CombineOneMesh(_position);
        _gameObject.SetActive(false);
    }

    public void DestroyGameObject()
    {
        _shakingSequence.Kill();
        _gameObject.transform.parent = null;
        Object.Destroy(_gameObject);
    }

    public void SetMeshRendererEnable(bool state)
    {
        _gameObject.GetComponent<MeshRenderer>().enabled = state;
    }

    private void CreateMesh(Block block, Vector3Int position)
    {
        for (var dir = 0; dir < MeshBlockData.CheckDireactions.Length; dir++)
        {
            for (var idx = 0; idx < MeshBlockData.FaceNumber.GetLength(1); idx++)
            {
                var vIdx = MeshBlockData.FaceNumber[dir, idx];
                var vPos = position + MeshBlockData.Vertices[vIdx];
                _vertices.Add(vPos);
            }

            for (var idx = 0; idx < MeshBlockData.Triangles.Length; idx++)
            {
                _indices.Add(_vertices.Count - 4 + MeshBlockData.Triangles[idx]);
            }

            AddTextureUV(block.GetTextureID(dir));
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
    private void UpdateMesh()
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
    }
}
