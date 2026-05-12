using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectArcane.WorldGeneration
{
    [ExecuteAlways]
    public sealed class WorldGenerationController : MonoBehaviour
    {
        [SerializeField] private WorldGeneratorConfig config;
        [SerializeField] private BlockRegistry blockRegistry;
        [SerializeField] private Material debugMaterial;
        [SerializeField] private int centerChunkX;
        [SerializeField] private int centerChunkZ;
        [SerializeField] private int debugRadius = 2;
        [SerializeField] private int debugYMin;
        [SerializeField] private int debugYMax = 160;
        [SerializeField] private int renderSectionHeight = 32;
        [SerializeField] private bool debugVisible = true;

        private readonly Dictionary<ChunkCoord, WorldChunkData> chunks = new Dictionary<ChunkCoord, WorldChunkData>();
        private readonly List<VisibleChunk> visibleChunks = new List<VisibleChunk>();
        private const HideFlags DebugObjectHideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
        private GameObject debugRoot;
        private WorldGenerator generator;
        private Material transientDebugMaterial;

        public WorldGeneratorConfig Config { get => config; set { config = value; ResetGenerator(); } }
        public BlockRegistry BlockRegistry { get => blockRegistry; set => blockRegistry = value; }
        public Material DebugMaterial { get => debugMaterial; set => debugMaterial = value; }
        public int CenterChunkX { get => centerChunkX; set => centerChunkX = value; }
        public int CenterChunkZ { get => centerChunkZ; set => centerChunkZ = value; }
        public int DebugRadius { get => debugRadius; set => debugRadius = Mathf.Max(0, value); }
        public int DebugYMin { get => debugYMin; set => debugYMin = Mathf.Clamp(value, 0, WorldDimensions.WorldHeight - 1); }
        public int DebugYMax { get => debugYMax; set => debugYMax = Mathf.Clamp(value, 0, WorldDimensions.WorldHeight - 1); }
        public int RenderSectionHeight { get => renderSectionHeight; set => renderSectionHeight = Mathf.Max(1, value); }
        public int GeneratedChunkCount => chunks.Count;
        public int VisibleChunkCount => visibleChunks.Count;

        public void GenerateWorld()
        {
            if (config == null)
            {
                Debug.LogWarning("World generation skipped: config is not assigned.", this);
                return;
            }

            EnsureGenerator();
            visibleChunks.Clear();
            WorldDimensions dimensions = config.Dimensions;
            for (int dz = -debugRadius; dz <= debugRadius; dz++)
            {
                for (int dx = -debugRadius; dx <= debugRadius; dx++)
                {
                    int unwrappedChunkX = centerChunkX + dx;
                    int unwrappedChunkZ = centerChunkZ + dz;
                    ChunkCoord logicalCoord = new ChunkCoord(
                        TorusMath.WrapChunkX(unwrappedChunkX, dimensions),
                        TorusMath.WrapChunkZ(unwrappedChunkZ, dimensions));
                    GetOrGenerateChunk(logicalCoord);
                    visibleChunks.Add(new VisibleChunk(logicalCoord, unwrappedChunkX, unwrappedChunkZ));
                }
            }

            if (debugVisible)
            {
                RebuildDebugMeshes();
            }
        }

        public void ClearWorld()
        {
            chunks.Clear();
            visibleChunks.Clear();
            DestroyDebugRoot();
        }

        public void SetDebugVisible(bool visible)
        {
            debugVisible = visible;
            if (visible)
            {
                if (visibleChunks.Count == 0)
                {
                    GenerateWorld();
                }
                else
                {
                    RebuildDebugMeshes();
                }
            }
            else
            {
                DestroyDebugRoot();
            }
        }

        public void RebuildDebugMeshes()
        {
            if (!debugVisible || config == null)
            {
                return;
            }

            if (visibleChunks.Count == 0)
            {
                GenerateWorld();
                return;
            }

            DestroyDebugRoot();
            debugRoot = new GameObject("World Debug Visualization");
            debugRoot.hideFlags = DebugObjectHideFlags;
            debugRoot.transform.SetParent(transform, false);
            int yMin = Mathf.Min(DebugYMin, DebugYMax);
            int yMaxExclusive = Mathf.Clamp(Mathf.Max(DebugYMin, DebugYMax) + 1, yMin + 1, WorldDimensions.WorldHeight);
            BlockRegistry registry = GetRegistry();

            for (int i = 0; i < visibleChunks.Count; i++)
            {
                VisibleChunk visible = visibleChunks[i];
                WorldChunkData chunk = GetOrGenerateChunk(visible.LogicalCoord);
                GameObject chunkObject = new GameObject($"Chunk {visible.LogicalCoord.X},{visible.LogicalCoord.Z}");
                chunkObject.hideFlags = DebugObjectHideFlags;
                chunkObject.transform.SetParent(debugRoot.transform, false);
                chunkObject.transform.localPosition = new Vector3(
                    visible.VisualChunkX * WorldDimensions.ChunkSizeX,
                    0f,
                    visible.VisualChunkZ * WorldDimensions.ChunkSizeZ);

                for (int y = yMin; y < yMaxExclusive; y += RenderSectionHeight)
                {
                    int sectionEnd = Mathf.Min(y + RenderSectionHeight, yMaxExclusive);
                    GreedyMeshData meshData = FastVoxelChunkMesher.BuildMeshData(chunk, y, sectionEnd, registry, GetOrGenerateChunk);
                    if (meshData.IsEmpty)
                    {
                        continue;
                    }

                    GameObject sectionObject = new GameObject($"Section {y}-{sectionEnd - 1}");
                    sectionObject.hideFlags = DebugObjectHideFlags;
                    sectionObject.transform.SetParent(chunkObject.transform, false);
                    Mesh mesh = new Mesh
                    {
                        name = $"Chunk_{visible.LogicalCoord.X}_{visible.LogicalCoord.Z}_Y{y}_{sectionEnd - 1}",
                        hideFlags = DebugObjectHideFlags,
                        indexFormat = meshData.Vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16
                    };
                    mesh.SetVertices(meshData.Vertices);
                    mesh.SetTriangles(meshData.Triangles, 0);
                    mesh.SetColors(meshData.Colors);
                    mesh.SetNormals(meshData.Normals);
                    mesh.RecalculateBounds();

                    MeshFilter filter = sectionObject.AddComponent<MeshFilter>();
                    MeshRenderer renderer = sectionObject.AddComponent<MeshRenderer>();
                    filter.sharedMesh = mesh;
                    renderer.sharedMaterial = GetDebugMaterial();
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                    renderer.receiveShadows = false;
                }
            }
        }

        public WorldChunkData GetOrGenerateChunk(ChunkCoord chunkCoord)
        {
            EnsureGenerator();
            ChunkCoord wrapped = TorusMath.WrapChunk(chunkCoord, config.Dimensions);
            if (!chunks.TryGetValue(wrapped, out WorldChunkData chunk))
            {
                chunk = generator.GenerateChunk(wrapped);
                chunks.Add(wrapped, chunk);
            }

            return chunk;
        }

        public CellState GetCellAtWorld(int worldX, int y, int worldZ)
        {
            if (config == null || y < 0 || y >= WorldDimensions.WorldHeight)
            {
                return CellState.Air;
            }

            WorldDimensions dimensions = config.Dimensions;
            ChunkCoord chunkCoord = TorusMath.WorldToChunk(worldX, worldZ, dimensions);
            WorldChunkData chunk = GetOrGenerateChunk(chunkCoord);
            int localX = TorusMath.LocalX(worldX, dimensions);
            int localZ = TorusMath.LocalZ(worldZ, dimensions);
            return chunk.GetCellLocal(localX, y, localZ);
        }

        private void EnsureGenerator()
        {
            if (generator == null)
            {
                generator = new WorldGenerator(config, GetRegistry());
            }
        }

        private void ResetGenerator()
        {
            generator = null;
            chunks.Clear();
            visibleChunks.Clear();
        }

        private BlockRegistry GetRegistry()
        {
            if (blockRegistry == null)
            {
                blockRegistry = BlockRegistry.CreateDefaultForTests();
            }

            return blockRegistry;
        }

        private Material GetDebugMaterial()
        {
            if (debugMaterial != null)
            {
                return debugMaterial;
            }

            if (transientDebugMaterial == null)
            {
                Shader shader = Shader.Find("ProjectArcane/Debug/VertexColorLit");
                if (shader == null)
                {
                    shader = Shader.Find("Universal Render Pipeline/Unlit");
                }

                transientDebugMaterial = new Material(shader)
                {
                    name = "Transient Vertex Color Debug Material",
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            return transientDebugMaterial;
        }

        private void DestroyDebugRoot()
        {
            if (debugRoot == null)
            {
                Transform existing = transform.Find("World Debug Visualization");
                if (existing != null)
                {
                    debugRoot = existing.gameObject;
                }
            }

            if (debugRoot == null)
            {
                return;
            }

            DestroyGeneratedMeshes(debugRoot);

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(debugRoot);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(debugRoot);
            }

            debugRoot = null;
        }

        private static void DestroyGeneratedMeshes(GameObject root)
        {
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < filters.Length; i++)
            {
                Mesh mesh = filters[i].sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                filters[i].sharedMesh = null;
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(mesh);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(mesh);
                }
            }
        }

        private readonly struct VisibleChunk
        {
            public readonly ChunkCoord LogicalCoord;
            public readonly int VisualChunkX;
            public readonly int VisualChunkZ;

            public VisibleChunk(ChunkCoord logicalCoord, int visualChunkX, int visualChunkZ)
            {
                LogicalCoord = logicalCoord;
                VisualChunkX = visualChunkX;
                VisualChunkZ = visualChunkZ;
            }
        }

    }
}
