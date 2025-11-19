using UnityEngine;

public class ChunkMeshGenerator
{
    private readonly int _chunkSize;
    private readonly float _worldSize;

    public ChunkMeshGenerator(int chunkSize, float worldSize)
    {
        _chunkSize = chunkSize;
        _worldSize = worldSize;
    }

    public void GenerateMesh(Chunk chunk)
    {
        InitializeArrays(chunk);
        GenerateVertices(chunk);
        GenerateTriangles(chunk);
        GenerateUVs(chunk);
        CreateAndConfigureMesh(chunk);
    }

    private void InitializeArrays(Chunk chunk)
    {
        int vertexCount = _chunkSize + 1;
        int totalVertices = vertexCount * vertexCount;
        chunk.Vertices = new Vector3[totalVertices];
        chunk.UVs = new Vector2[totalVertices];
        chunk.Triangles = new int[_chunkSize * _chunkSize * 6];
    }

    private void GenerateVertices(Chunk chunk)
    {
        int vertexCount = _chunkSize + 1;
        float step = _worldSize / _chunkSize;

        for (int z = 0, i = 0; z <= _chunkSize; z++)
        {
            for (int x = 0; x <= _chunkSize; x++, i++)
            {
                float localX = x * step;
                float localZ = z * step;
                chunk.Vertices[i] = new Vector3(localX, 0, localZ);
            }
        }
    }

    private void GenerateTriangles(Chunk chunk)
    {
        int vertexCount = _chunkSize + 1;
        int triIndex = 0;

        for (int z = 0; z < _chunkSize; z++)
        {
            for (int x = 0; x < _chunkSize; x++)
            {
                int bottomLeft = z * vertexCount + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = (z + 1) * vertexCount + x;
                int topRight = topLeft + 1;

                // Первый треугольник
                chunk.Triangles[triIndex++] = bottomLeft;
                chunk.Triangles[triIndex++] = topLeft;
                chunk.Triangles[triIndex++] = bottomRight;

                // Второй треугольник
                chunk.Triangles[triIndex++] = bottomRight;
                chunk.Triangles[triIndex++] = topLeft;
                chunk.Triangles[triIndex++] = topRight;
            }
        }
    }

    private void GenerateUVs(Chunk chunk)
    {
        for (int z = 0, i = 0; z <= _chunkSize; z++)
        {
            for (int x = 0; x <= _chunkSize; x++, i++)
            {
                chunk.UVs[i] = new Vector2(
                    x / (float)_chunkSize,
                    z / (float)_chunkSize
                );
            }
        }
    }

    private void CreateAndConfigureMesh(Chunk chunk)
    {
        chunk.Mesh = new Mesh();
        chunk.Mesh.name = $"ChunkMesh_{chunk.Coord.x}_{chunk.Coord.y}";

        chunk.Mesh.vertices = chunk.Vertices;
        chunk.Mesh.triangles = chunk.Triangles;
        chunk.Mesh.uv = chunk.UVs;

        chunk.Mesh.RecalculateNormals();
        chunk.Mesh.RecalculateBounds();

        if (chunk.MeshFilter != null)
        {
            chunk.MeshFilter.mesh = chunk.Mesh;
        }
    }
}