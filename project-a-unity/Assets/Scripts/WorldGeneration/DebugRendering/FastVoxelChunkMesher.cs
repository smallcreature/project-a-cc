using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    public static class FastVoxelChunkMesher
    {
        public static GreedyMeshData BuildMeshData(
            WorldChunkData chunk,
            int sectionYMin,
            int sectionYMaxExclusive,
            BlockRegistry registry,
            Func<ChunkCoord, WorldChunkData> chunkProvider = null)
        {
            if (chunk == null)
            {
                return new GreedyMeshData();
            }

            sectionYMin = Mathf.Clamp(sectionYMin, 0, WorldDimensions.WorldHeight);
            sectionYMaxExclusive = Mathf.Clamp(sectionYMaxExclusive, sectionYMin, WorldDimensions.WorldHeight);
            if (sectionYMaxExclusive <= sectionYMin)
            {
                return new GreedyMeshData();
            }

            int sectionHeight = sectionYMaxExclusive - sectionYMin;
            int estimatedQuads = WorldDimensions.ChunkSizeX * WorldDimensions.ChunkSizeZ +
                                 sectionHeight * (WorldDimensions.ChunkSizeX + WorldDimensions.ChunkSizeZ) * 2;
            GreedyMeshData meshData = new GreedyMeshData(estimatedQuads);
            ChunkNeighborhood neighborhood = new ChunkNeighborhood(chunk, chunkProvider);
            BlockDebugLookup blockLookup = new BlockDebugLookup(registry);
            CellState[] cells = chunk.Cells;
            for (int localZ = 0; localZ < WorldDimensions.ChunkSizeZ; localZ++)
            {
                for (int localX = 0; localX < WorldDimensions.ChunkSizeX; localX++)
                {
                    int columnStart = WorldDimensions.WorldHeight * (localX + WorldDimensions.ChunkSizeX * localZ);
                    for (int y = sectionYMin; y < sectionYMaxExclusive; y++)
                    {
                        CellState cell = cells[columnStart + y];
                        if (!blockLookup.IsSolid(cell))
                        {
                            continue;
                        }

                        if (!blockLookup.IsSolid(neighborhood.GetCell(localX + 1, y, localZ)))
                        {
                            AddQuad(meshData, localX + 1, y, localZ, 0, 1, 0, 0, 0, 1, 0, 1, cell, blockLookup);
                        }

                        if (!blockLookup.IsSolid(neighborhood.GetCell(localX - 1, y, localZ)))
                        {
                            AddQuad(meshData, localX, y, localZ, 0, 1, 0, 0, 0, 1, 0, -1, cell, blockLookup);
                        }

                        if (!blockLookup.IsSolid(neighborhood.GetCell(localX, y + 1, localZ)))
                        {
                            AddQuad(meshData, localX, y + 1, localZ, 0, 0, 1, 1, 0, 0, 1, 1, cell, blockLookup);
                        }

                        if (!blockLookup.IsSolid(neighborhood.GetCell(localX, y - 1, localZ)))
                        {
                            AddQuad(meshData, localX, y, localZ, 0, 0, 1, 1, 0, 0, 1, -1, cell, blockLookup);
                        }

                        if (!blockLookup.IsSolid(neighborhood.GetCell(localX, y, localZ + 1)))
                        {
                            AddQuad(meshData, localX, y, localZ + 1, 1, 0, 0, 0, 1, 0, 2, 1, cell, blockLookup);
                        }

                        if (!blockLookup.IsSolid(neighborhood.GetCell(localX, y, localZ - 1)))
                        {
                            AddQuad(meshData, localX, y, localZ, 1, 0, 0, 0, 1, 0, 2, -1, cell, blockLookup);
                        }
                    }
                }
            }

            return meshData;
        }

        private static void AddQuad(
            GreedyMeshData meshData,
            int x,
            int y,
            int z,
            int duX,
            int duY,
            int duZ,
            int dvX,
            int dvY,
            int dvZ,
            int normalAxis,
            int normalSign,
            CellState cell,
            BlockDebugLookup blockLookup)
        {
            int start = meshData.Vertices.Count;
            Vector3 a = new Vector3(x, y, z);
            Vector3 b = normalSign > 0
                ? new Vector3(x + dvX, y + dvY, z + dvZ)
                : new Vector3(x + duX, y + duY, z + duZ);
            Vector3 c = new Vector3(x + duX + dvX, y + duY + dvY, z + duZ + dvZ);
            Vector3 d = normalSign > 0
                ? new Vector3(x + duX, y + duY, z + duZ)
                : new Vector3(x + dvX, y + dvY, z + dvZ);

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
            normal[normalAxis] = normalSign;
            Color32 color = blockLookup.GetColor(cell.BlockId);
            for (int i = 0; i < 4; i++)
            {
                meshData.Normals.Add(normal);
                meshData.Colors.Add(color);
            }

            meshData.QuadCount++;
        }

        private sealed class BlockDebugLookup
        {
            private readonly BlockRegistry registry;
            private readonly bool[] defined;
            private readonly bool[] solid;
            private readonly Color32[] colors;

            public BlockDebugLookup(BlockRegistry registry)
            {
                this.registry = registry;
                IReadOnlyList<BlockDefinition> blocks = registry == null ? null : registry.Blocks;
                int maxId = 0;
                if (blocks != null)
                {
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        BlockDefinition block = blocks[i];
                        if (block != null)
                        {
                            maxId = Mathf.Max(maxId, block.StableId);
                        }
                    }
                }

                defined = new bool[maxId + 1];
                solid = new bool[maxId + 1];
                colors = new Color32[maxId + 1];
                if (blocks == null)
                {
                    return;
                }

                for (int i = 0; i < blocks.Count; i++)
                {
                    BlockDefinition block = blocks[i];
                    if (block == null || block.StableId < 0 || block.StableId >= defined.Length)
                    {
                        continue;
                    }

                    defined[block.StableId] = true;
                    solid[block.StableId] = block.Solid;
                    colors[block.StableId] = block.DebugColor;
                }
            }

            public bool IsSolid(CellState cell)
            {
                if (cell.IsAir)
                {
                    return false;
                }

                int blockId = cell.BlockId;
                if (blockId >= 0 && blockId < defined.Length && defined[blockId])
                {
                    return solid[blockId];
                }

                return registry == null || registry.IsSolid(cell);
            }

            public Color32 GetColor(int blockId)
            {
                if (blockId >= 0 && blockId < defined.Length && defined[blockId])
                {
                    return colors[blockId];
                }

                return registry == null ? Color.white : registry.GetDebugColor(blockId);
            }
        }

        private sealed class ChunkNeighborhood
        {
            private readonly WorldChunkData center;
            private readonly Func<ChunkCoord, WorldChunkData> chunkProvider;
            private bool leftResolved;
            private bool rightResolved;
            private bool backResolved;
            private bool frontResolved;
            private WorldChunkData left;
            private WorldChunkData right;
            private WorldChunkData back;
            private WorldChunkData front;

            public ChunkNeighborhood(WorldChunkData center, Func<ChunkCoord, WorldChunkData> chunkProvider)
            {
                this.center = center;
                this.chunkProvider = chunkProvider;
            }

            public CellState GetCell(int localX, int y, int localZ)
            {
                if (y < 0 || y >= WorldDimensions.WorldHeight)
                {
                    return CellState.Air;
                }

                if (localX >= 0 && localX < WorldDimensions.ChunkSizeX &&
                    localZ >= 0 && localZ < WorldDimensions.ChunkSizeZ)
                {
                    return center.Cells[WorldChunkData.ToIndex(localX, y, localZ)];
                }

                if (chunkProvider == null)
                {
                    return CellState.Air;
                }

                if (localX < 0)
                {
                    WorldChunkData chunk = GetLeft();
                    return chunk == null ? CellState.Air : chunk.GetCellLocal(WorldDimensions.ChunkSizeX - 1, y, localZ);
                }

                if (localX >= WorldDimensions.ChunkSizeX)
                {
                    WorldChunkData chunk = GetRight();
                    return chunk == null ? CellState.Air : chunk.GetCellLocal(0, y, localZ);
                }

                if (localZ < 0)
                {
                    WorldChunkData chunk = GetBack();
                    return chunk == null ? CellState.Air : chunk.GetCellLocal(localX, y, WorldDimensions.ChunkSizeZ - 1);
                }

                WorldChunkData frontChunk = GetFront();
                return frontChunk == null ? CellState.Air : frontChunk.GetCellLocal(localX, y, 0);
            }

            private WorldChunkData GetLeft()
            {
                if (!leftResolved)
                {
                    left = ResolveNeighbor(-1, 0);
                    leftResolved = true;
                }

                return left;
            }

            private WorldChunkData GetRight()
            {
                if (!rightResolved)
                {
                    right = ResolveNeighbor(1, 0);
                    rightResolved = true;
                }

                return right;
            }

            private WorldChunkData GetBack()
            {
                if (!backResolved)
                {
                    back = ResolveNeighbor(0, -1);
                    backResolved = true;
                }

                return back;
            }

            private WorldChunkData GetFront()
            {
                if (!frontResolved)
                {
                    front = ResolveNeighbor(0, 1);
                    frontResolved = true;
                }

                return front;
            }

            private WorldChunkData ResolveNeighbor(int chunkDeltaX, int chunkDeltaZ)
            {
                WorldDimensions dimensions = center.Dimensions;
                ChunkCoord coord = new ChunkCoord(
                    TorusMath.WrapChunkX(center.Coord.X + chunkDeltaX, dimensions),
                    TorusMath.WrapChunkZ(center.Coord.Z + chunkDeltaZ, dimensions));
                return chunkProvider(coord);
            }
        }
    }
}
