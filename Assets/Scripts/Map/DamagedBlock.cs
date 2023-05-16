using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class DamagedBlock
{
    private GameObject _gameObject;
    private float _hp;
 
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

    private DG.Tweening.Sequence _sequence;

    private bool _isBreaking = false;
    private bool _isStartDestroy = false;
    private bool _isBroken = false;
    private bool _canCombine = false;

    public DamagedBlock(Chunk chunk, Block block, Vector3Int position)
    {
        _position = position;
        _chunk = chunk;
        _hp = block.GetMaxHP();

        _gameObject = Object.Instantiate(WorldManager.Instance.BlockPrefab, position, Quaternion.identity, _chunk.GetGameObject().transform);
        _gameObject.name = $"Block {position.x} {position.y} {position.z}";

        _mesh = new Mesh();
        _meshFilter = _gameObject.GetComponent<MeshFilter>();

        textureAtlasCellWidth = 1f / textureAtlasWidth;
        textureAtlasCellHeight = 1f / textureAtlasHeight;

        _vertices = new List<Vector3>(24);
        _indices = new List<int>(36);
        _uvs = new List<Vector2>(24);

        CreateMesh(block, Vector3Int.zero);
        UpdateMesh();
    }

    public MeshFilter GetMeshFilter() { return _meshFilter; }

    public void SetCanCombine(bool state)
    {
        _canCombine = state;
    }

    public void DecreaseHP(float damage)
    {
        if (_isStartDestroy)
            return;

        _gameObject.SetActive(true);

        if (_isBreaking)
            _sequence.Restart();
        else {
            _sequence = DOTween.Sequence()
                    .Append(_gameObject.transform.DOShakePosition(0.5f, 0.25f, 20, 90))
                    .OnStart(() =>
                    {
                        _isBreaking = true;
                        _canCombine = false;
                    })
                    .OnComplete(() =>
                    {
                        ShakingEndEvent();
                    });
        }

        _hp -= damage;

        if (_hp <= 0)
        {
            _hp = 0;
            _isBroken = true;
            return;
        }
    }

    public bool IsBroken() { return _isBroken; }

    public bool CanCombine() { return _canCombine; }

    public void ShakingEndEvent()
    {
        _isBreaking = false;
        _canCombine = true;
        _chunk.CombineOneMesh(_position);
        _gameObject.SetActive(false);
    }

    public void DestroyGameObject()
    {
        if (_isStartDestroy)
            return;

        _sequence.Kill();
        _sequence = DOTween.Sequence()
            .Append(_gameObject.transform.DOShakePosition(0.5f, 0.25f, 20, 90))
            .Join(_gameObject.transform.DOScale(0, 0.5f))
            .Join(_gameObject.transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360))
            .Join(_gameObject.transform.DOLocalMoveY(-0.5f, 0.7f).SetEase(Ease.InOutQuad)) 
            .OnStart(() =>
            {
                _isStartDestroy = true;
                _canCombine = false;
            })
            .OnComplete(() =>
            {
                _chunk.RemoveDamagedBlocks(_position);
                _gameObject.transform.parent = null;
                Object.Destroy(_gameObject);
            });
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
