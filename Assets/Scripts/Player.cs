using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int ActiveChunksRange;
    private Vector2Int _chunkCoord;
    private Vector2Int _prevChunkCoord;
    private List<Vector2Int> _toRemoveChunkCoord;

    private WorldManager _worldGenerator;

    void Start()
    {
        _worldGenerator = WorldManager.Instance;
        _toRemoveChunkCoord = new List<Vector2Int>();
    }

    void Update()
    {
        _chunkCoord = _worldGenerator.CalculateChunkCoords(transform.position);

        if(_chunkCoord != _prevChunkCoord)
        {
            _prevChunkCoord = _chunkCoord;
            DectivatesChunks();
            ActivatesChunks();
        }
    }
    private void ActivatesChunks()
    {
        if (_worldGenerator.GetWorld() == null)
            return;

        for (var x = _chunkCoord.x - ActiveChunksRange; x <= _chunkCoord.x + ActiveChunksRange; x++)
        {
            for (var z = _chunkCoord.y - ActiveChunksRange; z <= _chunkCoord.y + ActiveChunksRange; z++)
            {
                var pos = new Vector2Int(x, z);

                if (_worldGenerator.GetWorld().IsPositionInWorld(pos))
                {
                    _toRemoveChunkCoord.Add(pos);

                    var chunk = _worldGenerator.GetWorld().GetChunk(pos);
                    chunk.ActivatesMesh();
                }
            }
        }
    }

    private void DectivatesChunks()
    {
        foreach(var chunkCoord in _toRemoveChunkCoord)
        {
            var chunk = _worldGenerator.GetWorld().GetChunk(chunkCoord.x, chunkCoord.y);
            chunk.DeactivatesMesh();
        }
        _toRemoveChunkCoord.Clear();
    }
}
