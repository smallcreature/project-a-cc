using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ProjectArcane.WorldGeneration;
using Object = UnityEngine.Object;

namespace ProjectArcane.WorldGeneration.Editor
{
    public sealed class WorldGeneratorWindow : EditorWindow
    {
        private const string ControllerTypeName = "ProjectArcane.WorldGeneration.WorldGenerationController";
        private const string ConfigTypeName = "ProjectArcane.WorldGeneration.WorldGeneratorConfig";
        private const string RegistryTypeName = "ProjectArcane.WorldGeneration.BlockRegistry";
        private const string NoiseSettingsTypeName = "ProjectArcane.WorldGeneration.NoiseSettings";
        private const string PreviewGeneratorTypeName = "ProjectArcane.WorldGeneration.WorldPreviewGenerator";

        private ObjectField controllerField;
        private ObjectField configField;
        private ObjectField registryField;
        private ObjectField materialField;
        private IntegerField centerChunkXField;
        private IntegerField centerChunkZField;
        private IntegerField debugRadiusField;
        private IntegerField debugYMinField;
        private IntegerField debugYMaxField;
        private IntegerField renderSectionHeightField;
        private IntegerField previewSizeField;
        private IntegerField previewAuxField;
        private Button generateButton;
        private Button clearButton;
        private Button debugOnButton;
        private Button debugOffButton;
        private Button rebuildMeshesButton;
        private Button refreshPreviewsButton;
        private Label statusLabel;
        private Image biomeMapImage;
        private Image noiseImage;
        private Image combinedImage;
        private Image caveImage;
        private Texture2D biomeMapPreview;
        private Texture2D noisePreview;
        private Texture2D combinedPreview;
        private Texture2D cavePreview;
        private VisualElement detailedNoisePreviews;
        private readonly List<Texture2D> detailedNoiseTextures = new List<Texture2D>();
        private Type controllerType;
        private Type configType;
        private Type registryType;
        private Type noiseSettingsType;
        private Type previewGeneratorType;

        [MenuItem("Tools/Project Arcane/World Generator")]
        public static void Open()
        {
            WorldGeneratorWindow window = GetWindow<WorldGeneratorWindow>();
            window.titleContent = new GUIContent("World Generator");
            window.minSize = new Vector2(540f, 640f);
        }

