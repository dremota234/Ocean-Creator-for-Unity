using System;
using UnityEngine;

public static class ChunkEvents
{
    public static event Action<Chunk> ChunkCreated;
    public static event Action<Vector2Int> ChunkDestroyed;
    public static event Action<Vector3> PlayerChunkChanged;

    public static void RaiseChunkCreated(Chunk chunk) => ChunkCreated?.Invoke(chunk);
    public static void RaiseChunkDestroyed(Vector2Int coord) => ChunkDestroyed?.Invoke(coord);
    public static void RaisePlayerChunkChanged(Vector3 position) => PlayerChunkChanged?.Invoke(position);
}
