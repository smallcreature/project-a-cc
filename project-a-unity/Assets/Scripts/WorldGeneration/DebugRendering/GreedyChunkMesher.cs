using System.Collections.Generic;
using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    public sealed class GreedyMeshData
    {
        public readonly List<Vector3> Vertices = new List<Vector3>();
        public readonly List<int> Triangles = new List<int>();
        public readonly List<Color32> Colors = new List<Color32>();
        public readonly List<Vector3> Normals = new List<Vector3>();
        public int QuadCount;

        public bool IsEmpty => Triangles.Count == 0;
    }

    public sealed class GreedyChunkMesher
    {
        private readonly BlockRegistry registry;

        public GreedyChunkMesher()
        {
        }

        public GreedyChunkMesher(BlockRegistry registry)
        {
            this.registry = registry;
        }

        private struct MaskCell
        {
            public bool Filled;
            public int NormalSign;
            public CellState Cell;

            public bool CanMergeWith(MaskCell other)
            {
                return Filled && other.Filled &&
                       NormalSign == other.NormalSign &&
                       Cell.BlockId == other.Cell.BlockId &&
                       Cell.Layer == other.Cell.Layer &&
                       Cell.Flags == other.Cell.Flags;
            }
        }

        public static GreedyMeshData BuildMeshData(IWorldBlockSampler sampler, ChunkCoord chunkCoord, int sectionYMin, int sectionYMaxExclusive, BlockRegistry registry)
        {
            GreedyMeshData meshData = new GreedyMeshData();
            if (sampler == null)
            {
                return meshData;
            }

            sectionYMin = Mathf.Clamp(sectionYMin, 0, WorldDimensions.WorldHeight);
            sectionYMaxExclusive = Mathf.Clamp(sectionYMaxExclusive, sectionYMin, WorldDimensions.WorldHeight);
            int sectionHeight = sectionYMaxExclusive - sectionYMin;
            if (sectionHeight <= 0)
            {
                return meshData;
            }

            int[] dims = { WorldDimensions.ChunkSizeX, sectionHeight, WorldDimensions.ChunkSizeZ };
            int[] x = new int[3];
            int[] q = new int[3];
            int worldOriginX = chunkCoord.X * WorldDimensions.ChunkSizeX;
            int worldOriginZ = chunkCoord.Z * WorldDimensions.ChunkSizeZ;

            for (int d = 0; d < 3; d++)
            {
                int u = (d + 1) % 3;
                int v = (d + 2) % 3;
                q[0] = 0;
                q[1] = 0;
                q[2] = 0;
                q[d] = 1;
                MaskCell[] mask = new MaskCell[dims[u] * dims[v]];

                for (x[d] = -1; x[d] < dims[d];)
                {
                    int maskIndex = 0;
                    for (x[v] = 0; x[v] < dims[v]; x[v]++)
                    {
                        for (x[u] = 0; x[u] < dims[u]; x[u]++)
                        {
                            CellState a = GetCell(sampler, worldOriginX, worldOriginZ, sectionYMin, x[0], x[1], x[2]);
                            CellState b = GetCell(sampler, worldOriginX, worldOriginZ, sectionYMin, x[0] + q[0], x[1] + q[1], x[2] + q[2]);
                            bool solidA = IsSolid(a, registry);
                            bool solidB = IsSolid(b, registry);

                            if (solidA == solidB)
                            {
                                mask[maskIndex++] = default;
                            }
                            else
                            {
                                mask[maskIndex++] = new MaskCell
                                {
                                    Filled = true,
                                    NormalSign = solidA ? 1 : -1,
                                    Cell = solidA ? a : b
                                };
                            }
                        }
                    }

                    x[d]++;
                    maskIndex = 0;
                    for (int j = 0; j < dims[v]; j++)
                    {
                        for (int i = 0; i < dims[u];)
                        {
                            MaskCell cell = mask[maskIndex];
                            if (!cell.Filled)
                            {
                                i++;
                                maskIndex++;
                                continue;
                            }

                            int width;
                            for (width = 1; i + width < dims[u] && cell.CanMergeWith(mask[maskIndex + width]); width++)
                            {
                            }

                            int height = 1;
                            bool done = false;
                            while (j + height < dims[v] && !done)
                            {
                                for (int k = 0; k < width; k++)
                                {
                                    if (!cell.CanMergeWith(mask[maskIndex + k + height * dims[u]]))
                                    {
                                        done = true;
                                        break;
                                    }
                                }

                                if (!done)
                                {
                                    height++;
                                }
                            }

                            int[] corner = { 0, 0, 0 };
                            int[] du = { 0, 0, 0 };
                            int[] dv = { 0, 0, 0 };
                            corner[d] = x[d];
                            corner[u] = i;
                            corner[v] = j;
                            du[u] = width;
                            dv[v] = height;
                            AddQuad(meshData, corner, du, dv, d, cell.NormalSign, cell.Cell, sectionYMin, registry);

                            for (int y = 0; y < height; y++)
                            {
                                for (int xMask = 0; xMask < width; xMask++)
                                {
                                    mask[maskIndex + xMask + y * dims[u]] = default;
                                }
                            }

                            i += width;
                            maskIndex += width;
                        }
                    }
                }
            }

            return meshData;
        }

        public GreedyMeshData BuildMesh(WorldChunkData chunk, BlockRegistry blockRegistry)
        {
            if (chunk == null)
            {
                return new GreedyMeshData();
            }

            return BuildMeshData(new ChunkOnlySampler(chunk), chunk.Coord, 0, WorldDimensions.WorldHeight, blockRegistry ?? registry);
        }

        public GreedyMeshData BuildMesh(WorldChunkData chunk)
        {
            return BuildMesh(chunk, registry);
        }

        private static CellState GetCell(IWorldBlockSampler sampler, int worldOriginX, int worldOriginZ, int sectionYMin, int localX, int localY, int localZ)
        {
            int y = sectionYMin + localY;
            if (y < 0 || y >= WorldDimensions.WorldHeight)
            {
                return CellState.Air;
            }

            return sampler.GetCell(worldOriginX + localX, y, worldOriginZ + localZ);
        }

        private static bool IsSolid(CellState cell, BlockRegistry registry)
        {
            return registry == null ? !cell.IsAir : registry.IsSolid(cell);
        }

        private static void AddQuad(GreedyMeshData meshData, int[] corner, int[] du, int[] dv, int axis, int normalSign, CellState cell, int sectionYMin, BlockRegistry registry)
        {
            int start = meshData.Vertices.Count;
            Vector3 a = ToVertex(corner, sectionYMin);
            Vector3 b = ToVertex(Add(corner, normalSign > 0 ? dv : du), sectionYMin);
            Vector3 c = ToVertex(Add(Add(corner, du), dv), sectionYMin);
            Vector3 d = ToVertex(Add(corner, normalSign > 0 ? du : dv), sectionYMin);

            meshData.Vertices.Add(a);
            meshData.Vertices.Add(b);
            meshData.Vertices.Add(c);
            meshData.Vertices.Add(d);
            meshData.Triangles.Add(start);
            meshData.Triangles.Add(start + 2);
            meshData.Triangles.Add(start + 1);
            meshData.Triangles.Add(start);
            meshData.Triangles.Add(start + 3);
            meshData.Triangles.Add(start + 2);

            Vector3 normal = Vector3.zero;
            normal[axis] = normalSign;
            Color32 color = registry == null ? Color.white : registry.GetDebugColor(cell.BlockId);
            for (int i = 0; i < 4; i++)
            {
                meshData.Normals.Add(normal);
                meshData.Colors.Add(color);
            }

            meshData.QuadCount++;
        }

        private static int[] Add(int[] a, int[] b)
        {
            return new[] { a[0] + b[0], a[1] + b[1], a[2] + b[2] };
        }

        private static Vector3 ToVertex(int[] coord, int sectionYMin)
        {
            return new Vector3(coord[0], sectionYMin + coord[1], coord[2]);
        }

        private sealed class ChunkOnlySampler : IWorldBlockSampler
        {
            private readonly WorldChunkData chunk;

            public ChunkOnlySampler(WorldChunkData chunk)
            {
                this.chunk = chunk;
            }

            public CellState GetCell(int worldX, int y, int worldZ)
            {
                int localX = worldX - chunk.Coord.X * WorldDimensions.ChunkSizeX;
                int localZ = worldZ - chunk.Coord.Z * WorldDimensions.ChunkSizeZ;
                return chunk.GetCellLocal(localX, y, localZ);
            }
        }
    }
}
