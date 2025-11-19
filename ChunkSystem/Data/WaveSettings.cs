using UnityEngine;

[CreateAssetMenu(menuName = "ChunkSystem/Wave Settings")]
public class WaveSettings : ScriptableObject
{
    public float waveSpeed = 1.0f;
    public float waveAmplitude = 0.5f;
    public float waveFrequency = 1.0f;
    [Range(0, 1)] public float waveSteepness = 0.5f;
    public Vector4[] waveDirections = new Vector4[]
    {
        new Vector4(1, 0, 0, 0),
        new Vector4(0, 1, 0, 0),
        new Vector4(0.7f, 0.7f, 0, 0)
    };
}
