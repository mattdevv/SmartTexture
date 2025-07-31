using System;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SmartTexture.Editor
{
    [CustomEditor(typeof(SmartTextureImporter), true)]
    [CanEditMultipleObjects]
    public class SmartTextureImporterEditor : ScriptedImporterEditor
    {
        internal static class Styles
        {
            public static readonly GUIContent[] labelChannels =
            {
                EditorGUIUtility.TrTextContent("Red Channel",   "This texture source channel will be packed into the output texture red channel"),
                EditorGUIUtility.TrTextContent("Green Channel", "This texture source channel will be packed into the output texture green channel"),
                EditorGUIUtility.TrTextContent("Blue Channel",  "This texture source channel will be packed into the output texture blue channel"),
                EditorGUIUtility.TrTextContent("Alpha Channel", "This texture source channel will be packed into the output texture alpha channel"),
            };

            public static readonly GUIContent invertColor =
                EditorGUIUtility.TrTextContent("Invert Color", "If enabled outputs the inverted color (1.0 - color)");

            public static readonly GUIContent channelSource =
                EditorGUIUtility.TrTextContent("Channel", "Choose which channel will be used from the source texture");

            public static readonly GUIContent[] channelSourceLabels =
            {
                EditorGUIUtility.TrTextContent("R", "Use source's red channel"),
                EditorGUIUtility.TrTextContent("G", "Use source's green channel"),
                EditorGUIUtility.TrTextContent("B", "Use source's blue channel"),
                EditorGUIUtility.TrTextContent("A", "Use source's alpha channel"),
            };

            public static readonly GUIContent resolutionMode = EditorGUIUtility.TrTextContent("Resolution Mode",
                "Choose how the output texture's resolution is determined.");

            public static readonly GUIContent[] resolutionModeLabels =
            {
                EditorGUIUtility.TrTextContent("Maximum", "Use maximum length found for each dimension"),
                EditorGUIUtility.TrTextContent("Minimum", "Use minimum length found for each dimension"),
                EditorGUIUtility.TrTextContent("Explicit", "Use provided dimensions as an exact resolution"),
            };

            public static readonly GUIContent targetResolution =
                EditorGUIUtility.TrTextContent("Target Resolution", "Exact resolution to use for output texture.");

            public static readonly GUIContent alphaIsTransparency =
                EditorGUIUtility.TrTextContent("Use alpha as transparency",
                    "Set to indicate that the texture's alpha channel should be used as a transparency mask.");

            public static readonly GUIContent readWrite = EditorGUIUtility.TrTextContent("Read/Write Enabled",
                "Enable to be able to access the raw pixel data from code.");

            public static readonly GUIContent generateMipMaps = EditorGUIUtility.TrTextContent("Generate Mip Maps");
            public static readonly GUIContent streamingMipMaps = EditorGUIUtility.TrTextContent("Streaming Mip Maps");

            public static readonly GUIContent streamingMipMapsPrio =
                EditorGUIUtility.TrTextContent("Streaming Mip Maps Priority");

            public static readonly GUIContent sRGBTexture = EditorGUIUtility.TrTextContent("sRGB (Color Texture)",
                "Texture content is stored in gamma space. Non-HDR color textures should enable this flag (except if used for IMGUI).");

            public static readonly GUIContent textureFilterMode = EditorGUIUtility.TrTextContent("Filter Mode");
            public static readonly GUIContent textureWrapMode = EditorGUIUtility.TrTextContent("Wrap Mode");

            public static readonly GUIContent textureAnisotropicLevel =
                EditorGUIUtility.TrTextContent("Anisotropic Level");

            public static readonly GUIContent compressionLevel = EditorGUIUtility.TrTextContent("Compression Quality",
                "Changes balance between image quality and file size.");

            public static readonly GUIContent crunchCompression =
                EditorGUIUtility.TrTextContent("Use Crunch Compression");

            public static readonly GUIContent crunchQuality = EditorGUIUtility.TrTextContent("Crunch Quality",
                "Changes amount of crunch compression used:\nHigher = more quality but larger file size\nLower = less quality but smaller file size");

            public static readonly GUIContent useExplicitTextureFormat =
                EditorGUIUtility.TrTextContent("Use Explicit Texture Format");

            public static readonly string[] textureSizeOptions =
            {
                "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192", "16384",
            };

            public static readonly string[] textureCompressionOptions =
                {"None", "Balanced", "High Quality", "Low Quality"};

            public static readonly string[] textureFormat = Enum.GetNames(typeof(TextureFormat));
            public static readonly string[] resizeAlgorithmOptions = Enum.GetNames(typeof(TextureResizeAlgorithm));
        }
        
        
        private bool isMultiEditing;

        SerializedProperty[] m_InputTextures = new SerializedProperty[4];
        SerializedProperty[] m_InputTextureSettings = new SerializedProperty[4];

        SerializedProperty m_ResolutionMode;
        SerializedProperty m_TargetResolution;

        SerializedProperty m_AlphaIsTransparency;
        SerializedProperty m_IsReadableProperty;
        SerializedProperty m_sRGBTextureProperty;

        SerializedProperty m_EnableMipMapProperty;
        SerializedProperty m_StreamingMipMaps;
        SerializedProperty m_StreamingMipMapPriority;

        SerializedProperty m_FilterModeProperty;
        SerializedProperty m_WrapModeProperty;
        SerializedProperty m_AnisotropicLevelPropery;

        SerializedProperty m_TexturePlatformSettingsProperty;

        SerializedProperty m_TextureFormat;
        SerializedProperty m_UseExplicitTextureFormat;

        bool m_showInputTextures = true;
        bool m_showOutputTexture = true;
        bool m_showTextureSettings = true;

        const string k_AdvancedTextureSettingName = "SmartTextureImporterShowAdvanced";

        public override void OnEnable()
        {
            base.OnEnable();
            CacheSerializedProperties();
        }
        

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            m_showInputTextures = EditorGUILayout.Foldout(m_showInputTextures, "Input Textures", EditorStyles.foldoutHeader);
            if (m_showInputTextures)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawInputTexture(0);
                    DrawInputTexture(1);
                    DrawInputTexture(2);
                    DrawInputTexture(3, true);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            m_showOutputTexture = EditorGUILayout.Foldout(m_showOutputTexture, "Output Texture", EditorStyles.foldoutHeader);
            if (m_showOutputTexture)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    using (new EditorGUI.MixedValueScope(m_ResolutionMode.hasMultipleDifferentValues))
                    {
                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            int resolutionMode = EditorGUILayout.EnumPopup(Styles.resolutionMode, (SmartTextureImporter.TextureSizeMode) m_ResolutionMode.intValue).GetHashCode();
                            if (check.changed) m_ResolutionMode.intValue = resolutionMode;
                        }
                    }
                    
                    using (new EditorGUI.DisabledScope(m_ResolutionMode.hasMultipleDifferentValues || m_ResolutionMode.intValue != (int) SmartTextureImporter.TextureSizeMode.Explicit))
                    {
                        using (new EditorGUI.MixedValueScope(m_TargetResolution.hasMultipleDifferentValues))
                        {
                            using (var check = new EditorGUI.ChangeCheckScope())
                            {
                                Vector2Int changedResolution = EditorGUILayout.Vector2IntField(Styles.targetResolution, m_TargetResolution.vector2IntValue);
                                changedResolution.x = Mathf.Max(changedResolution.x, 1);
                                changedResolution.y = Mathf.Max(changedResolution.y, 1);
                                if (check.changed) m_TargetResolution.vector2IntValue = changedResolution;
                            }
                        }
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(m_AlphaIsTransparency, Styles.alphaIsTransparency);
                    EditorGUILayout.PropertyField(m_sRGBTextureProperty, Styles.sRGBTexture);
                    EditorGUILayout.PropertyField(m_IsReadableProperty, Styles.readWrite);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(m_FilterModeProperty, Styles.textureFilterMode);
                    EditorGUILayout.PropertyField(m_WrapModeProperty, Styles.textureWrapMode);
                    EditorGUILayout.IntSlider(m_AnisotropicLevelPropery, 0, 16, Styles.textureAnisotropicLevel);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(m_EnableMipMapProperty, Styles.generateMipMaps);
                    EditorGUILayout.PropertyField(m_StreamingMipMaps, Styles.streamingMipMaps);
                    EditorGUILayout.PropertyField(m_StreamingMipMapPriority, Styles.streamingMipMapsPrio);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            m_showTextureSettings = EditorGUILayout.Foldout(m_showTextureSettings, "Texture Platform Settings", EditorStyles.foldoutHeader);
            if (m_showTextureSettings)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawTextureImporterSettings();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
        }

        void DrawInputTexture(int index, bool isAlpha = false)
        {
            if (index < 0 || index >= 4)
                return;

            if (m_InputTextures[index].hasMultipleDifferentValues == false)
            {
                // show warnings related to the selected texture
                Texture2D texture = (Texture2D) m_InputTextures[index].objectReferenceValue;
                if (texture != null)
                {
                    TextureImporterType textureType = SmartTextureImporter.GetTextureType(texture);
                    if (textureType == TextureImporterType.Default) // Check/show warnings if no errors
                    {
                        bool targetIsSRGB = isAlpha ? false : m_sRGBTextureProperty.boolValue;
                        bool sourceIsSRGB =
                            m_InputTextureSettings[index].FindPropertyRelative("channel").enumValueIndex ==
                            (int) TextureChannel.A
                                ? false
                                : TextureFormatUtilities.IsTextureSrgb(texture);

                        bool compressionWarning = TextureFormatUtilities.IsTextureCompressed(texture);
                        bool srgbWarning = sourceIsSRGB != targetIsSRGB;
                        if (compressionWarning || srgbWarning)
                        {
                            string warning = "Warning, input source will cause artifacts";
                            if (compressionWarning) warning += "\n - Source is using compression";
                            if (srgbWarning)
                                warning += $"\n - Input texture channel uses " + (sourceIsSRGB ? "srgb" : "linear") +
                                           " encoding but target will be " + (targetIsSRGB ? "srgb" : "linear");
                            EditorGUILayout.HelpBox(warning, MessageType.Warning);
                        }
                    }
                    else if (textureType == TextureImporterType.NormalMap)
                    {
                        EditorGUILayout.HelpBox(
                            "Textures marked as a normal map are not yet a valid source for SmartTextures. \nIf you wish to pack a normal map, the source texture should be set to as type \"default\" and srgb turned off.",
                            MessageType.Error);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(
                            "Texture sources should be imported as default type to work as expected with SmartTextures.",
                            MessageType.Error);
                    }
                }
            }

            EditorGUILayout.PropertyField(m_InputTextures[index], Styles.labelChannels[index]);
            EditorGUILayout.BeginHorizontal();
            {
                SerializedProperty invertColor = m_InputTextureSettings[index].FindPropertyRelative("invertColor");
                EditorGUILayout.PropertyField(invertColor);

                SerializedProperty channelSource = m_InputTextureSettings[index].FindPropertyRelative("channel");
                using (new EditorGUI.MixedValueScope(channelSource.hasMultipleDifferentValues))
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        int channelIndex = channelSource.hasMultipleDifferentValues ? -1 : channelSource.intValue;
                        channelIndex = GUILayout.Toolbar(channelIndex, Styles.channelSourceLabels);
                        if (check.changed) channelSource.intValue = channelIndex;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        void DrawTextureImporterSettings()
        {
            SerializedProperty maxTextureSize =
                m_TexturePlatformSettingsProperty.FindPropertyRelative("m_MaxTextureSize");
            SerializedProperty resizeAlgorithm =
                m_TexturePlatformSettingsProperty.FindPropertyRelative("m_ResizeAlgorithm");
            SerializedProperty textureCompressionLevel =
                m_TexturePlatformSettingsProperty.FindPropertyRelative("m_TextureCompression");
            SerializedProperty textureUseCrunch =
                m_TexturePlatformSettingsProperty.FindPropertyRelative("m_CrunchedCompression");
            SerializedProperty textureCrunchQuality =
                m_TexturePlatformSettingsProperty.FindPropertyRelative("m_CompressionQuality");
            
            using (new EditorGUI.MixedValueScope(maxTextureSize.hasMultipleDifferentValues)) {
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    int sizeOption = EditorGUILayout.Popup("Texture Size", (int) Mathf.Log(maxTextureSize.intValue, 2) - 5, Styles.textureSizeOptions);
                    if (check.changed) maxTextureSize.intValue = 32 << sizeOption;
                }
            }
            
            using (new EditorGUI.MixedValueScope(resizeAlgorithm.hasMultipleDifferentValues)) {
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    int resizeOption = EditorGUILayout.Popup("Resize Algorithm", resizeAlgorithm.intValue, Styles.resizeAlgorithmOptions);
                    if (check.changed) resizeAlgorithm.intValue = resizeOption;
                }
            }

            EditorGUILayout.LabelField("Compression", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_UseExplicitTextureFormat);
                bool allowEditingTextureFormat = m_UseExplicitTextureFormat.hasMultipleDifferentValues ? false : m_UseExplicitTextureFormat.boolValue;

                using (new EditorGUI.DisabledScope(!allowEditingTextureFormat))
                {
                    EditorGUILayout.PropertyField(m_TextureFormat);
                    
                    using (new EditorGUI.MixedValueScope(textureCompressionLevel.hasMultipleDifferentValues)) {
                        using (var check = new EditorGUI.ChangeCheckScope()) {
                            int compressionOption = EditorGUILayout.Popup(Styles.compressionLevel, textureCompressionLevel.intValue, Styles.textureCompressionOptions);
                            if (check.changed) textureCompressionLevel.intValue = compressionOption;
                        }
                    }
                }
                
                using (new EditorGUI.MixedValueScope(textureUseCrunch.hasMultipleDifferentValues)) {
                    using (var check = new EditorGUI.ChangeCheckScope()) {
                        bool crunchEnabled = EditorGUILayout.Toggle(Styles.crunchCompression, textureUseCrunch.intValue > 0);
                        if (check.changed) textureUseCrunch.boolValue = crunchEnabled;
                    }
                }
                
                using (new EditorGUI.DisabledScope(textureCrunchQuality.hasMultipleDifferentValues ? true : !textureUseCrunch.boolValue))
                {
                    using (new EditorGUI.MixedValueScope(textureUseCrunch.hasMultipleDifferentValues)) {
                        using (var check = new EditorGUI.ChangeCheckScope()) {
                            int crunchLevel = EditorGUILayout.IntSlider(Styles.crunchQuality, textureCrunchQuality.hasMultipleDifferentValues ? -1 : textureCrunchQuality.intValue, 0, 100, new GUILayoutOption[] {GUILayout.ExpandWidth(true)});
                            if (check.changed) textureCrunchQuality.intValue = crunchLevel;
                        }
                    }
                }
            }
        }

        void CacheSerializedProperties()
        {
            isMultiEditing = serializedObject.isEditingMultipleObjects;
            
            SerializedProperty texturesProperty = serializedObject.FindProperty("m_InputTextures");
            SerializedProperty settingsProperty = serializedObject.FindProperty("m_InputTextureSettings");
            for (int i = 0; i < 4; ++i)
            {
                m_InputTextures[i] = texturesProperty.GetArrayElementAtIndex(i);
                m_InputTextureSettings[i] = settingsProperty.GetArrayElementAtIndex(i);
            }

            m_ResolutionMode = serializedObject.FindProperty("m_ResolutionMode");
            m_TargetResolution = serializedObject.FindProperty("m_TargetResolution");

            m_AlphaIsTransparency = serializedObject.FindProperty("m_AlphaIsTransparency");
            m_IsReadableProperty = serializedObject.FindProperty("m_IsReadable");
            m_sRGBTextureProperty = serializedObject.FindProperty("m_sRGBTexture");

            m_EnableMipMapProperty = serializedObject.FindProperty("m_EnableMipMap");
            m_StreamingMipMaps = serializedObject.FindProperty("m_StreamingMipMaps");
            m_StreamingMipMapPriority = serializedObject.FindProperty("m_StreamingMipMapPriority");

            m_FilterModeProperty = serializedObject.FindProperty("m_FilterMode");
            m_WrapModeProperty = serializedObject.FindProperty("m_WrapMode");
            m_AnisotropicLevelPropery = serializedObject.FindProperty("m_AnisotropicLevel");

            m_TexturePlatformSettingsProperty = serializedObject.FindProperty("m_TexturePlatformSettings");
            m_TextureFormat = serializedObject.FindProperty("m_TextureFormat");
            m_UseExplicitTextureFormat = serializedObject.FindProperty("m_UseExplicitTextureFormat");
        }
    }
}