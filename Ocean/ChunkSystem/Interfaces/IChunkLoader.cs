using System.Collections.Generic;
using UnityEngine;

public interface IChunkLoader
{
    void LoadChunk(Vector2Int coord);
    void UnloadChunk(Vector2Int coord);
    IEnumerable<Vector2Int> GetChunksToLoad(Vector3 playerPosition);
    IEnumerable<Vector2Int> GetChunksToUnload(Vector3 playerPosition); // Добавляем этот метод
}