        public void CreateGUI()
        {
            ResolveTypes();

            ScrollView root = new ScrollView();
            root.style.paddingLeft = 10f;
            root.style.paddingRight = 10f;
            root.style.paddingTop = 8f;
            root.style.paddingBottom = 10f;
            root.style.flexGrow = 1f;
            rootVisualElement.Add(root);

            Label title = new Label("World Generator");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 18f;
            title.style.marginBottom = 8f;
            root.Add(title);

            statusLabel = new Label();
            statusLabel.style.whiteSpace = WhiteSpace.Normal;
            statusLabel.style.marginBottom = 8f;
            root.Add(statusLabel);

            controllerField = NewObjectField("Controller", controllerType, typeof(MonoBehaviour), true);
            configField = NewObjectField("Config", configType, typeof(ScriptableObject), false);
            registryField = NewObjectField("Block Registry", registryType, typeof(ScriptableObject), false);
            materialField = NewObjectField("Debug Material", typeof(Material), typeof(Material), false);

            controllerField.RegisterValueChangedCallback(_ =>
            {
                PullFromController();
                RefreshState();
            });
            configField.RegisterValueChangedCallback(_ => RefreshState());
            registryField.RegisterValueChangedCallback(_ => RefreshState());
            materialField.RegisterValueChangedCallback(_ => RefreshState());

            root.Add(controllerField);
            root.Add(configField);
            root.Add(registryField);
            root.Add(materialField);

            VisualElement numbers = new VisualElement();
            numbers.style.flexDirection = FlexDirection.Row;
            numbers.style.flexWrap = Wrap.Wrap;
            numbers.style.marginTop = 6f;
            root.Add(numbers);

            centerChunkXField = NewIntField("Center X", 0, numbers);
            centerChunkZField = NewIntField("Center Z", 0, numbers);
            debugRadiusField = NewIntField("Radius", 2, numbers);
            debugYMinField = NewIntField("Y Min", 0, numbers);
            debugYMaxField = NewIntField("Y Max", 64, numbers);
            renderSectionHeightField = NewIntField("Section Height", 16, numbers);

            VisualElement buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.flexWrap = Wrap.Wrap;
            buttons.style.marginTop = 8f;
            buttons.style.marginBottom = 10f;
            root.Add(buttons);

            generateButton = NewButton("Generate", () => InvokeControllerMethod("GenerateWorld"), buttons);
            clearButton = NewButton("Clear", () => InvokeControllerMethod("ClearWorld"), buttons);
            debugOnButton = NewButton("Debug On", () => InvokeControllerMethod("SetDebugVisible", true), buttons);
            debugOffButton = NewButton("Debug Off", () => InvokeControllerMethod("SetDebugVisible", false), buttons);
            rebuildMeshesButton = NewButton("Rebuild Meshes", () => InvokeControllerMethod("RebuildDebugMeshes"), buttons);
            refreshPreviewsButton = NewButton("Refresh Previews", RefreshPreviews, buttons);

            VisualElement previewControls = new VisualElement();
            previewControls.style.flexDirection = FlexDirection.Row;
            previewControls.style.flexWrap = Wrap.Wrap;
            previewControls.style.marginBottom = 6f;
            root.Add(previewControls);
            previewSizeField = NewIntField("Preview Size", 256, previewControls);
            previewAuxField = NewIntField("Preview Aux", 0, previewControls);

            VisualElement previews = new VisualElement();
            previews.style.flexDirection = FlexDirection.Row;
            previews.style.flexWrap = Wrap.Wrap;
            root.Add(previews);

            biomeMapImage = NewPreview("Biome Map", previews);
            noiseImage = NewPreview("Noise", previews);
            combinedImage = NewPreview("Combined", previews);
            caveImage = NewPreview("Caves", previews);

            Label detailedTitle = new Label("Default Biome Noise Previews");
            detailedTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            detailedTitle.style.marginTop = 8f;
            detailedTitle.style.marginBottom = 4f;
            root.Add(detailedTitle);

            detailedNoisePreviews = new VisualElement();
            detailedNoisePreviews.style.flexDirection = FlexDirection.Row;
            detailedNoisePreviews.style.flexWrap = Wrap.Wrap;
            root.Add(detailedNoisePreviews);

            PullFromSelection();
            PullFromController();
            RefreshState();
        }

        private void OnDisable()
        {
            DestroyPreview(ref biomeMapPreview);
            DestroyPreview(ref noisePreview);
            DestroyPreview(ref combinedPreview);
            DestroyPreview(ref cavePreview);
            ClearDetailedNoisePreviews();
        }

        private void ResolveTypes()
        {
            controllerType = FindType(ControllerTypeName);
            configType = FindType(ConfigTypeName);
            registryType = FindType(RegistryTypeName);
            noiseSettingsType = FindType(NoiseSettingsTypeName);
            previewGeneratorType = FindType(PreviewGeneratorTypeName);
        }

        private ObjectField NewObjectField(string label, Type preferredType, Type fallbackType, bool allowSceneObjects)
        {
            return new ObjectField(label)
            {
                objectType = preferredType ?? fallbackType,
                allowSceneObjects = allowSceneObjects
            };
        }

        private IntegerField NewIntField(string label, int value, VisualElement parent)
        {
            IntegerField field = new IntegerField(label) { value = value };
            field.style.minWidth = 150f;
            field.style.marginRight = 8f;
            field.style.marginBottom = 4f;
            parent.Add(field);
            return field;
        }

        private Button NewButton(string text, Action action, VisualElement parent)
        {
            Button button = new Button(action) { text = text };
            button.style.marginRight = 6f;
            button.style.marginBottom = 6f;
            parent.Add(button);
            return button;
        }

        private Image NewPreview(string label, VisualElement parent)
        {
            VisualElement frame = new VisualElement();
            frame.style.width = 250f;
            frame.style.marginRight = 8f;
            frame.style.marginBottom = 10f;
            parent.Add(frame);

            Label title = new Label(label);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 3f;
            frame.Add(title);

            Image image = new Image { scaleMode = ScaleMode.ScaleToFit };
            image.style.width = 250f;
            image.style.height = 250f;
            image.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f, 1f);
            frame.Add(image);
            return image;
        }

