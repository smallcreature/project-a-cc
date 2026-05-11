using System;

namespace ProjectArcane.WorldGeneration
{
    [Serializable]
    public struct CellState : IEquatable<CellState>
    {
        public int BlockId;
        public BlockLayer Layer;
        public byte Flags;

        public CellState(int blockId, BlockLayer layer, byte flags = 0)
        {
            BlockId = blockId;
            Layer = blockId == 0 ? BlockLayer.None : layer;
            Flags = flags;
        }

        public static CellState Air => new CellState(0, BlockLayer.None);
        public bool IsAir => BlockId == 0 || Layer == BlockLayer.None;

        public bool Equals(CellState other)
        {
            return BlockId == other.BlockId && Layer == other.Layer && Flags == other.Flags;
        }

        public override bool Equals(object obj)
        {
            return obj is CellState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = BlockId;
                hash = (hash * 397) ^ (int)Layer;
                hash = (hash * 397) ^ Flags;
                return hash;
            }
        }
    }
}
