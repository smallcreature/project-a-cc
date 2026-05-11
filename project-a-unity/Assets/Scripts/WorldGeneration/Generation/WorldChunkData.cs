using System;
using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    public sealed class WorldChunkData
    {
        public WorldChunkData(ChunkCoord coord, WorldDimensions dimensions)
        {
            Coord = coord;
            Dimensions = dimensions;
            Cells = new CellState[WorldDimensions.ChunkSizeX * WorldDimensions.WorldHeight * WorldDimensions.ChunkSizeZ];
            SurfaceHeights = new float[WorldDimensions.ChunkSizeX * WorldDimensions.ChunkSizeZ];
            DominantBiomeIds = new int[WorldDimensions.ChunkSizeX * WorldDimensions.ChunkSizeZ];
        }

        public ChunkCoord Coord { get; }
        public WorldDimensions Dimensions { get; }
        public CellState[] Cells { get; }
        public float[] SurfaceHeights { get; }
        public int[] DominantBiomeIds { get; }

        public CellState GetCellLocal(int localX, int y, int localZ)
        {
            if (!IsInside(localX, y, localZ))
            {
                return CellState.Air;
            }

            return Cells[ToIndex(localX, y, localZ)];
        }

        public CellState GetCell(int localX, int y, int localZ)
        {
            return GetCellLocal(localX, y, localZ);
        }

        public void SetCellLocal(int localX, int y, int localZ, CellState cell)
        {
            if (!IsInside(localX, y, localZ))
            {
                return;
            }

            Cells[ToIndex(localX, y, localZ)] = cell;
        }

        public void SetCell(int localX, int y, int localZ, CellState cell)
        {
            SetCellLocal(localX, y, localZ, cell);
        }

        public static WorldChunkData CreateForTests(ChunkCoord coord)
        {
            return new WorldChunkData(coord, new WorldDimensions(1, 1));
        }

        public void SetColumnInfo(int localX, int localZ, float surfaceHeight, int dominantBiomeId)
        {
            int index = ToColumnIndex(localX, localZ);
            SurfaceHeights[index] = surfaceHeight;
            DominantBiomeIds[index] = dominantBiomeId;
        }

        public float GetSurfaceHeightLocal(int localX, int localZ)
        {
            return SurfaceHeights[ToColumnIndex(localX, localZ)];
        }

        public int GetDominantBiomeIdLocal(int localX, int localZ)
        {
            return DominantBiomeIds[ToColumnIndex(localX, localZ)];
        }

        public static bool IsInside(int localX, int y, int localZ)
        {
            return localX >= 0 && localX < WorldDimensions.ChunkSizeX &&
                   y >= 0 && y < WorldDimensions.WorldHeight &&
                   localZ >= 0 && localZ < WorldDimensions.ChunkSizeZ;
        }

        public static int ToIndex(int localX, int y, int localZ)
        {
            return y + WorldDimensions.WorldHeight * (localX + WorldDimensions.ChunkSizeX * localZ);
        }

        public static int ToColumnIndex(int localX, int localZ)
        {
            return localX + WorldDimensions.ChunkSizeX * localZ;
        }
    }

    public readonly struct ColumnSample
    {
        public readonly float BaseHeight;
        public readonly float SurfaceHeight;
        public readonly float Steepness;
        public readonly int DominantBiomeIndex;
        public readonly float[] Weights;

        public ColumnSample(float baseHeight, float surfaceHeight, float steepness, int dominantBiomeIndex, float[] weights)
        {
            BaseHeight = baseHeight;
            SurfaceHeight = surfaceHeight;
            Steepness = steepness;
            DominantBiomeIndex = dominantBiomeIndex;
            Weights = weights;
        }
    }

    public interface IWorldBlockSampler
    {
        CellState GetCell(int worldX, int y, int worldZ);
    }
}