        private void PullFromSelection()
        {
            Object active = Selection.activeObject;
            if (controllerField.value == null && IsInstanceOf(active, controllerType))
                controllerField.SetValueWithoutNotify(active);
        }

        private void PullFromController()
        {
            Object controller = controllerField.value;
            if (controller == null)
                return;

            SetObjectField(configField, GetMemberAsObject(controller, "Config"));
            SetObjectField(registryField, GetMemberAsObject(controller, "BlockRegistry"));
            SetObjectField(materialField, GetMemberAsObject(controller, "DebugMaterial"));
            centerChunkXField?.SetValueWithoutNotify(GetMemberAsInt(controller, "CenterChunkX", centerChunkXField.value));
            centerChunkZField?.SetValueWithoutNotify(GetMemberAsInt(controller, "CenterChunkZ", centerChunkZField.value));
            debugRadiusField?.SetValueWithoutNotify(GetMemberAsInt(controller, "DebugRadius", debugRadiusField.value));
            debugYMinField?.SetValueWithoutNotify(GetMemberAsInt(controller, "DebugYMin", debugYMinField.value));
            debugYMaxField?.SetValueWithoutNotify(GetMemberAsInt(controller, "DebugYMax", debugYMaxField.value));
            renderSectionHeightField?.SetValueWithoutNotify(GetMemberAsInt(controller, "RenderSectionHeight", renderSectionHeightField.value));
        }

        private void ApplyToController()
        {
            Object controller = controllerField.value;
            if (controller == null)
                return;

            int radius = Mathf.Max(0, debugRadiusField.value);
            int yMin = debugYMinField.value;
            int yMax = Mathf.Max(yMin, debugYMaxField.value);
            int sectionHeight = Mathf.Max(1, renderSectionHeightField.value);
            debugRadiusField.SetValueWithoutNotify(radius);
            debugYMaxField.SetValueWithoutNotify(yMax);
            renderSectionHeightField.SetValueWithoutNotify(sectionHeight);

            Undo.RecordObject(controller, "Configure World Generator");
            SetMember(controller, "Config", configField.value);
            SetMember(controller, "BlockRegistry", registryField.value);
            SetMember(controller, "DebugMaterial", materialField.value);
            SetMember(controller, "CenterChunkX", centerChunkXField.value);
            SetMember(controller, "CenterChunkZ", centerChunkZField.value);
            SetMember(controller, "DebugRadius", radius);
            SetMember(controller, "DebugYMin", yMin);
            SetMember(controller, "DebugYMax", yMax);
            SetMember(controller, "RenderSectionHeight", sectionHeight);
            EditorUtility.SetDirty(controller);
        }

        private void InvokeControllerMethod(string methodName, params object[] arguments)
        {
            Object controller = controllerField.value;
            if (controller == null)
            {
                SetStatus("Assign a WorldGenerationController before using runtime actions.");
                return;
            }

            ApplyToController();
            MethodInfo method = controller.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            if (method == null)
            {
                SetStatus($"{methodName} is not available on the assigned controller.");
                return;
            }

            try
            {
                method.Invoke(controller, arguments);
                SetStatus($"{methodName} completed.");
            }
            catch (TargetInvocationException exception)
            {
                Debug.LogException(exception.InnerException ?? exception);
                SetStatus($"{methodName} failed. See Console for details.");
            }
        }

