using UnityEngine;

[CreateAssetMenu(menuName = "ChunkSystem/Settings")]
public class ChunkSystemSettings : ScriptableObject
{
    [Header("Chunk Configuration")]
    public int chunksX = 3;
    public int chunksZ = 3;
    public int chunkSize = 10;
    public float chunkWorldSize = 10f;

    [Header("LOD Settings")]
    public int[] lodLevels = new[] { 64, 32, 16 };
    public float[] lodDistances = new[] { 50f, 100f, 200f };

    [Header("Performance")]
    public int maxChunksPerFrame = 2;
    public float chunkUpdateInterval = 0.1f;
}
