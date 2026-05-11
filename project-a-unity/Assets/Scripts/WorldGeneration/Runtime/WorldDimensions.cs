using System;
using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    [Serializable]
    public struct WorldDimensions
    {
        public const int ChunkSizeX = 16;
        public const int ChunkSizeZ = 16;
        public const int WorldHeight = 512;

        [SerializeField] private int chunkCountX;
        [SerializeField] private int chunkCountZ;

        public WorldDimensions(int chunkCountX, int chunkCountZ)
        {
            this.chunkCountX = Mathf.Max(1, chunkCountX);
            this.chunkCountZ = Mathf.Max(1, chunkCountZ);
        }

        public int ChunkCountX => Mathf.Max(1, chunkCountX);
        public int ChunkCountZ => Mathf.Max(1, chunkCountZ);
        public int WorldSizeX => ChunkCountX * ChunkSizeX;
        public int WorldSizeZ => ChunkCountZ * ChunkSizeZ;
    }
}