        private void RefreshPreviews()
        {
            Object config = configField.value;
            if (config == null)
            {
                SetStatus("Assign a WorldGeneratorConfig before refreshing previews.");
                return;
            }

            ResolveTypes();
            if (previewGeneratorType == null)
            {
                SetStatus("WorldPreviewGenerator is not available yet.");
                return;
            }

            int size = Mathf.Clamp(previewSizeField.value, 32, 1024);
            previewSizeField.SetValueWithoutNotify(size);
            DestroyPreview(ref biomeMapPreview);
            DestroyPreview(ref noisePreview);
            DestroyPreview(ref combinedPreview);
            DestroyPreview(ref cavePreview);
            ClearDetailedNoisePreviews();

            WorldGeneratorConfig typedConfig = config as WorldGeneratorConfig;
            if (typedConfig != null)
            {
                BiomeConfig defaultBiome = typedConfig.DefaultBiome;
                biomeMapPreview = WorldPreviewGenerator.CreateBiomeMapPreview(typedConfig, size);
                noisePreview = defaultBiome == null ? null : WorldPreviewGenerator.CreateNoisePreview(defaultBiome.HeightNoise, typedConfig, size, 0);
                combinedPreview = WorldPreviewGenerator.CreateBiomeCombinedPreview(typedConfig, 0, size);
                cavePreview = WorldPreviewGenerator.CreateCavePreview(typedConfig, size, previewAuxField.value);
                if (defaultBiome != null)
                {
                    AddDetailedNoisePreview("Biome Mask", defaultBiome.MapNoise, typedConfig, size, 11);
                    AddDetailedNoisePreview("Height", defaultBiome.HeightNoise, typedConfig, size, 101);
                    AddDetailedNoisePreview("Continentalness", defaultBiome.ContinentalnessNoise, typedConfig, size, 151);
                    AddDetailedNoisePreview("Erosion", defaultBiome.ErosionNoise, typedConfig, size, 211);
                    AddDetailedNoisePreview("Peaks & Valleys", defaultBiome.PeaksAndValleysNoise, typedConfig, size, 271);
                    AddDetailedNoisePreview("Cliffs XZ Slice", defaultBiome.CliffsAndOverhangsNoise, typedConfig, size, 331);
                    if (defaultBiome.Caves != null)
                    {
                        AddDetailedNoisePreview("Cheese Caves", defaultBiome.Caves.CheeseNoise, typedConfig, size, 401);
                        AddDetailedNoisePreview("Spaghetti Caves", defaultBiome.Caves.SpaghettiNoise, typedConfig, size, 503);
                    }
                }
            }
            else
            {
                biomeMapPreview = InvokePreview("CreateBiomeMapPreview", config);
                noisePreview = InvokePreview("CreateNoisePreview", config);
                combinedPreview = InvokePreview("CreateBiomeCombinedPreview", config);
                cavePreview = InvokePreview("CreateCavePreview", config);
            }

            biomeMapImage.image = biomeMapPreview;
            noiseImage.image = noisePreview;
            combinedImage.image = combinedPreview;
            caveImage.image = cavePreview;
            SetStatus("Previews refreshed.");
        }

        private void AddDetailedNoisePreview(string label, NoiseSettings settings, WorldGeneratorConfig config, int size, int seedOffset)
        {
            if (settings == null || detailedNoisePreviews == null)
            {
                return;
            }

            Image image = NewPreview(label, detailedNoisePreviews);
            Texture2D texture = WorldPreviewGenerator.CreateNoisePreview(settings, config, size, seedOffset);
            detailedNoiseTextures.Add(texture);
            image.image = texture;
        }

        private void ClearDetailedNoisePreviews()
        {
            for (int i = 0; i < detailedNoiseTextures.Count; i++)
            {
                Texture2D texture = detailedNoiseTextures[i];
                if (texture != null)
                {
                    DestroyImmediate(texture);
                }
            }

            detailedNoiseTextures.Clear();
            detailedNoisePreviews?.Clear();
        }

        private Texture2D InvokePreview(string methodName, Object config)
        {
            MethodInfo method = FindPreviewMethod(methodName);
            if (method == null)
                return null;

            object[] arguments = BuildPreviewArguments(method, config);
            if (arguments == null)
                return null;

            try
            {
                return method.Invoke(null, arguments) as Texture2D;
            }
            catch (TargetInvocationException exception)
            {
                Debug.LogException(exception.InnerException ?? exception);
                return null;
            }
        }

