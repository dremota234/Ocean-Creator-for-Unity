using UnityEngine;

[System.Serializable]
public struct ChunkData
{
    public Vector2Int Coord;
    public Bounds WorldBounds;
    public int LODLevel;
    public bool IsVisible;
    public float DistanceToPlayer;
}
