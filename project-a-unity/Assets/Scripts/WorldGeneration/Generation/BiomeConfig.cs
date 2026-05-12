using System;
using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    [Serializable]
    public sealed class BiomeConfig
    {
        [SerializeField] private string name = "Default";
        [SerializeField] private int biomeId;
        [SerializeField] private Color32 debugColor = new Color32(80, 170, 90, 255);
        [SerializeField] private float mapWeight = 1f;
        [SerializeField, Range(0f, 1f)] private float mapThreshold = 0.55f;
        [SerializeField, Range(0.001f, 0.5f)] private float mapBlend = 0.12f;
        [SerializeField] private NoiseSettings mapNoise = NoiseSettings.Create(1f, 420f, 3, 11);
        [SerializeField] private float baseHeight = 78f;
        [SerializeField] private int stoneBlockId = BlockRegistry.DefaultStoneId;
        [SerializeField] private int soilBlockId = BlockRegistry.DefaultSoilId;
        [SerializeField] private int grassBlockId = BlockRegistry.DefaultGrassId;
        [SerializeField] private int sandBlockId = BlockRegistry.DefaultSandId;
        [SerializeField] private int sandHeight = 64;
        [SerializeField] private Vector2Int soilDepthRange = new Vector2Int(3, 5);
        [SerializeField] private float steepnessThreshold = 10f;
        [SerializeField] private NoiseSettings heightNoise = NoiseSettings.Create(22f, 180f, 4, 101);
        [SerializeField] private NoiseSettings continentalnessNoise = NoiseSettings.Create(1f, 620f, 4, 151);
        [SerializeField] private SplineSettings continentalnessSpline = SplineSettings.Create(
            new SplinePoint(-1f, -42f),
            new SplinePoint(-0.15f, -12f),
            new SplinePoint(0.35f, 22f),
            new SplinePoint(1f, 72f));
        [SerializeField] private NoiseSettings erosionNoise = NoiseSettings.Create(1f, 300f, 3, 211);
        [SerializeField] private SplineSettings erosionSpline = SplineSettings.Create(
            new SplinePoint(-1f, 16f),
            new SplinePoint(0f, 0f),
            new SplinePoint(1f, -24f));
        [SerializeField] private NoiseSettings peaksAndValleysNoise = NoiseSettings.Create(18f, 240f, 4, 271);
        [SerializeField] private NoiseSettings cliffsAndOverhangsNoise = NoiseSettings.Create(18f, 90f, 3, 331);
        [SerializeField] private CaveSettings caves = new CaveSettings();

        public string Name { get => name; set => name = value; }
        public int BiomeId { get => biomeId; set => biomeId = value; }
        public Color32 DebugColor { get => debugColor; set => debugColor = value; }
        public float MapWeight { get => mapWeight; set => mapWeight = Mathf.Max(0f, value); }
        public float MapThreshold { get => mapThreshold; set => mapThreshold = Mathf.Clamp01(value); }
        public float MapBlend { get => mapBlend; set => mapBlend = Mathf.Clamp(value, 0.001f, 0.5f); }
        public NoiseSettings MapNoise => mapNoise;
        public float BaseHeight { get => baseHeight; set => baseHeight = Mathf.Clamp(value, 0f, WorldDimensions.WorldHeight - 1); }
        public int StoneBlockId { get => stoneBlockId; set => stoneBlockId = value; }
        public int SoilBlockId { get => soilBlockId; set => soilBlockId = value; }
        public int GrassBlockId { get => grassBlockId; set => grassBlockId = value; }
        public int SandBlockId { get => sandBlockId == BlockRegistry.AirId ? BlockRegistry.DefaultSandId : sandBlockId; set => sandBlockId = value; }
        public int SandHeight { get => sandHeight; set => sandHeight = Mathf.Clamp(value, 0, WorldDimensions.WorldHeight - 1); }
        public Vector2Int SoilDepthRange { get => soilDepthRange; set => soilDepthRange = value; }
        public float SteepnessThreshold { get => steepnessThreshold; set => steepnessThreshold = Mathf.Max(0f, value); }
        public NoiseSettings HeightNoise => heightNoise;
        public NoiseSettings ContinentalnessNoise => continentalnessNoise;
        public SplineSettings ContinentalnessSpline => continentalnessSpline;
        public NoiseSettings ErosionNoise => erosionNoise;
        public SplineSettings ErosionSpline => erosionSpline;
        public NoiseSettings PeaksAndValleysNoise => peaksAndValleysNoise;
        public NoiseSettings CliffsAndOverhangsNoise => cliffsAndOverhangsNoise;
        public CaveSettings Caves => caves;

        public void Validate()
        {
            baseHeight = Mathf.Clamp(baseHeight, 0f, WorldDimensions.WorldHeight - 1);
            mapWeight = Mathf.Max(0f, mapWeight);
            mapThreshold = Mathf.Clamp01(mapThreshold);
            mapBlend = Mathf.Clamp(mapBlend, 0.001f, 0.5f);
            sandBlockId = SandBlockId;
            sandHeight = Mathf.Clamp(sandHeight, 0, WorldDimensions.WorldHeight - 1);
            steepnessThreshold = Mathf.Max(0f, steepnessThreshold);
        }

        public BiomeConfig Clone()
        {
            return new BiomeConfig
            {
                name = name,
                biomeId = biomeId,
                debugColor = debugColor,
                mapWeight = mapWeight,
                mapThreshold = mapThreshold,
                mapBlend = mapBlend,
                mapNoise = mapNoise.Clone(),
                baseHeight = baseHeight,
                stoneBlockId = stoneBlockId,
                soilBlockId = soilBlockId,
                grassBlockId = grassBlockId,
                sandBlockId = SandBlockId,
                sandHeight = sandHeight,
                soilDepthRange = soilDepthRange,
                steepnessThreshold = steepnessThreshold,
                heightNoise = heightNoise.Clone(),
                continentalnessNoise = continentalnessNoise.Clone(),
                continentalnessSpline = continentalnessSpline.Clone(),
                erosionNoise = erosionNoise.Clone(),
                erosionSpline = erosionSpline.Clone(),
                peaksAndValleysNoise = peaksAndValleysNoise.Clone(),
                cliffsAndOverhangsNoise = cliffsAndOverhangsNoise.Clone(),
                caves = caves.Clone()
            };
        }

        public static BiomeConfig CreateDefault()
        {
            return new BiomeConfig();
        }

        public static BiomeConfig CreateFromDefault(BiomeConfig defaultBiome, int id, string biomeName)
        {
            BiomeConfig biome = defaultBiome == null ? CreateDefault() : defaultBiome.Clone();
            biome.biomeId = id;
            biome.name = biomeName;
            biome.mapWeight = Mathf.Max(0.1f, biome.mapWeight);
            return biome;
        }
    }
}
