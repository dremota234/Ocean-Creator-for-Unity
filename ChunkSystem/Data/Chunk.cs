using UnityEngine;

[System.Serializable]
public class Chunk
{
    public Vector2Int Coord { get; set; }
    public GameObject ChunkObject { get; set; }
    public MeshFilter MeshFilter { get; set; }
    public MeshRenderer MeshRenderer { get; set; }
    public Mesh Mesh { get; set; }
    public Vector3[] Vertices { get; set; }
    public int[] Triangles { get; set; }
    public Vector2[] UVs { get; set; }
    public bool IsActive { get; set; }

    public Chunk(Vector2Int coord)
    {
        Coord = coord;
        IsActive = true;
    }
}
