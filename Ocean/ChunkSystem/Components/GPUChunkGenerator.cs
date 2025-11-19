using UnityEngine;

public class GPUChunkGenerator : IChunkGenerator
{
    private readonly ChunkSystemSettings _chunkSettings;
    private readonly WaveSettings _waveSettings;

    public GPUChunkGenerator(ChunkSystemSettings chunkSettings, WaveSettings waveSettings)
    {
        _chunkSettings = chunkSettings;
        _waveSettings = waveSettings;
    }

    public Chunk GenerateChunk(Vector2Int coord, ChunkSystemSettings settings)
    {
        var chunk = new Chunk(coord);
        GenerateBaseMesh(chunk);
        ApplyWaveDeformation(chunk); // Добавляем волны при генерации
        return chunk;
    }

    public void UpdateChunkGeometry(Chunk chunk, WaveSettings waveSettings)
    {
        if (chunk.Mesh != null && chunk.Vertices != null)
        {
            ApplyWaveDeformation(chunk); // Обновляем вершины с волнами
            chunk.Mesh.vertices = chunk.Vertices;
            chunk.Mesh.RecalculateNormals();
            chunk.Mesh.RecalculateBounds();
        }
    }

    private void GenerateBaseMesh(Chunk chunk)
    {
        var meshGenerator = new ChunkMeshGenerator(_chunkSettings.chunkSize, _chunkSettings.chunkWorldSize);
        meshGenerator.GenerateMesh(chunk);
    }

    private void ApplyWaveDeformation(Chunk chunk)
    {
        if (chunk.Vertices == null || _waveSettings == null) return;

        float time = Time.time;
        Vector2 chunkWorldPos = new Vector2(
            chunk.Coord.x * _chunkSettings.chunkWorldSize,
            chunk.Coord.y * _chunkSettings.chunkWorldSize
        );

        for (int i = 0; i < chunk.Vertices.Length; i++)
        {
            Vector3 vertex = chunk.Vertices[i];
            Vector2 worldPos = new Vector2(
                chunkWorldPos.x + vertex.x,
                chunkWorldPos.y + vertex.z
            );

            float height = CalculateWaveHeight(worldPos, time);
            chunk.Vertices[i] = new Vector3(vertex.x, height, vertex.z);
        }
    }

    private float CalculateWaveHeight(Vector2 worldPos, float time)
    {
        float height = 0f;

        if (_waveSettings.waveDirections.Length >= 1)
            height += GerstnerWave(worldPos, _waveSettings.waveDirections[0], time);
        if (_waveSettings.waveDirections.Length >= 2)
            height += GerstnerWave(worldPos, _waveSettings.waveDirections[1], time);
        if (_waveSettings.waveDirections.Length >= 3)
            height += GerstnerWave(worldPos, _waveSettings.waveDirections[2], time);

        return height;
    }

    private float GerstnerWave(Vector2 position, Vector4 waveParams, float time)
    {
        Vector2 direction = new Vector2(waveParams.x, waveParams.y);
        float speed = waveParams.z * _waveSettings.waveSpeed;
        float amplitude = waveParams.w * _waveSettings.waveAmplitude;

        float frequency = _waveSettings.waveFrequency;
        float steepness = _waveSettings.waveSteepness;

        float dot = Vector2.Dot(direction, position);
        float phase = speed * frequency;
        float theta = dot * frequency + time * phase;

        return amplitude * Mathf.Sin(theta);
    }
}