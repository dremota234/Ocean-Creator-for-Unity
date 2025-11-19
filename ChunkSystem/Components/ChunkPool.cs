using System.Collections.Generic;
using UnityEngine;

public class ChunkPool : IChunkProvider
{
    private readonly Dictionary<Vector2Int, Chunk> _activeChunks = new();
    private readonly Queue<Chunk> _pool = new();
    private readonly ChunkSystemSettings _settings;
    private readonly Transform _parentTransform;

    public ChunkPool(ChunkSystemSettings settings, Transform parent)
    {
        _settings = settings;
        _parentTransform = parent;
        PrewarmPool();
    }

    private void PrewarmPool()
    {
        int poolSize = _settings.chunksX * _settings.chunksZ;
        for (int i = 0; i < poolSize; i++)
        {
            var chunkObject = new GameObject($"Chunk_Pooled_{i}");
            chunkObject.transform.parent = _parentTransform;
            chunkObject.SetActive(false);

            var meshFilter = chunkObject.AddComponent<MeshFilter>();
            var meshRenderer = chunkObject.AddComponent<MeshRenderer>();

            var chunk = new Chunk(Vector2Int.zero)
            {
                ChunkObject = chunkObject,
                MeshFilter = meshFilter,
                MeshRenderer = meshRenderer,
                Mesh = new Mesh()
            };

            _pool.Enqueue(chunk);
        }
    }

    public Chunk GetChunk(Vector2Int coord)
    {
        if (_activeChunks.TryGetValue(coord, out var existingChunk))
            return existingChunk;

        if (_pool.Count > 0)
        {
            var chunk = _pool.Dequeue();
            chunk.Coord = coord;
            chunk.ChunkObject.name = $"Chunk_{coord.x}_{coord.y}";
            chunk.ChunkObject.transform.position = new Vector3(
                coord.x * _settings.chunkWorldSize,
                0,
                coord.y * _settings.chunkWorldSize
            );
            chunk.ChunkObject.SetActive(true);
            chunk.IsActive = true;

            _activeChunks[coord] = chunk;
            return chunk;
        }

        return CreateNewChunk(coord);
    }

    public void ReturnChunk(Vector2Int coord)
    {
        if (_activeChunks.TryGetValue(coord, out var chunk))
        {
            _activeChunks.Remove(coord);
            chunk.ChunkObject.SetActive(false);
            chunk.IsActive = false;
            _pool.Enqueue(chunk);
        }
    }

    public bool IsChunkActive(Vector2Int coord) => _activeChunks.ContainsKey(coord);

    private Chunk CreateNewChunk(Vector2Int coord)
    {
        var chunkObject = new GameObject($"Chunk_{coord.x}_{coord.y}");
        chunkObject.transform.parent = _parentTransform;
        chunkObject.transform.position = new Vector3(
            coord.x * _settings.chunkWorldSize,
            0,
            coord.y * _settings.chunkWorldSize
        );

        var meshFilter = chunkObject.AddComponent<MeshFilter>();
        var meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        return new Chunk(coord)
        {
            ChunkObject = chunkObject,
            MeshFilter = meshFilter,
            MeshRenderer = meshRenderer,
            Mesh = new Mesh(),
            IsActive = true
        };
    }

    public void Cleanup()
    {
        foreach (var chunk in _activeChunks.Values)
        {
            if (chunk.ChunkObject != null)
                Object.Destroy(chunk.ChunkObject);
        }
        foreach (var chunk in _pool)
        {
            if (chunk.ChunkObject != null)
                Object.Destroy(chunk.ChunkObject);
        }
        _activeChunks.Clear();
        _pool.Clear();
    }
}
