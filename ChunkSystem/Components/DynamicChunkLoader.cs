using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicChunkLoader : IChunkLoader
{
    private readonly ChunkSystemSettings _settings;
    private readonly HashSet<Vector2Int> _loadedChunks = new();

    public DynamicChunkLoader(ChunkSystemSettings settings)
    {
        _settings = settings;
    }

    public IEnumerable<Vector2Int> GetChunksToLoad(Vector3 playerPosition)
    {
        var playerChunkCoord = WorldToChunkCoord(playerPosition);
        var chunksToLoad = new List<Vector2Int>();

        for (int x = -_settings.chunksX / 2; x <= _settings.chunksX / 2; x++)
        {
            for (int z = -_settings.chunksZ / 2; z <= _settings.chunksZ / 2; z++)
            {
                var coord = new Vector2Int(
                    playerChunkCoord.x + x,
                    playerChunkCoord.y + z
                );

                if (!_loadedChunks.Contains(coord))
                {
                    chunksToLoad.Add(coord);
                }
            }
        }

        return chunksToLoad;
    }

    public IEnumerable<Vector2Int> GetChunksToUnload(Vector3 playerPosition)
    {
        var playerChunkCoord = WorldToChunkCoord(playerPosition);
        var chunksToUnload = new List<Vector2Int>();
        var loadRadius = Mathf.Max(_settings.chunksX, _settings.chunksZ) / 2;

        // Используем ToList() для безопасной итерации
        foreach (var coord in _loadedChunks.ToList())
        {
            var distance = Vector2Int.Distance(coord, playerChunkCoord);
            if (distance > loadRadius + 1)
            {
                chunksToUnload.Add(coord);
            }
        }

        return chunksToUnload;
    }

    public void LoadChunk(Vector2Int coord)
    {
        _loadedChunks.Add(coord);
    }

    public void UnloadChunk(Vector2Int coord)
    {
        _loadedChunks.Remove(coord);
    }

    private Vector2Int WorldToChunkCoord(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / _settings.chunkWorldSize),
            Mathf.FloorToInt(worldPos.z / _settings.chunkWorldSize)
        );
    }
}