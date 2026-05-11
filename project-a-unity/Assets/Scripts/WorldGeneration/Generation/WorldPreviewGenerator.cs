using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    public static class WorldPreviewGenerator
    {
        public static Texture2D CreateBiomeMapPreview(WorldGeneratorConfig config, int size)
        {
            Texture2D texture = CreateTexture(size);
            if (config == null)
            {
                return texture;
            }

            WorldGenerator generator = new WorldGenerator(config, BlockRegistry.CreateDefaultForTests());
            WorldDimensions dimensions = config.Dimensions;
            Color32[] pixels = new Color32[size * size];
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    int worldX = Mathf.FloorToInt(x / (float)size * dimensions.WorldSizeX);
                    int worldZ = Mathf.FloorToInt(z / (float)size * dimensions.WorldSizeZ);
                    pixels[x + z * size] = generator.SampleBiomeColor(worldX, worldZ);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        public static Texture2D CreateNoisePreview(NoiseSettings settings, WorldGeneratorConfig config, int size, int seedOffset)
        {
            Texture2D texture = CreateTexture(size);
            if (settings == null || config == null)
            {
                return texture;
            }

            WorldDimensions dimensions = config.Dimensions;
            Color32[] pixels = new Color32[size * size];
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    int worldX = Mathf.FloorToInt(x / (float)size * dimensions.WorldSizeX);
                    int worldZ = Mathf.FloorToInt(z / (float)size * dimensions.WorldSizeZ);
                    float value = PeriodicNoise.Sample2D(settings, config.Seed + seedOffset, worldX, worldZ, dimensions);
                    pixels[x + z * size] = ToHeat(value);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        public static Texture2D CreateBiomeCombinedPreview(WorldGeneratorConfig config, int biomeIndex, int size)
        {
            Texture2D texture = CreateTexture(size);
            if (config == null)
            {
                return texture;
            }

            WorldGenerator generator = new WorldGenerator(config, BlockRegistry.CreateDefaultForTests());
            WorldDimensions dimensions = config.Dimensions;
            Color32[] pixels = new Color32[size * size];
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    int worldX = Mathf.FloorToInt(x / (float)size * dimensions.WorldSizeX);
                    int worldZ = Mathf.FloorToInt(z / (float)size * dimensions.WorldSizeZ);
                    ColumnSample column = generator.SampleColumn(worldX, worldZ, includeSteepness: false);
                    float normalizedHeight = Mathf.InverseLerp(0f, WorldDimensions.WorldHeight, column.SurfaceHeight);
                    pixels[x + z * size] = Color32.Lerp(new Color32(20, 45, 90, 255), new Color32(235, 235, 210, 255), normalizedHeight);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        public static Texture2D CreateCavePreview(WorldGeneratorConfig config, int size, int y)
        {
            Texture2D texture = CreateTexture(size);
            if (config == null)
            {
                return texture;
            }

            WorldGenerator generator = new WorldGenerator(config, BlockRegistry.CreateDefaultForTests());
            WorldDimensions dimensions = config.Dimensions;
            Color32[] pixels = new Color32[size * size];
            y = Mathf.Clamp(y, 0, WorldDimensions.WorldHeight - 1);
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    int worldX = Mathf.FloorToInt(x / (float)size * dimensions.WorldSizeX);
                    int worldZ = Mathf.FloorToInt(z / (float)size * dimensions.WorldSizeZ);
                    float[] weights = generator.SampleBiomeWeights(worldX, worldZ);
                    float cave = generator.SampleCaveCarveAmount(worldX, y, worldZ, weights);
                    pixels[x + z * size] = Color32.Lerp(new Color32(18, 18, 24, 255), new Color32(230, 225, 190, 255), cave);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static Texture2D CreateTexture(int size)
        {
            size = Mathf.Clamp(size, 16, 1024);
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "World Generator Preview",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            return texture;
        }

        private static Color32 ToHeat(float value)
        {
            float t = Mathf.InverseLerp(-1f, 1f, value);
            Color low = new Color(0.08f, 0.12f, 0.28f, 1f);
            Color mid = new Color(0.85f, 0.85f, 0.72f, 1f);
            Color high = new Color(0.75f, 0.18f, 0.1f, 1f);
            return t < 0.5f
                ? Color.Lerp(low, mid, t * 2f)
                : Color.Lerp(mid, high, (t - 0.5f) * 2f);
        }
    }
}
