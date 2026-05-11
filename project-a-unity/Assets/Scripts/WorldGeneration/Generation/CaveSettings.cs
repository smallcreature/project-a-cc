using System;
using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    [Serializable]
    public sealed class CaveSettings
    {
        [SerializeField] private bool enabled = true;
        [SerializeField] private NoiseSettings cheeseNoise = NoiseSettings.Create(1f, 80f, 3, 401);
        [SerializeField] private NoiseSettings spaghettiNoise = NoiseSettings.Create(1f, 42f, 4, 503);
        [SerializeField, Range(-1f, 1f)] private float cheeseThreshold = 0.45f;
        [SerializeField, Range(-1f, 1f)] private float spaghettiCenter = 0f;
        [SerializeField, Range(0.01f, 1f)] private float spaghettiWidth = 0.08f;
        [SerializeField] private int minY = 8;
        [SerializeField] private int maxY = 220;

        public bool Enabled { get => enabled; set => enabled = value; }
        public NoiseSettings CheeseNoise => cheeseNoise;
        public NoiseSettings SpaghettiNoise => spaghettiNoise;
        public float CheeseThreshold { get => cheeseThreshold; set => cheeseThreshold = Mathf.Clamp(value, -1f, 1f); }
        public float SpaghettiCenter { get => spaghettiCenter; set => spaghettiCenter = Mathf.Clamp(value, -1f, 1f); }
        public float SpaghettiWidth { get => spaghettiWidth; set => spaghettiWidth = Mathf.Clamp(value, 0.01f, 1f); }
        public int MinY { get => minY; set => minY = Mathf.Clamp(value, 0, WorldDimensions.WorldHeight - 1); }
        public int MaxY { get => maxY; set => maxY = Mathf.Clamp(value, 0, WorldDimensions.WorldHeight - 1); }

        public CaveSettings Clone()
        {
            return new CaveSettings
            {
                enabled = enabled,
                cheeseNoise = cheeseNoise.Clone(),
                spaghettiNoise = spaghettiNoise.Clone(),
                cheeseThreshold = cheeseThreshold,
                spaghettiCenter = spaghettiCenter,
                spaghettiWidth = spaghettiWidth,
                minY = minY,
                maxY = maxY
            };
        }
    }
}
