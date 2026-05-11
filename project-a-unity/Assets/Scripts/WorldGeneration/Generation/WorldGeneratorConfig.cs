using System.Collections.Generic;
using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    [CreateAssetMenu(menuName = "Project Arcane/World Generation/World Generator Config", fileName = "WorldGeneratorConfig")]
    public sealed class WorldGeneratorConfig : ScriptableObject
    {
        [SerializeField] private int seed = 12345;
        [SerializeField] private int chunkCountX = 64;
        [SerializeField] private int chunkCountZ = 64;
        [SerializeField] private TerrainStageToggles stageToggles = new TerrainStageToggles();
        [SerializeField] private List<BiomeConfig> biomes = new List<BiomeConfig> { BiomeConfig.CreateDefault() };

        public int Seed { get => seed; set => seed = value; }
        public int ChunkCountX { get => chunkCountX; set => chunkCountX = Mathf.Clamp(value, 1, 256); }
        public int ChunkCountZ { get => chunkCountZ; set => chunkCountZ = Mathf.Clamp(value, 1, 256); }
        public TerrainStageToggles StageToggles => stageToggles;
        public List<BiomeConfig> Biomes => biomes;
        public WorldDimensions Dimensions => new WorldDimensions(ChunkCountX, ChunkCountZ);
        public BiomeConfig DefaultBiome => biomes != null && biomes.Count > 0 ? biomes[0] : null;
        public bool Continentalness { get => stageToggles.Continentalness; set => stageToggles.Continentalness = value; }
        public bool Erosion { get => stageToggles.Erosion; set => stageToggles.Erosion = value; }
        public bool PeaksAndValleys { get => stageToggles.PeaksAndValleys; set => stageToggles.PeaksAndValleys = value; }
        public bool CliffsAndOverhangs { get => stageToggles.CliffsAndOverhangs; set => stageToggles.CliffsAndOverhangs = value; }
        public bool Caves { get => stageToggles.Caves; set => stageToggles.Caves = value; }

        private void OnValidate()
        {
            chunkCountX = Mathf.Clamp(chunkCountX, 1, 256);
            chunkCountZ = Mathf.Clamp(chunkCountZ, 1, 256);
            if (stageToggles == null)
            {
                stageToggles = new TerrainStageToggles();
            }

            if (biomes == null)
            {
                biomes = new List<BiomeConfig>();
            }

            if (biomes.Count == 0)
            {
                biomes.Add(BiomeConfig.CreateDefault());
            }

            for (int i = 0; i < biomes.Count; i++)
            {
                if (biomes[i] == null)
                {
                    biomes[i] = i == 0 ? BiomeConfig.CreateDefault() : BiomeConfig.CreateFromDefault(biomes[0], i, $"Biome {i}");
                }

                biomes[i].BiomeId = i;
            }
        }

        public BiomeConfig AddBiomeFromDefault(string biomeName = null)
        {
            if (biomes == null || biomes.Count == 0)
            {
                biomes = new List<BiomeConfig> { BiomeConfig.CreateDefault() };
            }

            int id = biomes.Count;
            BiomeConfig biome = BiomeConfig.CreateFromDefault(biomes[0], id, string.IsNullOrWhiteSpace(biomeName) ? $"Biome {id}" : biomeName);
            biomes.Add(biome);
            return biome;
        }

        public int ComputeStableHash()
        {
            unchecked
            {
                int hash = seed;
                hash = (hash * 397) ^ ChunkCountX;
                hash = (hash * 397) ^ ChunkCountZ;
                hash = (hash * 397) ^ (biomes == null ? 0 : biomes.Count);
                return hash;
            }
        }

        public static WorldGeneratorConfig CreateDefaultForTests()
        {
            WorldGeneratorConfig config = CreateInstance<WorldGeneratorConfig>();
            config.seed = 12345;
            config.chunkCountX = 8;
            config.chunkCountZ = 8;
            config.stageToggles = new TerrainStageToggles();
            config.biomes = new List<BiomeConfig> { BiomeConfig.CreateDefault() };
            return config;
        }
    }
}
