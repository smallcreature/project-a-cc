using System;
using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    [Serializable]
    public sealed class NoiseSettings
    {
        [SerializeField] private bool enabled = true;
        [SerializeField] private float intensity = 1f;
        [SerializeField] private float contrast = 1f;
        [SerializeField] private float size = 128f;
        [SerializeField, Range(1, 8)] private int octaves = 3;
        [SerializeField] private float persistence = 0.5f;
        [SerializeField] private float lacunarity = 2f;
        [SerializeField] private int seedOffset;

        public bool Enabled { get => enabled; set => enabled = value; }
        public float Intensity { get => intensity; set => intensity = value; }
        public float Contrast { get => contrast; set => contrast = Mathf.Max(0.01f, value); }
        public float Size { get => size; set => size = Mathf.Max(1f, value); }
        public int Octaves { get => Mathf.Clamp(octaves, 1, 8); set => octaves = Mathf.Clamp(value, 1, 8); }
        public float Persistence { get => persistence; set => persistence = Mathf.Clamp01(value); }
        public float Lacunarity { get => lacunarity; set => lacunarity = Mathf.Max(1f, value); }
        public int SeedOffset { get => seedOffset; set => seedOffset = value; }

        public NoiseSettings Clone()
        {
            return new NoiseSettings
            {
                enabled = enabled,
                intensity = intensity,
                contrast = contrast,
                size = size,
                octaves = octaves,
                persistence = persistence,
                lacunarity = lacunarity,
                seedOffset = seedOffset
            };
        }

        public static NoiseSettings Create(float intensity, float size, int octaves, int seedOffset = 0)
        {
            return new NoiseSettings
            {
                intensity = intensity,
                size = size,
                octaves = octaves,
                seedOffset = seedOffset
            };
        }
    }
}
