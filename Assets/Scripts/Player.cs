using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int ActiveChunksRange;
    private Vector2Int _chunkCoord;
    private Vector2Int _prevChunkCoord;
    private List<Vector2Int> _toRemoveChunkCoord;

    private WorldManager _worldGenerator;

    private void Start()
    {
        _worldGenerator = WorldManager.Instance;
        _toRemoveChunkCoord = new List<Vector2Int>();
    }

    private void Update()
    {
        ActivatesAllChunks();
        
        // ActivatesChunks();
        // _chunkCoord = _worldGenerator.CalculateChunkCoords(transform.position);
        //
        //  if(_chunkCoord != _prevChunkCoord)
        //  {
        //      _prevChunkCoord = _chunkCoord;
        //      ActivatesChunks();
        //      DectivatesChunks();
        //  }
    }
    
    private void ActivatesAllChunks()
    {
        if (_worldGenerator.GetWorld() == null)
            return;
    
        foreach (var chunk in _worldGenerator.GetWorld().GetChunkAll)
        {
            chunk.ActivatesMesh();
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