        private MethodInfo FindPreviewMethod(string methodName)
        {
            MethodInfo[] methods = previewGeneratorType.GetMethods(BindingFlags.Static | BindingFlags.Public);
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name == methodName)
                    return methods[i];
            }

            return null;
        }

        private object[] BuildPreviewArguments(MethodInfo method, Object config)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] arguments = new object[parameters.Length];
            int intIndex = 0;

            for (int i = 0; i < parameters.Length; i++)
            {
                Type parameterType = parameters[i].ParameterType;
                if (parameterType == typeof(int))
                {
                    arguments[i] = GetPreviewInt(parameters[i], intIndex++);
                    continue;
                }

                if (parameterType.IsInstanceOfType(config))
                {
                    arguments[i] = config;
                    continue;
                }

                if (noiseSettingsType != null && parameterType == noiseSettingsType)
                {
                    arguments[i] = FindNoiseSettings(config);
                    if (arguments[i] != null)
                        continue;
                }

                return null;
            }

            return arguments;
        }

        private int GetPreviewInt(ParameterInfo parameter, int index)
        {
            string name = parameter.Name?.ToLowerInvariant() ?? string.Empty;
            if (name.Contains("size") || name.Contains("width") || name.Contains("height") || name.Contains("resolution"))
                return previewSizeField.value;

            if (name.Contains("seed") || name.Contains("slice") || name == "y" || name.Contains("ylevel"))
                return previewAuxField.value;

            return index == 0 ? previewSizeField.value : previewAuxField.value;
        }

        private object FindNoiseSettings(Object config)
        {
            if (noiseSettingsType == null || config == null)
                return null;

            Type type = config.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].GetIndexParameters().Length == 0 && noiseSettingsType.IsAssignableFrom(properties[i].PropertyType))
                    return properties[i].GetValue(config);
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < fields.Length; i++)
            {
                if (noiseSettingsType.IsAssignableFrom(fields[i].FieldType))
                    return fields[i].GetValue(config);
            }

            return null;
        }

        private void RefreshState()
        {
            ResolveTypes();
            bool hasController = controllerField.value != null;
            bool hasConfig = configField.value != null;
            generateButton.SetEnabled(hasController);
            clearButton.SetEnabled(hasController);
            debugOnButton.SetEnabled(hasController);
            debugOffButton.SetEnabled(hasController);
            rebuildMeshesButton.SetEnabled(hasController);
            refreshPreviewsButton.SetEnabled(hasConfig && previewGeneratorType != null);

            if (!hasController)
                SetStatus("Assign a controller for generation actions. Previews only need a config.");
            else if (!hasConfig)
                SetStatus("Controller assigned. Assign a config for generation and previews.");
            else if (previewGeneratorType == null)
                SetStatus("Controller and config assigned. Preview API is not available yet.");
            else
                SetStatus("Ready.");
        }

        private void SetStatus(string message)
        {
            if (statusLabel != null)
                statusLabel.text = message;
        }

        private static void SetObjectField(ObjectField field, Object value)
        {
            field.SetValueWithoutNotify(value);
        }

        private static Object GetMemberAsObject(Object target, string name)
        {
            object value = GetMember(target, name);
            return value as Object;
        }

        private static int GetMemberAsInt(Object target, string name, int fallback)
        {
            object value = GetMember(target, name);
            if (value == null)
                return fallback;

            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        private static object GetMember(Object target, string name)
        {
            if (target == null)
                return null;

            Type type = target.GetType();
            PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (property != null && property.CanRead && property.GetIndexParameters().Length == 0)
                return property.GetValue(target);

            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            return field?.GetValue(target);
        }

        private static void SetMember(Object target, string name, object value)
        {
            if (target == null)
                return;

            Type type = target.GetType();
            PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (property != null && property.CanWrite && CanAssign(property.PropertyType, value))
            {
                property.SetValue(target, ConvertValue(property.PropertyType, value));
                return;
            }

            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field != null && CanAssign(field.FieldType, value))
                field.SetValue(target, ConvertValue(field.FieldType, value));
        }

        private static bool CanAssign(Type targetType, object value)
        {
            if (value == null)
                return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;

            if (targetType.IsInstanceOfType(value))
                return true;

            return targetType.IsPrimitive && value is IConvertible;
        }

        private static object ConvertValue(Type targetType, object value)
        {
            if (value == null || targetType.IsInstanceOfType(value))
                return value;

            return Convert.ChangeType(value, targetType);
        }

        private static bool IsInstanceOf(Object value, Type type)
        {
            return value != null && type != null && type.IsInstanceOfType(value);
        }

        private static Type FindType(string fullName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type type = assemblies[i].GetType(fullName);
                if (type != null)
                    return type;
            }

            return null;
        }

        private static void DestroyPreview(ref Texture2D texture)
        {
            if (texture != null)
                DestroyImmediate(texture);
            texture = null;
        }
    }
}
