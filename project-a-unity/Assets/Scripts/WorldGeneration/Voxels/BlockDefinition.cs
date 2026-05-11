using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    [CreateAssetMenu(menuName = "Project Arcane/World Generation/Block Definition", fileName = "BlockDefinition")]
    public sealed class BlockDefinition : ScriptableObject
    {
        [SerializeField] private int stableId = 1;
        [SerializeField] private string stableKey = "stone";
        [SerializeField] private BlockLayer defaultLayer = BlockLayer.Terrain;
        [SerializeField] private Color32 debugColor = new Color32(130, 130, 130, 255);
        [SerializeField] private bool solid = true;

        public int StableId => stableId;
        public string StableKey => stableKey;
        public BlockLayer DefaultLayer => defaultLayer;
        public Color32 DebugColor => debugColor;
        public bool Solid => solid;

        public void ConfigureForTests(int id, string key, BlockLayer layer, Color32 color, bool isSolid)
        {
            stableId = id;
            stableKey = key;
            defaultLayer = layer;
            debugColor = color;
            solid = isSolid;
        }
    }
}
