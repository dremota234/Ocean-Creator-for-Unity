using UnityEngine;

public interface IChunkProvider
{
    Chunk GetChunk(Vector2Int coord);
    void ReturnChunk(Vector2Int coord);
    bool IsChunkActive(Vector2Int coord);
    void Cleanup();
}
