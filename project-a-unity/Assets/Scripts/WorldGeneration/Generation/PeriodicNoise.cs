using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    public static class PeriodicNoise
    {
        public static float Sample2D(float worldX, float worldZ, NoiseSettings settings, WorldDimensions dimensions)
        {
            return Sample2D(settings, 0, worldX, worldZ, dimensions);
        }

        public static float Sample2D(NoiseSettings settings, float worldX, float worldZ, WorldDimensions dimensions)
        {
            return Sample2D(settings, 0, worldX, worldZ, dimensions);
        }

        public static float Sample2D(NoiseSettings settings, WorldDimensions dimensions, float worldX, float worldZ)
        {
            return Sample2D(settings, 0, worldX, worldZ, dimensions);
        }

        public static float Sample2D(NoiseSettings settings, int seed, float worldX, float worldZ, WorldDimensions dimensions)
        {
            if (settings == null || !settings.Enabled || settings.Intensity == 0f)
            {
                return 0f;
            }

            float amplitude = 1f;
            float amplitudeSum = 0f;
            float valueSum = 0f;
            float size = Mathf.Max(1f, settings.Size);

            for (int octave = 0; octave < settings.Octaves; octave++)
            {
                int cellsX = Mathf.Max(1, Mathf.RoundToInt(dimensions.WorldSizeX / size));
                int cellsZ = Mathf.Max(1, Mathf.RoundToInt(dimensions.WorldSizeZ / size));
                float wrappedX = PositiveModulo(worldX, dimensions.WorldSizeX);
                float wrappedZ = PositiveModulo(worldZ, dimensions.WorldSizeZ);
                float u = wrappedX / dimensions.WorldSizeX * cellsX;
                float v = wrappedZ / dimensions.WorldSizeZ * cellsZ;
                valueSum += Value2D(u, v, cellsX, cellsZ, seed + settings.SeedOffset + octave * 92821) * amplitude;
                amplitudeSum += amplitude;
                amplitude *= settings.Persistence;
                size /= settings.Lacunarity;
            }

            float normalized = amplitudeSum <= 0f ? 0f : valueSum / amplitudeSum;
            return ApplyContrast(normalized, settings.Contrast) * settings.Intensity;
        }

        public static float Sample3D(NoiseSettings settings, int seed, float worldX, float worldY, float worldZ, WorldDimensions dimensions)
        {
            if (settings == null || !settings.Enabled || settings.Intensity == 0f)
            {
                return 0f;
            }

            float amplitude = 1f;
            float amplitudeSum = 0f;
            float valueSum = 0f;
            float size = Mathf.Max(1f, settings.Size);

            for (int octave = 0; octave < settings.Octaves; octave++)
            {
                int cellsX = Mathf.Max(1, Mathf.RoundToInt(dimensions.WorldSizeX / size));
                int cellsZ = Mathf.Max(1, Mathf.RoundToInt(dimensions.WorldSizeZ / size));
                float wrappedX = PositiveModulo(worldX, dimensions.WorldSizeX);
                float wrappedZ = PositiveModulo(worldZ, dimensions.WorldSizeZ);
                float u = wrappedX / dimensions.WorldSizeX * cellsX;
                float v = worldY / size;
                float w = wrappedZ / dimensions.WorldSizeZ * cellsZ;
                valueSum += Value3D(u, v, w, cellsX, cellsZ, seed + settings.SeedOffset + octave * 92821) * amplitude;
                amplitudeSum += amplitude;
                amplitude *= settings.Persistence;
                size /= settings.Lacunarity;
            }

            float normalized = amplitudeSum <= 0f ? 0f : valueSum / amplitudeSum;
            return ApplyContrast(normalized, settings.Contrast) * settings.Intensity;
        }

        private static float Value2D(float u, float v, int periodX, int periodZ, int seed)
        {
            int x0 = Mathf.FloorToInt(u);
            int z0 = Mathf.FloorToInt(v);
            int x1 = x0 + 1;
            int z1 = z0 + 1;
            float tx = Smooth(u - x0);
            float tz = Smooth(v - z0);

            float a = HashToUnit(TorusMath.PositiveModulo(x0, periodX), 0, TorusMath.PositiveModulo(z0, periodZ), seed);
            float b = HashToUnit(TorusMath.PositiveModulo(x1, periodX), 0, TorusMath.PositiveModulo(z0, periodZ), seed);
            float c = HashToUnit(TorusMath.PositiveModulo(x0, periodX), 0, TorusMath.PositiveModulo(z1, periodZ), seed);
            float d = HashToUnit(TorusMath.PositiveModulo(x1, periodX), 0, TorusMath.PositiveModulo(z1, periodZ), seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), tz);
        }

        private static float Value3D(float u, float v, float w, int periodX, int periodZ, int seed)
        {
            int x0 = Mathf.FloorToInt(u);
            int y0 = Mathf.FloorToInt(v);
            int z0 = Mathf.FloorToInt(w);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            int z1 = z0 + 1;
            float tx = Smooth(u - x0);
            float ty = Smooth(v - y0);
            float tz = Smooth(w - z0);

            float c000 = HashToUnit(TorusMath.PositiveModulo(x0, periodX), y0, TorusMath.PositiveModulo(z0, periodZ), seed);
            float c100 = HashToUnit(TorusMath.PositiveModulo(x1, periodX), y0, TorusMath.PositiveModulo(z0, periodZ), seed);
            float c010 = HashToUnit(TorusMath.PositiveModulo(x0, periodX), y1, TorusMath.PositiveModulo(z0, periodZ), seed);
            float c110 = HashToUnit(TorusMath.PositiveModulo(x1, periodX), y1, TorusMath.PositiveModulo(z0, periodZ), seed);
            float c001 = HashToUnit(TorusMath.PositiveModulo(x0, periodX), y0, TorusMath.PositiveModulo(z1, periodZ), seed);
            float c101 = HashToUnit(TorusMath.PositiveModulo(x1, periodX), y0, TorusMath.PositiveModulo(z1, periodZ), seed);
            float c011 = HashToUnit(TorusMath.PositiveModulo(x0, periodX), y1, TorusMath.PositiveModulo(z1, periodZ), seed);
            float c111 = HashToUnit(TorusMath.PositiveModulo(x1, periodX), y1, TorusMath.PositiveModulo(z1, periodZ), seed);

            float x00 = Mathf.Lerp(c000, c100, tx);
            float x10 = Mathf.Lerp(c010, c110, tx);
            float x01 = Mathf.Lerp(c001, c101, tx);
            float x11 = Mathf.Lerp(c011, c111, tx);
            float y0Value = Mathf.Lerp(x00, x10, ty);
            float y1Value = Mathf.Lerp(x01, x11, ty);
            return Mathf.Lerp(y0Value, y1Value, tz);
        }

        private static float Smooth(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }

        private static float ApplyContrast(float value, float contrast)
        {
            float clamped = Mathf.Clamp(value, -1f, 1f);
            return Mathf.Sign(clamped) * Mathf.Pow(Mathf.Abs(clamped), Mathf.Max(0.01f, contrast));
        }

        private static float PositiveModulo(float value, float modulo)
        {
            if (modulo <= 0f)
            {
                return 0f;
            }

            float result = value % modulo;
            return result < 0f ? result + modulo : result;
        }

        private static float HashToUnit(int x, int y, int z, int seed)
        {
            uint hash = (uint)seed;
            hash ^= (uint)x * 0x9E3779B9u;
            hash ^= (uint)y * 0x85EBCA6Bu;
            hash ^= (uint)z * 0xC2B2AE35u;
            hash ^= hash >> 16;
            hash *= 0x7FEB352Du;
            hash ^= hash >> 15;
            hash *= 0x846CA68Bu;
            hash ^= hash >> 16;
            return (hash / (float)uint.MaxValue) * 2f - 1f;
        }
    }
}
