using UnityEngine;

public interface IChunkGenerator
{
    Chunk GenerateChunk(Vector2Int coord, ChunkSystemSettings settings);
    void UpdateChunkGeometry(Chunk chunk, WaveSettings waveSettings);
}
