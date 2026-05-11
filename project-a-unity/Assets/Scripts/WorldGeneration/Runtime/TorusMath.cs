using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    public static class TorusMath
    {
        public static int PositiveModulo(int value, int modulo)
        {
            if (modulo <= 0)
            {
                return 0;
            }

            int result = value % modulo;
            return result < 0 ? result + modulo : result;
        }

        public static int FloorDiv(int value, int divisor)
        {
            if (divisor <= 0)
            {
                return 0;
            }

            int quotient = value / divisor;
            int remainder = value % divisor;
            if (remainder != 0 && ((remainder < 0) != (divisor < 0)))
            {
                quotient--;
            }

            return quotient;
        }

        public static int WrapX(int voxelX, WorldDimensions dimensions)
        {
            return PositiveModulo(voxelX, dimensions.WorldSizeX);
        }

        public static int WrapZ(int voxelZ, WorldDimensions dimensions)
        {
            return PositiveModulo(voxelZ, dimensions.WorldSizeZ);
        }

        public static int WrapChunkX(int chunkX, WorldDimensions dimensions)
        {
            return PositiveModulo(chunkX, dimensions.ChunkCountX);
        }

        public static int WrapChunkZ(int chunkZ, WorldDimensions dimensions)
        {
            return PositiveModulo(chunkZ, dimensions.ChunkCountZ);
        }

        public static ChunkCoord WrapChunk(ChunkCoord chunkCoord, WorldDimensions dimensions)
        {
            return new ChunkCoord(WrapChunkX(chunkCoord.X, dimensions), WrapChunkZ(chunkCoord.Z, dimensions));
        }

        public static ChunkCoord WorldToChunk(int voxelX, int voxelZ, WorldDimensions dimensions)
        {
            int wrappedX = WrapX(voxelX, dimensions);
            int wrappedZ = WrapZ(voxelZ, dimensions);
            return new ChunkCoord(wrappedX / WorldDimensions.ChunkSizeX, wrappedZ / WorldDimensions.ChunkSizeZ);
        }

        public static int LocalX(int voxelX, WorldDimensions dimensions)
        {
            return WrapX(voxelX, dimensions) % WorldDimensions.ChunkSizeX;
        }

        public static int LocalZ(int voxelZ, WorldDimensions dimensions)
        {
            return WrapZ(voxelZ, dimensions) % WorldDimensions.ChunkSizeZ;
        }

        public static int ShortestDelta(int from, int to, int period)
        {
            if (period <= 0)
            {
                return to - from;
            }

            int half = period / 2;
            return PositiveModulo(to - from + half, period) - half;
        }

        public static int ShortestDeltaX(int fromX, int toX, WorldDimensions dimensions)
        {
            return ShortestDelta(fromX, toX, dimensions.WorldSizeX);
        }

        public static int ShortestDeltaZ(int fromZ, int toZ, WorldDimensions dimensions)
        {
            return ShortestDelta(fromZ, toZ, dimensions.WorldSizeZ);
        }

        public static float WrappedDistanceXZ(Vector2 from, Vector2 to, WorldDimensions dimensions)
        {
            float dx = ShortestDelta(Mathf.RoundToInt(from.x), Mathf.RoundToInt(to.x), dimensions.WorldSizeX);
            float dz = ShortestDelta(Mathf.RoundToInt(from.y), Mathf.RoundToInt(to.y), dimensions.WorldSizeZ);
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
