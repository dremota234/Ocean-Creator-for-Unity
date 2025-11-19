using System;
using UnityEngine;

public interface IChunkSystem
{
    void Initialize();
    void UpdateSystem(Vector3 playerPosition);
    void Cleanup();
    void SetWaveParameters(WaveSettings parameters);
    event Action<Chunk> OnChunkCreated;
    event Action<Vector2Int> OnChunkRemoved;
}

