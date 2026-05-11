using System.Collections.Generic;
using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    public sealed class WorldGenerator
    {
        private readonly WorldGeneratorConfig config;
        private readonly BlockRegistry blockRegistry;
        private readonly WorldDimensions dimensions;

        public WorldGenerator(WorldGeneratorConfig config, BlockRegistry blockRegistry)
        {
            this.config = config;
            this.blockRegistry = blockRegistry;
            dimensions = config == null ? new WorldDimensions(1, 1) : config.Dimensions;
        }

        public WorldDimensions Dimensions => dimensions;

        public WorldChunkData GenerateChunk(ChunkCoord requestedCoord)
        {
            ChunkCoord chunkCoord = TorusMath.WrapChunk(requestedCoord, dimensions);
            WorldChunkData chunk = new WorldChunkData(chunkCoord, dimensions);
            if (config == null || config.Biomes == null || config.Biomes.Count == 0)
            {
                return chunk;
            }

            ColumnSample[,] columns = new ColumnSample[WorldDimensions.ChunkSizeX, WorldDimensions.ChunkSizeZ];
            int worldOriginX = chunkCoord.X * WorldDimensions.ChunkSizeX;
            int worldOriginZ = chunkCoord.Z * WorldDimensions.ChunkSizeZ;

            for (int localZ = 0; localZ < WorldDimensions.ChunkSizeZ; localZ++)
            {
                for (int localX = 0; localX < WorldDimensions.ChunkSizeX; localX++)
                {
                    int worldX = worldOriginX + localX;
                    int worldZ = worldOriginZ + localZ;
                    columns[localX, localZ] = SampleColumn(worldX, worldZ, includeSteepness: true);
                    chunk.SetColumnInfo(localX, localZ, columns[localX, localZ].SurfaceHeight, GetBiome(columns[localX, localZ]).BiomeId);
                }
            }

            for (int localZ = 0; localZ < WorldDimensions.ChunkSizeZ; localZ++)
            {
                for (int localX = 0; localX < WorldDimensions.ChunkSizeX; localX++)
                {
                    int worldX = worldOriginX + localX;
                    int worldZ = worldOriginZ + localZ;
                    ColumnSample column = columns[localX, localZ];
                    BiomeConfig biome = GetBiome(column);
                    for (int y = 0; y < WorldDimensions.WorldHeight; y++)
                    {
                        float density = column.SurfaceHeight - y;
                        if (config.StageToggles.CliffsAndOverhangs)
                        {
                            density += SampleWeighted3DNoise(worldX, y, worldZ, column.Weights, b => b.CliffsAndOverhangsNoise);
                        }

                        if (density >= 0f)
                        {
                            chunk.SetCellLocal(localX, y, localZ, new CellState(biome.StoneBlockId, BlockLayer.Terrain));
                        }
                    }
                }
            }

            if (config.StageToggles.Caves)
            {
                ApplyCaves(chunk, columns);
            }

            ApplySurfaceReplacement(chunk, columns);
            return chunk;
        }

        public CellState SampleCell(int worldX, int y, int worldZ)
        {
            if (y < 0 || y >= WorldDimensions.WorldHeight)
            {
                return CellState.Air;
            }

            ColumnSample column = SampleColumn(worldX, worldZ, includeSteepness: false);
            BiomeConfig biome = GetBiome(column);
            float density = column.SurfaceHeight - y;
            if (config != null && config.StageToggles.CliffsAndOverhangs)
            {
                density += SampleWeighted3DNoise(worldX, y, worldZ, column.Weights, b => b.CliffsAndOverhangsNoise);
            }

            if (density < 0f || (config != null && config.StageToggles.Caves && SampleCaveCarveAmount(worldX, y, worldZ, column.Weights) > 0.5f))
            {
                return CellState.Air;
            }

            return new CellState(biome.StoneBlockId, BlockLayer.Terrain);
        }

        public ColumnSample SampleColumn(int worldX, int worldZ, bool includeSteepness)
        {
            float[] weights = SampleBiomeWeights(worldX, worldZ);
            float baseHeight = SampleWeightedValue(weights, b => b.BaseHeight);
            float height = baseHeight;
            height += SampleWeighted2DNoise(worldX, worldZ, weights, b => b.HeightNoise);

            if (config.StageToggles.Continentalness)
            {
                height += SampleWeightedSplineStage(worldX, worldZ, weights, b => b.ContinentalnessNoise, b => b.ContinentalnessSpline);
            }

            if (config.StageToggles.Erosion)
            {
                height += SampleWeightedSplineStage(worldX, worldZ, weights, b => b.ErosionNoise, b => b.ErosionSpline);
            }

            if (config.StageToggles.PeaksAndValleys)
            {
                float peaks = SampleWeighted2DNoise(worldX, worldZ, weights, b => b.PeaksAndValleysNoise);
                height += Mathf.Sign(peaks) * Mathf.Abs(peaks);
            }

            height = Mathf.Clamp(height, 1f, WorldDimensions.WorldHeight - 2);
            int dominantBiome = FindDominantBiome(weights);
            float steepness = includeSteepness ? SampleSteepness(worldX, worldZ, height) : 0f;
            return new ColumnSample(baseHeight, height, steepness, dominantBiome, weights);
        }

        public float[] SampleBiomeWeights(int worldX, int worldZ)
        {
            IReadOnlyList<BiomeConfig> biomes = config.Biomes;
            float[] weights = new float[biomes.Count];
            weights[0] = 1f;

            for (int i = 1; i < biomes.Count; i++)
            {
                BiomeConfig biome = biomes[i];
                float noise = (PeriodicNoise.Sample2D(biome.MapNoise, config.Seed, worldX, worldZ, dimensions) + 1f) * 0.5f;
                float low = biome.MapThreshold - biome.MapBlend;
                float high = biome.MapThreshold + biome.MapBlend;
                float weight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(low, high, noise)) * biome.MapWeight;
                weights[i] = weight;
            }

            Normalize(weights);
            return weights;
        }

        public Color32 SampleBiomeColor(int worldX, int worldZ)
        {
            float[] weights = SampleBiomeWeights(worldX, worldZ);
            float r = 0f;
            float g = 0f;
            float b = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                Color32 color = config.Biomes[i].DebugColor;
                r += color.r * weights[i];
                g += color.g * weights[i];
                b += color.b * weights[i];
            }

            return new Color32((byte)Mathf.Clamp(Mathf.RoundToInt(r), 0, 255), (byte)Mathf.Clamp(Mathf.RoundToInt(g), 0, 255), (byte)Mathf.Clamp(Mathf.RoundToInt(b), 0, 255), 255);
        }

        public float SampleCaveCarveAmount(int worldX, int y, int worldZ, float[] weights)
        {
            if (config == null || !config.StageToggles.Caves || weights == null)
            {
                return 0f;
            }

            float amount = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                float weight = weights[i];
                if (weight <= 0f)
                {
                    continue;
                }

                CaveSettings cave = config.Biomes[i].Caves;
                if (cave == null || !cave.Enabled || y < cave.MinY || y > cave.MaxY)
                {
                    continue;
                }

                float cheese = PeriodicNoise.Sample3D(cave.CheeseNoise, config.Seed, worldX, y, worldZ, dimensions);
                float spaghetti = PeriodicNoise.Sample3D(cave.SpaghettiNoise, config.Seed, worldX, y, worldZ, dimensions);
                float cheeseCarve = Mathf.SmoothStep(cave.CheeseThreshold, 1f, cheese);
                float spaghettiDistance = Mathf.Abs(spaghetti - cave.SpaghettiCenter);
                float spaghettiCarve = 1f - Mathf.SmoothStep(cave.SpaghettiWidth, cave.SpaghettiWidth * 1.75f, spaghettiDistance);
                amount += Mathf.Max(cheeseCarve, spaghettiCarve) * weight;
            }

            return Mathf.Clamp01(amount);
        }

        private void ApplyCaves(WorldChunkData chunk, ColumnSample[,] columns)
        {
            int worldOriginX = chunk.Coord.X * WorldDimensions.ChunkSizeX;
            int worldOriginZ = chunk.Coord.Z * WorldDimensions.ChunkSizeZ;
            for (int localZ = 0; localZ < WorldDimensions.ChunkSizeZ; localZ++)
            {
                for (int localX = 0; localX < WorldDimensions.ChunkSizeX; localX++)
                {
                    int worldX = worldOriginX + localX;
                    int worldZ = worldOriginZ + localZ;
                    ColumnSample column = columns[localX, localZ];
                    for (int y = 0; y < WorldDimensions.WorldHeight; y++)
                    {
                        CellState cell = chunk.GetCellLocal(localX, y, localZ);
                        if (cell.IsAir)
                        {
                            continue;
                        }

                        if (SampleCaveCarveAmount(worldX, y, worldZ, column.Weights) > 0.5f)
                        {
                            chunk.SetCellLocal(localX, y, localZ, CellState.Air);
                        }
                    }
                }
            }
        }

        private void ApplySurfaceReplacement(WorldChunkData chunk, ColumnSample[,] columns)
        {
            for (int localZ = 0; localZ < WorldDimensions.ChunkSizeZ; localZ++)
            {
                for (int localX = 0; localX < WorldDimensions.ChunkSizeX; localX++)
                {
                    ColumnSample column = columns[localX, localZ];
                    BiomeConfig biome = GetBiome(column);
                    if (column.Steepness > biome.SteepnessThreshold)
                    {
                        continue;
                    }

                    int surfaceY = FindSkyExposedSurface(chunk, localX, localZ);
                    if (surfaceY < 0)
                    {
                        continue;
                    }

                    int minDepth = Mathf.Min(biome.SoilDepthRange.x, biome.SoilDepthRange.y);
                    int maxDepth = Mathf.Max(biome.SoilDepthRange.x, biome.SoilDepthRange.y);
                    int depth = Mathf.Clamp(Mathf.RoundToInt(Mathf.Abs(PeriodicNoise.Sample2D(biome.HeightNoise, config.Seed + 701, chunk.Coord.X * 16 + localX, chunk.Coord.Z * 16 + localZ, dimensions))), 0, 1024);
                    depth = minDepth + (maxDepth <= minDepth ? 0 : depth % (maxDepth - minDepth + 1));

                    for (int offset = 0; offset < depth; offset++)
                    {
                        int y = surfaceY - offset;
                        if (y < 0)
                        {
                            break;
                        }

                        CellState existing = chunk.GetCellLocal(localX, y, localZ);
                        if (existing.IsAir)
                        {
                            break;
                        }

                        int blockId = offset == 0 && surfaceY >= Mathf.FloorToInt(column.BaseHeight) ? biome.GrassBlockId : biome.SoilBlockId;
                        chunk.SetCellLocal(localX, y, localZ, new CellState(blockId, BlockLayer.Terrain));
                    }
                }
            }
        }

        private int FindSkyExposedSurface(WorldChunkData chunk, int localX, int localZ)
        {
            for (int y = WorldDimensions.WorldHeight - 2; y >= 0; y--)
            {
                CellState cell = chunk.GetCellLocal(localX, y, localZ);
                if (cell.IsAir)
                {
                    continue;
                }

                if (chunk.GetCellLocal(localX, y + 1, localZ).IsAir)
                {
                    return y;
                }
            }

            return -1;
        }

        private float SampleWeightedValue(float[] weights, System.Func<BiomeConfig, float> selector)
        {
            float value = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                value += selector(config.Biomes[i]) * weights[i];
            }

            return value;
        }

        private float SampleWeighted2DNoise(int worldX, int worldZ, float[] weights, System.Func<BiomeConfig, NoiseSettings> selector)
        {
            float value = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] <= 0f)
                {
                    continue;
                }

                value += PeriodicNoise.Sample2D(selector(config.Biomes[i]), config.Seed, worldX, worldZ, dimensions) * weights[i];
            }

            return value;
        }

        private float SampleWeighted3DNoise(int worldX, int y, int worldZ, float[] weights, System.Func<BiomeConfig, NoiseSettings> selector)
        {
            float value = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] <= 0f)
                {
                    continue;
                }

                value += PeriodicNoise.Sample3D(selector(config.Biomes[i]), config.Seed, worldX, y, worldZ, dimensions) * weights[i];
            }

            return value;
        }

        private float SampleWeightedSplineStage(int worldX, int worldZ, float[] weights, System.Func<BiomeConfig, NoiseSettings> noiseSelector, System.Func<BiomeConfig, SplineSettings> splineSelector)
        {
            float value = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] <= 0f)
                {
                    continue;
                }

                BiomeConfig biome = config.Biomes[i];
                float noise = PeriodicNoise.Sample2D(noiseSelector(biome), config.Seed, worldX, worldZ, dimensions);
                value += splineSelector(biome).Evaluate(noise) * weights[i];
            }

            return value;
        }

        private float SampleSteepness(int worldX, int worldZ, float centerHeight)
        {
            float maxDelta = 0f;
            maxDelta = Mathf.Max(maxDelta, Mathf.Abs(SampleColumn(worldX + 1, worldZ, includeSteepness: false).SurfaceHeight - centerHeight));
            maxDelta = Mathf.Max(maxDelta, Mathf.Abs(SampleColumn(worldX - 1, worldZ, includeSteepness: false).SurfaceHeight - centerHeight));
            maxDelta = Mathf.Max(maxDelta, Mathf.Abs(SampleColumn(worldX, worldZ + 1, includeSteepness: false).SurfaceHeight - centerHeight));
            maxDelta = Mathf.Max(maxDelta, Mathf.Abs(SampleColumn(worldX, worldZ - 1, includeSteepness: false).SurfaceHeight - centerHeight));
            return maxDelta;
        }

        private BiomeConfig GetBiome(ColumnSample column)
        {
            int index = Mathf.Clamp(column.DominantBiomeIndex, 0, config.Biomes.Count - 1);
            return config.Biomes[index];
        }

        private int FindDominantBiome(float[] weights)
        {
            int dominant = 0;
            float best = float.MinValue;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] > best)
                {
                    best = weights[i];
                    dominant = i;
                }
            }

            return dominant;
        }

        private static void Normalize(float[] weights)
        {
            float total = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                total += Mathf.Max(0f, weights[i]);
            }

            if (total <= 0f)
            {
                weights[0] = 1f;
                return;
            }

            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = Mathf.Max(0f, weights[i]) / total;
            }
        }
    }
}
