using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using ProjectArcane.WorldGeneration;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectArcane.Tests.EditMode.WorldGeneration
{
    public sealed class WorldGenerationEditModeTests
    {
        private const int TestSeed = 52711;
        private static readonly BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly BindingFlags AnyStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        [Test]
        public void TorusMath_WrapsNegativeCoordinatesIntoPositiveRange()
        {
            WorldDimensions dimensions = new WorldDimensions(2, 3);

            Assert.That(TorusMath.PositiveModulo(-1, 16), Is.EqualTo(15));
            Assert.That(TorusMath.PositiveModulo(-17, 16), Is.EqualTo(15));
            Assert.That(TorusMath.WrapX(-1, dimensions), Is.EqualTo(dimensions.WorldSizeX - 1));
            Assert.That(TorusMath.WrapZ(-1, dimensions), Is.EqualTo(dimensions.WorldSizeZ - 1));
            Assert.That(TorusMath.LocalX(-1, dimensions), Is.EqualTo(WorldDimensions.ChunkSizeX - 1));
            Assert.That(TorusMath.LocalZ(-1, dimensions), Is.EqualTo(WorldDimensions.ChunkSizeZ - 1));

            ChunkCoord wrappedChunk = TorusMath.WorldToChunk(-1, -1, dimensions);
            Assert.That(wrappedChunk.X, Is.EqualTo(dimensions.ChunkCountX - 1));
            Assert.That(wrappedChunk.Z, Is.EqualTo(dimensions.ChunkCountZ - 1));
        }

        [Test]
        public void TorusMath_ReturnsShortestDeltaAcrossWrappedSeam()
        {
            WorldDimensions dimensions = new WorldDimensions(2, 2);

            Assert.That(TorusMath.ShortestDelta(0, dimensions.WorldSizeX - 1, dimensions.WorldSizeX), Is.EqualTo(-1));
            Assert.That(TorusMath.ShortestDelta(dimensions.WorldSizeX - 1, 0, dimensions.WorldSizeX), Is.EqualTo(1));
            Assert.That(TorusMath.ShortestDelta(2, dimensions.WorldSizeX - 2, dimensions.WorldSizeX), Is.EqualTo(-4));
            Assert.That(TorusMath.ShortestDeltaX(0, dimensions.WorldSizeX - 1, dimensions), Is.EqualTo(-1));
            Assert.That(TorusMath.ShortestDeltaZ(dimensions.WorldSizeZ - 1, 0, dimensions), Is.EqualTo(1));
        }

        [Test]
        public void PeriodicNoise_IsSeamlessAcrossWorldXAndZ()
        {
            WorldDimensions dimensions = new WorldDimensions(2, 3);
            object settings = Api.CreateNoiseSettings(TestSeed, dimensions);

            float z = 11.25f;
            float x = 7.5f;
            float left = Api.SamplePeriodicNoise(settings, dimensions, 0f, z);
            float right = Api.SamplePeriodicNoise(settings, dimensions, dimensions.WorldSizeX, z);
            float beforeLeft = Api.SamplePeriodicNoise(settings, dimensions, -4.75f, z);
            float afterRight = Api.SamplePeriodicNoise(settings, dimensions, dimensions.WorldSizeX - 4.75f, z);
            float back = Api.SamplePeriodicNoise(settings, dimensions, x, 0f);
            float front = Api.SamplePeriodicNoise(settings, dimensions, x, dimensions.WorldSizeZ);
            float beforeBack = Api.SamplePeriodicNoise(settings, dimensions, x, -3.5f);
            float afterFront = Api.SamplePeriodicNoise(settings, dimensions, x, dimensions.WorldSizeZ - 3.5f);

            Assert.That(right, Is.EqualTo(left).Within(0.0005f), "X seam should sample the same value at 0 and world width.");
            Assert.That(afterRight, Is.EqualTo(beforeLeft).Within(0.0005f), "X seam should stay periodic for negative coordinates too.");
            Assert.That(front, Is.EqualTo(back).Within(0.0005f), "Z seam should sample the same value at 0 and world depth.");
            Assert.That(afterFront, Is.EqualTo(beforeBack).Within(0.0005f), "Z seam should stay periodic for negative coordinates too.");
        }

        [Test]
        public void WorldGenerator_GeneratesDeterministicChunkForSameSeedConfigAndCoord()
        {
            WorldDimensions dimensions = new WorldDimensions(2, 2);
            using (GeneratedWorld world = GeneratedWorld.Create(TestSeed, dimensions))
            {
                ChunkCoord coord = new ChunkCoord(1, 0);

                object first = world.GenerateChunk(coord);
                object second = world.GenerateChunk(coord);

                Assert.That(Api.ChunkFingerprint(second), Is.EqualTo(Api.ChunkFingerprint(first)));
            }
        }

        [Test]
        public void WorldGenerator_StageToggleChangesGeneratedChunkData()
        {
            WorldDimensions dimensions = new WorldDimensions(2, 2);
            object enabledConfig = Api.CreateGeneratorConfig(TestSeed, dimensions);
            object disabledConfig = Api.CreateGeneratorConfig(TestSeed, dimensions);
            StageToggle toggle = Api.FindStageToggle(enabledConfig);

            Assert.That(toggle, Is.Not.Null, "WorldGeneratorConfig should expose at least one generation-stage toggle for tests.");
            toggle.Set(enabledConfig, true);
            toggle.Set(disabledConfig, false);

            using (GeneratedWorld enabledWorld = GeneratedWorld.Create(enabledConfig, dimensions))
            using (GeneratedWorld disabledWorld = GeneratedWorld.Create(disabledConfig, dimensions))
            {
                ChunkCoord[] coords =
                {
                    new ChunkCoord(0, 0),
                    new ChunkCoord(1, 0),
                    new ChunkCoord(0, 1),
                    new ChunkCoord(1, 1)
                };

                bool foundDifference = false;
                for (int i = 0; i < coords.Length; i++)
                {
                    object enabledChunk = enabledWorld.GenerateChunk(coords[i]);
                    object disabledChunk = disabledWorld.GenerateChunk(coords[i]);
                    if (!Api.ChunkFingerprint(enabledChunk).Equals(Api.ChunkFingerprint(disabledChunk)))
                    {
                        foundDifference = true;
                        break;
                    }
                }

                Assert.That(foundDifference, Is.True, $"Toggling {toggle.Name} should affect at least one nearby generated chunk.");
            }
        }

        [Test]
        public void GreedyChunkMesher_CullsInteriorFaceForAdjacentSolidBlocks()
        {
            using (MeshTestFixture fixture = MeshTestFixture.Create())
            {
                object singleBlockChunk = fixture.CreateChunkWithSolidBlocks(new VoxelCoord(0, 10, 0));
                object adjacentPairChunk = fixture.CreateChunkWithSolidBlocks(new VoxelCoord(0, 10, 0), new VoxelCoord(1, 10, 0));

                int singleBlockQuads = fixture.CountGeneratedQuads(singleBlockChunk);
                int adjacentPairQuads = fixture.CountGeneratedQuads(adjacentPairChunk);

                Assert.That(singleBlockQuads, Is.GreaterThan(0), "A visible solid block should produce mesh quads.");
                Assert.That(adjacentPairQuads, Is.LessThan(singleBlockQuads * 2), "Adjacent solid blocks should not emit both interior faces.");
                Assert.That(adjacentPairQuads, Is.LessThanOrEqualTo(10), "Two adjacent cubes have at most ten exposed faces before greedy surface merging.");
            }
        }

        private sealed class GeneratedWorld : IDisposable
        {
            private readonly List<Object> objectsToDestroy = new List<Object>();

            private GeneratedWorld(object config, object registry, object generator, WorldDimensions dimensions)
            {
                Config = config;
                Registry = registry;
                Generator = generator;
                Dimensions = dimensions;
                Track(config);
                Track(registry);
                Track(generator);
            }

            public object Config { get; }
            public object Registry { get; }
            public object Generator { get; }
            public WorldDimensions Dimensions { get; }

            public static GeneratedWorld Create(int seed, WorldDimensions dimensions)
            {
                object config = Api.CreateGeneratorConfig(seed, dimensions);
                return Create(config, dimensions);
            }

            public static GeneratedWorld Create(object config, WorldDimensions dimensions)
            {
                object registry = BlockRegistry.CreateDefaultForTests();
                object generator = Api.CreateWorldGenerator(config, registry, dimensions);
                return new GeneratedWorld(config, registry, generator, dimensions);
            }

            public object GenerateChunk(ChunkCoord coord)
            {
                return Api.GenerateChunk(Generator, Config, Registry, Dimensions, coord);
            }

            public void Dispose()
            {
                for (int i = objectsToDestroy.Count - 1; i >= 0; i--)
                {
                    if (objectsToDestroy[i] != null)
                    {
                        Object.DestroyImmediate(objectsToDestroy[i]);
                    }
                }
            }

            private void Track(object value)
            {
                Object unityObject = value as Object;
                if (unityObject != null && !objectsToDestroy.Contains(unityObject))
                {
                    objectsToDestroy.Add(unityObject);
                }
            }
        }

        private sealed class MeshTestFixture : IDisposable
        {
            private readonly List<Object> objectsToDestroy = new List<Object>();

            private MeshTestFixture(BlockRegistry registry, object mesher)
            {
                Registry = registry;
                Mesher = mesher;
                Track(registry);
                Track(mesher);
            }

            private BlockRegistry Registry { get; }
            private object Mesher { get; }

            public static MeshTestFixture Create()
            {
                BlockRegistry registry = BlockRegistry.CreateDefaultForTests();
                object mesher = Api.CreateGreedyChunkMesher(registry);
                return new MeshTestFixture(registry, mesher);
            }

            public object CreateChunkWithSolidBlocks(params VoxelCoord[] solidBlocks)
            {
                object chunk = Api.CreateWorldChunkData(new ChunkCoord(0, 0));
                for (int i = 0; i < solidBlocks.Length; i++)
                {
                    Api.SetCell(chunk, solidBlocks[i], new CellState(BlockRegistry.DefaultStoneId, BlockLayer.Terrain));
                }

                Track(chunk);
                return chunk;
            }

            public int CountGeneratedQuads(object chunk)
            {
                object meshData = Api.MeshChunk(Mesher, chunk, Registry);
                Track(meshData);
                return Api.CountQuads(meshData);
            }

            public void Dispose()
            {
                for (int i = objectsToDestroy.Count - 1; i >= 0; i--)
                {
                    if (objectsToDestroy[i] != null)
                    {
                        Object.DestroyImmediate(objectsToDestroy[i]);
                    }
                }
            }

            private void Track(object value)
            {
                Object unityObject = value as Object;
                if (unityObject != null && !objectsToDestroy.Contains(unityObject))
                {
                    objectsToDestroy.Add(unityObject);
                }
            }
        }

        private sealed class StageToggle
        {
            private readonly MemberInfo member;

            public StageToggle(MemberInfo member)
            {
                this.member = member;
                Name = member.Name;
            }

            public string Name { get; }

            public void Set(object target, bool value)
            {
                Api.SetMemberValue(member, target, value);
            }
        }

        private struct ChunkFingerprint : IEquatable<ChunkFingerprint>
        {
            public readonly long Hash;
            public readonly int Count;

            public ChunkFingerprint(long hash, int count)
            {
                Hash = hash;
                Count = count;
            }

            public bool Equals(ChunkFingerprint other)
            {
                return Hash == other.Hash && Count == other.Count;
            }

            public override bool Equals(object obj)
            {
                return obj is ChunkFingerprint other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int)Hash * 397) ^ Count;
                }
            }

            public override string ToString()
            {
                return $"{Hash}:{Count}";
            }
        }

        private static class Api
        {
            private static readonly string[] StageToggleWords =
            {
                "biome",
                "height",
                "terrain",
                "cave",
                "surface",
                "replacement",
                "object",
                "erosion",
                "continental",
                "cliff",
                "overhang",
                "stage"
            };

            public static object CreateNoiseSettings(int seed, WorldDimensions dimensions)
            {
                Type type = RequiredType("NoiseSettings");
                object settings = CreateConfigLikeObject(type);

                TrySetMember(settings, seed, "Seed", "seed", "BaseSeed", "baseSeed");
                TrySetMember(settings, 0.075f, "Frequency", "frequency", "Scale", "scale");
                TrySetMember(settings, 3, "Octaves", "octaves");
                TrySetMember(settings, 0.5f, "Persistence", "persistence");
                TrySetMember(settings, 2f, "Lacunarity", "lacunarity");
                TrySetMember(settings, 1f, "Amplitude", "amplitude");
                TrySetMember(settings, dimensions.WorldSizeX, "PeriodX", "periodX", "WorldSizeX", "worldSizeX");
                TrySetMember(settings, dimensions.WorldSizeZ, "PeriodZ", "periodZ", "WorldSizeZ", "worldSizeZ");

                return settings;
            }

            public static float SamplePeriodicNoise(object settings, WorldDimensions dimensions, float x, float z)
            {
                Type type = RequiredType("PeriodicNoise");

                object[] staticArgs;
                if (TryInvoke(type, null, AnyStatic, new[] { "Sample2D", "Sample", "Evaluate2D", "Evaluate", "Fractal2D" }, out object staticResult,
                    new object[] { x, z, settings, dimensions },
                    new object[] { settings, dimensions, x, z },
                    new object[] { settings, x, z, dimensions },
                    new object[] { x, z, settings, dimensions.WorldSizeX, dimensions.WorldSizeZ },
                    new object[] { settings, x, z, dimensions.WorldSizeX, dimensions.WorldSizeZ },
                    new object[] { x, z, settings },
                    new object[] { new Vector2(x, z), settings, dimensions },
                    new object[] { settings, new Vector2(x, z), dimensions },
                    new object[] { new Vector2(x, z), settings }))
                {
                    return Convert.ToSingle(staticResult);
                }

                object instance = CreatePeriodicNoiseInstance(type, settings, dimensions);
                if (TryInvoke(type, instance, AnyInstance, new[] { "Sample2D", "Sample", "Evaluate2D", "Evaluate", "Fractal2D" }, out object instanceResult,
                    new object[] { x, z },
                    new object[] { new Vector2(x, z) },
                    new object[] { x, z, dimensions },
                    new object[] { x, z, settings, dimensions },
                    new object[] { settings, dimensions, x, z }))
                {
                    return Convert.ToSingle(instanceResult);
                }

                staticArgs = new object[] { x, z, settings, dimensions };
                Assert.Fail($"PeriodicNoise should expose a 2D sampling method compatible with ({FormatArguments(staticArgs)}).");
                return 0f;
            }

            public static object CreateGeneratorConfig(int seed, WorldDimensions dimensions)
            {
                Type type = RequiredType("WorldGeneratorConfig");
                MethodInfo factory = FindMethod(type, AnyStatic, new[] { "CreateDefaultForTests" }, Array.Empty<object>());
                Assert.That(factory, Is.Not.Null, "WorldGeneratorConfig.CreateDefaultForTests() should exist for EditMode tests.");

                object config = factory.Invoke(null, null);
                Assert.That(config, Is.Not.Null, "WorldGeneratorConfig.CreateDefaultForTests() should return a config instance.");

                TrySetMember(config, seed, "Seed", "seed", "WorldSeed", "worldSeed", "BaseSeed", "baseSeed");
                TrySetMember(config, dimensions, "Dimensions", "dimensions", "WorldDimensions", "worldDimensions");
                TrySetMember(config, dimensions.ChunkCountX, "ChunkCountX", "chunkCountX");
                TrySetMember(config, dimensions.ChunkCountZ, "ChunkCountZ", "chunkCountZ");
                TrySetMember(config, dimensions.WorldSizeX, "WorldSizeX", "worldSizeX");
                TrySetMember(config, dimensions.WorldSizeZ, "WorldSizeZ", "worldSizeZ");

                return config;
            }

            public static object CreateWorldGenerator(object config, object registry, WorldDimensions dimensions)
            {
                Type type = RequiredType("WorldGenerator");

                object generator;
                if (TryCreateInstance(type, out generator,
                    new object[] { config, registry },
                    new object[] { config, registry, dimensions },
                    new object[] { registry, config },
                    new object[] { dimensions, config, registry },
                    new object[] { config },
                    Array.Empty<object>()))
                {
                    TrySetMember(generator, config, "Config", "config", "WorldGeneratorConfig", "worldGeneratorConfig");
                    TrySetMember(generator, registry, "Registry", "registry", "BlockRegistry", "blockRegistry");
                    TrySetMember(generator, dimensions, "Dimensions", "dimensions", "WorldDimensions", "worldDimensions");
                    return generator;
                }

                Assert.Fail("WorldGenerator should be constructible for tests with config and BlockRegistry.");
                return null;
            }

            public static object GenerateChunk(object generator, object config, object registry, WorldDimensions dimensions, ChunkCoord coord)
            {
                Type type = generator != null ? generator.GetType() : RequiredType("WorldGenerator");
                if (TryInvoke(type, generator, AnyInstance, new[] { "GenerateChunk", "Generate", "CreateChunk", "BuildChunk" }, out object result,
                    new object[] { coord },
                    new object[] { coord, config },
                    new object[] { coord, config, registry },
                    new object[] { coord, config, registry, dimensions },
                    new object[] { coord.X, coord.Z },
                    new object[] { coord.X, coord.Z, config },
                    new object[] { coord.X, coord.Z, config, registry },
                    new object[] { coord.X, coord.Z, config, registry, dimensions }))
                {
                    Assert.That(result, Is.Not.Null, "GenerateChunk should return WorldChunkData.");
                    return result;
                }

                if (TryInvoke(type, null, AnyStatic, new[] { "GenerateChunk", "Generate", "CreateChunk", "BuildChunk" }, out result,
                    new object[] { coord, config, registry },
                    new object[] { coord, config, registry, dimensions },
                    new object[] { coord.X, coord.Z, config, registry },
                    new object[] { coord.X, coord.Z, config, registry, dimensions }))
                {
                    Assert.That(result, Is.Not.Null, "GenerateChunk should return WorldChunkData.");
                    return result;
                }

                Assert.Fail("WorldGenerator should expose GenerateChunk/Generate/CreateChunk/BuildChunk returning WorldChunkData.");
                return null;
            }

            public static StageToggle FindStageToggle(object config)
            {
                Type type = config.GetType();
                StageToggle fallback = null;

                foreach (MemberInfo member in GetWritableMembers(type))
                {
                    Type memberType = GetMemberType(member);
                    if (memberType != typeof(bool))
                    {
                        continue;
                    }

                    if (NameContainsAny(member.Name, StageToggleWords))
                    {
                        return new StageToggle(member);
                    }

                    if (fallback == null)
                    {
                        fallback = new StageToggle(member);
                    }
                }

                return fallback;
            }

            public static ChunkFingerprint ChunkFingerprint(object chunk)
            {
                Assert.That(chunk, Is.Not.Null);

                if (TryHashStoredCollections(chunk, out ChunkFingerprint collectionFingerprint))
                {
                    return collectionFingerprint;
                }

                MethodInfo getter = FindMethod(chunk.GetType(), AnyInstance, new[] { "GetCell", "GetVoxel", "GetBlock", "GetBlockId" }, new object[] { 0, 0, 0 });
                Assert.That(getter, Is.Not.Null, "WorldChunkData should expose readable voxel data for deterministic generation tests.");

                long hash = 1469598103934665603L;
                int count = 0;
                for (int y = 0; y < WorldDimensions.WorldHeight; y++)
                {
                    for (int z = 0; z < WorldDimensions.ChunkSizeZ; z++)
                    {
                        for (int x = 0; x < WorldDimensions.ChunkSizeX; x++)
                        {
                            object cell = Invoke(getter, chunk, new object[] { x, y, z });
                            AddHash(ref hash, HashValue(cell));
                            count++;
                        }
                    }
                }

                return new ChunkFingerprint(hash, count);
            }

            public static object CreateWorldChunkData(ChunkCoord coord)
            {
                Type type = RequiredType("WorldChunkData");

                MethodInfo factory = FindMethod(type, AnyStatic, new[] { "CreateForTests", "Create" }, new object[] { coord });
                if (factory != null)
                {
                    return factory.Invoke(null, new object[] { coord });
                }

                object chunk;
                if (TryCreateInstance(type, out chunk,
                    new object[] { coord },
                    new object[] { coord, new WorldDimensions(1, 1) },
                    new object[] { new WorldDimensions(1, 1), coord },
                    new object[] { coord.X, coord.Z },
                    Array.Empty<object>()))
                {
                    TrySetMember(chunk, coord, "Coord", "coord", "ChunkCoord", "chunkCoord");
                    TrySetMember(chunk, coord.X, "X", "x", "ChunkX", "chunkX");
                    TrySetMember(chunk, coord.Z, "Z", "z", "ChunkZ", "chunkZ");
                    return chunk;
                }

                Assert.Fail("WorldChunkData should be constructible for tests.");
                return null;
            }

            public static void SetCell(object chunk, VoxelCoord coord, CellState cell)
            {
                Type type = chunk.GetType();

                if (TryInvoke(type, chunk, AnyInstance, new[] { "SetCell", "SetVoxel", "SetBlock" }, out _,
                    new object[] { coord.X, coord.Y, coord.Z, cell },
                    new object[] { coord.X, coord.Y, coord.Z, cell.BlockId },
                    new object[] { coord.X, coord.Y, coord.Z, cell.BlockId, cell.Layer }))
                {
                    return;
                }

                if (TrySetIndexedStorage(chunk, coord, cell))
                {
                    return;
                }

                Assert.Fail("WorldChunkData should expose SetCell/SetVoxel/SetBlock or writable dense storage for mesher tests.");
            }

            public static object CreateGreedyChunkMesher(BlockRegistry registry)
            {
                Type type = RequiredType("GreedyChunkMesher");

                object mesher;
                if (TryCreateInstance(type, out mesher, new object[] { registry }, Array.Empty<object>()))
                {
                    TrySetMember(mesher, registry, "Registry", "registry", "BlockRegistry", "blockRegistry");
                    return mesher;
                }

                Assert.Fail("GreedyChunkMesher should be constructible for tests.");
                return null;
            }

            public static object MeshChunk(object mesher, object chunk, BlockRegistry registry)
            {
                Type type = mesher.GetType();
                if (TryInvoke(type, mesher, AnyInstance, new[] { "BuildMesh", "GenerateMesh", "MeshChunk", "Mesh", "Build" }, out object result,
                    new object[] { chunk, registry },
                    new object[] { chunk },
                    new object[] { chunk, registry, new WorldDimensions(1, 1) }))
                {
                    Assert.That(result, Is.Not.Null, "GreedyChunkMesher should return mesh data.");
                    return result;
                }

                if (TryInvoke(type, null, AnyStatic, new[] { "BuildMesh", "GenerateMesh", "MeshChunk", "Mesh", "Build" }, out result,
                    new object[] { chunk, registry },
                    new object[] { chunk },
                    new object[] { chunk, registry, new WorldDimensions(1, 1) }))
                {
                    Assert.That(result, Is.Not.Null, "GreedyChunkMesher should return mesh data.");
                    return result;
                }

                Assert.Fail("GreedyChunkMesher should expose BuildMesh/GenerateMesh/MeshChunk returning a mesh or mesh data.");
                return null;
            }

            public static int CountQuads(object meshData)
            {
                Mesh mesh = meshData as Mesh;
                if (mesh != null)
                {
                    return CountMeshQuads(mesh);
                }

                object nestedMesh = TryGetMemberValue(meshData, "Mesh", "mesh", "UnityMesh", "unityMesh");
                mesh = nestedMesh as Mesh;
                if (mesh != null)
                {
                    return CountMeshQuads(mesh);
                }

                object quads = TryGetMemberValue(meshData, "Quads", "quads");
                if (quads != null)
                {
                    return CountCollection(quads);
                }

                object triangles = TryGetMemberValue(meshData, "Triangles", "triangles", "Indices", "indices");
                int triangleIndexCount = CountCollection(triangles);
                if (triangleIndexCount > 0)
                {
                    return triangleIndexCount / 6;
                }

                object vertices = TryGetMemberValue(meshData, "Vertices", "vertices");
                int vertexCount = CountCollection(vertices);
                if (vertexCount > 0)
                {
                    return vertexCount / 4;
                }

                Assert.Fail("Mesh result should expose a Unity Mesh, quads, triangles/indices, or vertices.");
                return 0;
            }

            public static void SetMemberValue(MemberInfo member, object target, object value)
            {
                FieldInfo field = member as FieldInfo;
                if (field != null)
                {
                    field.SetValue(target, ConvertArgument(value, field.FieldType));
                    return;
                }

                PropertyInfo property = member as PropertyInfo;
                if (property != null)
                {
                    property.SetValue(target, ConvertArgument(value, property.PropertyType), null);
                }
            }

            private static Type RequiredType(string simpleName)
            {
                string fullName = "ProjectArcane.WorldGeneration." + simpleName;
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    Type type = assemblies[i].GetType(fullName);
                    if (type != null)
                    {
                        return type;
                    }
                }

                Assert.Fail($"Expected runtime API type {fullName} to exist.");
                return null;
            }

            private static object CreateConfigLikeObject(Type type)
            {
                if (typeof(ScriptableObject).IsAssignableFrom(type))
                {
                    return ScriptableObject.CreateInstance(type);
                }

                return Activator.CreateInstance(type);
            }

            private static object CreatePeriodicNoiseInstance(Type type, object settings, WorldDimensions dimensions)
            {
                object instance;
                if (TryCreateInstance(type, out instance,
                    new object[] { settings, dimensions },
                    new object[] { dimensions, settings },
                    new object[] { settings, dimensions.WorldSizeX, dimensions.WorldSizeZ },
                    new object[] { TestSeed, dimensions.WorldSizeX, dimensions.WorldSizeZ },
                    new object[] { settings },
                    Array.Empty<object>()))
                {
                    TrySetMember(instance, settings, "Settings", "settings", "NoiseSettings", "noiseSettings");
                    TrySetMember(instance, dimensions, "Dimensions", "dimensions", "WorldDimensions", "worldDimensions");
                    return instance;
                }

                Assert.Fail("PeriodicNoise should be constructible with NoiseSettings and WorldDimensions, or expose static sampling.");
                return null;
            }

            private static bool TryCreateInstance(Type type, out object instance, params object[][] candidateArguments)
            {
                if (typeof(ScriptableObject).IsAssignableFrom(type))
                {
                    instance = ScriptableObject.CreateInstance(type);
                    return true;
                }

                for (int i = 0; i < candidateArguments.Length; i++)
                {
                    object[] converted;
                    ConstructorInfo constructor = FindConstructor(type, candidateArguments[i], out converted);
                    if (constructor != null)
                    {
                        instance = constructor.Invoke(converted);
                        return true;
                    }
                }

                instance = null;
                return false;
            }

            private static bool TryInvoke(Type type, object target, BindingFlags flags, string[] names, out object result, params object[][] candidateArguments)
            {
                for (int i = 0; i < candidateArguments.Length; i++)
                {
                    MethodInfo method = FindMethod(type, flags, names, candidateArguments[i]);
                    if (method == null)
                    {
                        continue;
                    }

                    result = Invoke(method, target, candidateArguments[i]);
                    return true;
                }

                result = null;
                return false;
            }

            private static object Invoke(MethodInfo method, object target, object[] arguments)
            {
                ParameterInfo[] parameters = method.GetParameters();
                object[] converted = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    converted[i] = ConvertArgument(arguments[i], parameters[i].ParameterType);
                }

                return method.Invoke(target, converted);
            }

            private static ConstructorInfo FindConstructor(Type type, object[] arguments, out object[] converted)
            {
                ConstructorInfo[] constructors = type.GetConstructors(AnyInstance);
                for (int i = 0; i < constructors.Length; i++)
                {
                    ParameterInfo[] parameters = constructors[i].GetParameters();
                    if (parameters.Length != arguments.Length)
                    {
                        continue;
                    }

                    if (TryConvertArguments(parameters, arguments, out converted))
                    {
                        return constructors[i];
                    }
                }

                converted = null;
                return null;
            }

            private static MethodInfo FindMethod(Type type, BindingFlags flags, string[] names, object[] arguments)
            {
                MethodInfo[] methods = type.GetMethods(flags);
                for (int i = 0; i < methods.Length; i++)
                {
                    if (!NameEqualsAny(methods[i].Name, names))
                    {
                        continue;
                    }

                    ParameterInfo[] parameters = methods[i].GetParameters();
                    if (parameters.Length != arguments.Length)
                    {
                        continue;
                    }

                    object[] converted;
                    if (TryConvertArguments(parameters, arguments, out converted))
                    {
                        return methods[i];
                    }
                }

                return null;
            }

            private static bool TryConvertArguments(ParameterInfo[] parameters, object[] arguments, out object[] converted)
            {
                converted = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!CanConvertArgument(arguments[i], parameters[i].ParameterType))
                    {
                        converted = null;
                        return false;
                    }

                    converted[i] = ConvertArgument(arguments[i], parameters[i].ParameterType);
                }

                return true;
            }

            private static bool CanConvertArgument(object value, Type targetType)
            {
                if (value == null)
                {
                    return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;
                }

                Type valueType = value.GetType();
                if (targetType.IsAssignableFrom(valueType))
                {
                    return true;
                }

                Type underlyingTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;
                if (underlyingTarget.IsEnum)
                {
                    return value is string || IsNumeric(value);
                }

                return IsNumeric(value) && IsNumericType(underlyingTarget);
            }

            private static object ConvertArgument(object value, Type targetType)
            {
                if (value == null)
                {
                    return null;
                }

                Type valueType = value.GetType();
                if (targetType.IsAssignableFrom(valueType))
                {
                    return value;
                }

                Type underlyingTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;
                if (underlyingTarget.IsEnum)
                {
                    if (value is string)
                    {
                        return Enum.Parse(underlyingTarget, (string)value);
                    }

                    return Enum.ToObject(underlyingTarget, value);
                }

                return Convert.ChangeType(value, underlyingTarget);
            }

            private static bool TrySetMember(object target, object value, params string[] names)
            {
                if (target == null)
                {
                    return false;
                }

                Type type = target.GetType();
                foreach (MemberInfo member in GetWritableMembers(type))
                {
                    if (!NameEqualsAny(member.Name, names))
                    {
                        continue;
                    }

                    Type memberType = GetMemberType(member);
                    if (!CanConvertArgument(value, memberType))
                    {
                        continue;
                    }

                    SetMemberValue(member, target, value);
                    return true;
                }

                return false;
            }

            private static object TryGetMemberValue(object target, params string[] names)
            {
                if (target == null)
                {
                    return null;
                }

                Type type = target.GetType();
                for (int i = 0; i < names.Length; i++)
                {
                    FieldInfo field = type.GetField(names[i], AnyInstance);
                    if (field != null)
                    {
                        return field.GetValue(target);
                    }

                    PropertyInfo property = type.GetProperty(names[i], AnyInstance);
                    if (property != null && property.GetIndexParameters().Length == 0)
                    {
                        return property.GetValue(target, null);
                    }
                }

                return null;
            }

            private static IEnumerable<MemberInfo> GetWritableMembers(Type type)
            {
                FieldInfo[] fields = type.GetFields(AnyInstance);
                for (int i = 0; i < fields.Length; i++)
                {
                    if (!fields[i].IsInitOnly)
                    {
                        yield return fields[i];
                    }
                }

                PropertyInfo[] properties = type.GetProperties(AnyInstance);
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].CanWrite && properties[i].GetIndexParameters().Length == 0)
                    {
                        yield return properties[i];
                    }
                }
            }

            private static Type GetMemberType(MemberInfo member)
            {
                FieldInfo field = member as FieldInfo;
                if (field != null)
                {
                    return field.FieldType;
                }

                return ((PropertyInfo)member).PropertyType;
            }

            private static bool TryHashStoredCollections(object chunk, out ChunkFingerprint fingerprint)
            {
                long hash = 1469598103934665603L;
                int count = 0;

                foreach (MemberInfo member in GetReadableMembers(chunk.GetType()))
                {
                    string memberName = member.Name.ToLowerInvariant();
                    if (!memberName.Contains("cell") && !memberName.Contains("voxel") && !memberName.Contains("block"))
                    {
                        continue;
                    }

                    object value = GetMemberValue(member, chunk);
                    if (value == null || value is string)
                    {
                        continue;
                    }

                    if (value is IEnumerable enumerable)
                    {
                        foreach (object item in enumerable)
                        {
                            AddHash(ref hash, HashValue(item));
                            count++;
                        }
                    }
                }

                fingerprint = new ChunkFingerprint(hash, count);
                return count > 0;
            }

            private static IEnumerable<MemberInfo> GetReadableMembers(Type type)
            {
                FieldInfo[] fields = type.GetFields(AnyInstance);
                for (int i = 0; i < fields.Length; i++)
                {
                    yield return fields[i];
                }

                PropertyInfo[] properties = type.GetProperties(AnyInstance);
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].CanRead && properties[i].GetIndexParameters().Length == 0)
                    {
                        yield return properties[i];
                    }
                }
            }

            private static object GetMemberValue(MemberInfo member, object target)
            {
                FieldInfo field = member as FieldInfo;
                if (field != null)
                {
                    return field.GetValue(target);
                }

                return ((PropertyInfo)member).GetValue(target, null);
            }

            private static bool TrySetIndexedStorage(object chunk, VoxelCoord coord, CellState cell)
            {
                foreach (MemberInfo member in GetReadableMembers(chunk.GetType()))
                {
                    string memberName = member.Name.ToLowerInvariant();
                    if (!memberName.Contains("cell") && !memberName.Contains("voxel") && !memberName.Contains("block"))
                    {
                        continue;
                    }

                    Array array = GetMemberValue(member, chunk) as Array;
                    if (array == null)
                    {
                        continue;
                    }

                    int flatIndex = coord.X + WorldDimensions.ChunkSizeX * (coord.Z + WorldDimensions.ChunkSizeZ * coord.Y);
                    if (flatIndex < 0 || flatIndex >= array.Length)
                    {
                        continue;
                    }

                    Type elementType = array.GetType().GetElementType();
                    if (elementType == typeof(CellState))
                    {
                        array.SetValue(cell, flatIndex);
                        return true;
                    }

                    if (elementType == typeof(int))
                    {
                        array.SetValue(cell.BlockId, flatIndex);
                        return true;
                    }

                    if (CanConvertArgument(cell.BlockId, elementType))
                    {
                        array.SetValue(ConvertArgument(cell.BlockId, elementType), flatIndex);
                        return true;
                    }
                }

                return false;
            }

            private static int CountMeshQuads(Mesh mesh)
            {
                if (mesh.triangles != null && mesh.triangles.Length > 0)
                {
                    return mesh.triangles.Length / 6;
                }

                return mesh.vertexCount / 4;
            }

            private static int CountCollection(object value)
            {
                if (value == null || value is string)
                {
                    return 0;
                }

                ICollection collection = value as ICollection;
                if (collection != null)
                {
                    return collection.Count;
                }

                Array array = value as Array;
                if (array != null)
                {
                    return array.Length;
                }

                IEnumerable enumerable = value as IEnumerable;
                if (enumerable == null)
                {
                    return 0;
                }

                int count = 0;
                foreach (object ignored in enumerable)
                {
                    count++;
                }

                return count;
            }

            private static int HashValue(object value)
            {
                if (value == null)
                {
                    return 0;
                }

                if (value is CellState)
                {
                    return value.GetHashCode();
                }

                Type type = value.GetType();
                if (type.IsPrimitive || type.IsEnum || value is string)
                {
                    return value.GetHashCode();
                }

                object blockId = TryGetMemberValue(value, "BlockId", "blockId", "StableId", "stableId");
                object layer = TryGetMemberValue(value, "Layer", "layer");
                object flags = TryGetMemberValue(value, "Flags", "flags");
                if (blockId != null || layer != null || flags != null)
                {
                    unchecked
                    {
                        int hash = 17;
                        hash = (hash * 397) ^ (blockId != null ? blockId.GetHashCode() : 0);
                        hash = (hash * 397) ^ (layer != null ? layer.GetHashCode() : 0);
                        hash = (hash * 397) ^ (flags != null ? flags.GetHashCode() : 0);
                        return hash;
                    }
                }

                return value.GetHashCode();
            }

            private static void AddHash(ref long hash, int value)
            {
                unchecked
                {
                    hash ^= value;
                    hash *= 1099511628211L;
                }
            }

            private static bool NameEqualsAny(string name, string[] expected)
            {
                for (int i = 0; i < expected.Length; i++)
                {
                    if (string.Equals(name, expected[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }

            private static bool NameContainsAny(string name, string[] words)
            {
                for (int i = 0; i < words.Length; i++)
                {
                    if (name.IndexOf(words[i], StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            private static bool IsNumeric(object value)
            {
                return IsNumericType(value.GetType());
            }

            private static bool IsNumericType(Type type)
            {
                TypeCode code = Type.GetTypeCode(type);
                return code == TypeCode.Byte
                    || code == TypeCode.SByte
                    || code == TypeCode.Int16
                    || code == TypeCode.UInt16
                    || code == TypeCode.Int32
                    || code == TypeCode.UInt32
                    || code == TypeCode.Int64
                    || code == TypeCode.UInt64
                    || code == TypeCode.Single
                    || code == TypeCode.Double
                    || code == TypeCode.Decimal;
            }

            private static string FormatArguments(object[] arguments)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(arguments[i] == null ? "null" : arguments[i].GetType().Name);
                }

                return builder.ToString();
            }
        }
    }
}
