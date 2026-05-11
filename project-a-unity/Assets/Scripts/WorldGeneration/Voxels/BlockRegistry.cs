using System.Collections.Generic;
using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    [CreateAssetMenu(menuName = "Project Arcane/World Generation/Block Registry", fileName = "BlockRegistry")]
    public sealed class BlockRegistry : ScriptableObject
    {
        public const int AirId = 0;
        public const int DefaultStoneId = 1;
        public const int DefaultSoilId = 2;
        public const int DefaultGrassId = 3;

        [SerializeField] private List<BlockDefinition> blocks = new List<BlockDefinition>();

        private readonly Dictionary<int, BlockDefinition> lookup = new Dictionary<int, BlockDefinition>();
        private bool lookupDirty = true;

        public IReadOnlyList<BlockDefinition> Blocks => blocks;

        private void OnValidate()
        {
            lookupDirty = true;
        }

        public Color32 GetDebugColor(int blockId)
        {
            if (blockId == AirId)
            {
                return new Color32(0, 0, 0, 0);
            }

            BlockDefinition definition = GetDefinition(blockId);
            if (definition != null)
            {
                return definition.DebugColor;
            }

            if (blockId == DefaultStoneId)
            {
                return new Color32(118, 121, 124, 255);
            }

            if (blockId == DefaultSoilId)
            {
                return new Color32(116, 82, 52, 255);
            }

            if (blockId == DefaultGrassId)
            {
                return new Color32(76, 150, 74, 255);
            }

            return new Color32(220, 80, 220, 255);
        }

        public bool IsSolid(CellState cell)
        {
            if (cell.IsAir)
            {
                return false;
            }

            BlockDefinition definition = GetDefinition(cell.BlockId);
            return definition == null || definition.Solid;
        }

        public BlockDefinition GetDefinition(int blockId)
        {
            EnsureLookup();
            lookup.TryGetValue(blockId, out BlockDefinition definition);
            return definition;
        }

        private void EnsureLookup()
        {
            if (!lookupDirty)
            {
                return;
            }

            lookup.Clear();
            for (int i = 0; i < blocks.Count; i++)
            {
                BlockDefinition block = blocks[i];
                if (block == null || block.StableId == AirId)
                {
                    continue;
                }

                lookup[block.StableId] = block;
            }

            lookupDirty = false;
        }

        public static BlockRegistry CreateDefaultForTests()
        {
            BlockRegistry registry = CreateInstance<BlockRegistry>();
            registry.blocks = new List<BlockDefinition>
            {
                CreateTestBlock(DefaultStoneId, "stone", new Color32(118, 121, 124, 255)),
                CreateTestBlock(DefaultSoilId, "soil", new Color32(116, 82, 52, 255)),
                CreateTestBlock(DefaultGrassId, "grass", new Color32(76, 150, 74, 255))
            };
            registry.lookupDirty = true;
            return registry;
        }

        private static BlockDefinition CreateTestBlock(int id, string key, Color32 color)
        {
            BlockDefinition block = CreateInstance<BlockDefinition>();
            block.ConfigureForTests(id, key, BlockLayer.Terrain, color, true);
            return block;
        }
    }
}
