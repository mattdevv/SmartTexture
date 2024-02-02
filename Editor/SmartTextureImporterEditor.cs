using System;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[CustomEditor(typeof(SmartTextureImporter), true)]
public class SmartTextureImporterEditor : ScriptedImporterEditor
{
    internal static class Styles
    {
        public static readonly GUIContent[] labelChannels =
        {
            EditorGUIUtility.TrTextContent("Red Channel", "This texture source channel will be packed into the Output texture red channel"),
            EditorGUIUtility.TrTextContent("Green Channel", "This texture source channel will be packed into the Output texture green channel"),
            EditorGUIUtility.TrTextContent("Blue Channel", "This texture source channel will be packed into the Output texture blue channel"),
            EditorGUIUtility.TrTextContent("Alpha Channel", "This texture source channel will be packed into the Output texture alpha channel"),
        };

        public static readonly GUIContent invertColor = EditorGUIUtility.TrTextContent("Invert Color", "If enabled outputs the inverted color (1.0 - color)");
        public static readonly GUIContent channelSource = EditorGUIUtility.TrTextContent("Channel", "Choose which channel will be used from the source texture");
        public static readonly GUIContent[] channelSourceLabels =
        {
            EditorGUIUtility.TrTextContent("R", "Use source's red channel"),
            EditorGUIUtility.TrTextContent("G", "Use source's green channel"),
            EditorGUIUtility.TrTextContent("B", "Use source's blue channel"),
            EditorGUIUtility.TrTextContent("A", "Use source's alpha channel"),
        };
        
        public static readonly GUIContent resolutionMode = EditorGUIUtility.TrTextContent("Resolution Mode", "Choose how the output texture's resolution is determined.");
        public static readonly GUIContent[] resolutionModeLabels =
        {
            EditorGUIUtility.TrTextContent("Maximum", "Use maximum length found for each dimension"),
            EditorGUIUtility.TrTextContent("Minimum", "Use minimum length found for each dimension"),
            EditorGUIUtility.TrTextContent("Explicit", "Use provided dimensions as an exact resolution"),
        };
        public static readonly GUIContent targetResolution = EditorGUIUtility.TrTextContent("Target Resolution", "Exact resolution to use for output texture.");

        public static readonly GUIContent alphaIsTransparency = EditorGUIUtility.TrTextContent("Use alpha as transparency", "Set to indicate that the texture's alpha channel should be used as a transparency mask.");
        public static readonly GUIContent readWrite = EditorGUIUtility.TrTextContent("Read/Write Enabled", "Enable to be able to access the raw pixel data from code.");
        public static readonly GUIContent generateMipMaps = EditorGUIUtility.TrTextContent("Generate Mip Maps");
        public static readonly GUIContent streamingMipMaps = EditorGUIUtility.TrTextContent("Streaming Mip Maps");
        public static readonly GUIContent streamingMipMapsPrio = EditorGUIUtility.TrTextContent("Streaming Mip Maps Priority");
        public static readonly GUIContent sRGBTexture = EditorGUIUtility.TrTextContent("sRGB (Color Texture)", "Texture content is stored in gamma space. Non-HDR color textures should enable this flag (except if used for IMGUI).");

        public static readonly GUIContent textureFilterMode = EditorGUIUtility.TrTextContent("Filter Mode");
        public static readonly GUIContent textureWrapMode = EditorGUIUtility.TrTextContent("Wrap Mode");
        public static readonly GUIContent textureAnisotropicLevel = EditorGUIUtility.TrTextContent("Anisotropic Level");

        public static readonly GUIContent crunchCompression = EditorGUIUtility.TrTextContent("Crunch");
        public static readonly GUIContent useExplicitTextureFormat = EditorGUIUtility.TrTextContent("Use Explicit Texture Format");

        public static readonly string[] textureSizeOptions =
        {
            "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192",
        };

        public static readonly string[] textureCompressionOptions = Enum.GetNames(typeof(TextureImporterCompression));
        public static readonly string[] textureFormat = Enum.GetNames(typeof(TextureFormat));
        public static readonly string[] resizeAlgorithmOptions = Enum.GetNames(typeof(TextureResizeAlgorithm));
    }

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
    SerializedProperty m_AnisotropiceLevelPropery;

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
                DrawInputTexture(3);
            }
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        m_showOutputTexture = EditorGUILayout.Foldout(m_showOutputTexture, "Output Texture", EditorStyles.foldoutHeader);
        if (m_showOutputTexture)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                m_ResolutionMode.intValue = EditorGUILayout.EnumPopup(Styles.resolutionMode, (SmartTextureImporter.TextureSizeMode)m_ResolutionMode.intValue).GetHashCode();
                using (new EditorGUI.DisabledScope(m_ResolutionMode.intValue != (int)SmartTextureImporter.TextureSizeMode.Explicit))
                {
                    Vector2Int changedResolution = EditorGUILayout.Vector2IntField(Styles.targetResolution, m_TargetResolution.vector2IntValue);
                    changedResolution.x = Mathf.Max(changedResolution.x, 1);
                    changedResolution.y = Mathf.Max(changedResolution.y, 1);
                    m_TargetResolution.vector2IntValue = changedResolution;
                }
                EditorGUILayout.Space();
                
                // TODO: Figure out how to apply TextureImporterSettings on ScriptedImporter
                EditorGUILayout.PropertyField(m_AlphaIsTransparency, Styles.alphaIsTransparency);
                EditorGUILayout.PropertyField(m_sRGBTextureProperty, Styles.sRGBTexture);
                EditorGUILayout.PropertyField(m_IsReadableProperty, Styles.readWrite);
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(m_FilterModeProperty, Styles.textureFilterMode);
                EditorGUILayout.PropertyField(m_WrapModeProperty, Styles.textureWrapMode);
                EditorGUILayout.IntSlider(m_AnisotropiceLevelPropery, 0, 16, Styles.textureAnisotropicLevel);
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
                // TODO: Figure out how to apply PlatformTextureImporterSettings on ScriptedImporter
                DrawTextureImporterSettings();
            }
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        serializedObject.ApplyModifiedProperties();
        ApplyRevertGUI();
    }

    void DrawInputTexture(int index)
    {
        if (index < 0 || index >= 4)
            return;
        
        EditorGUILayout.PropertyField(m_InputTextures[index], Styles.labelChannels[index]);
        EditorGUILayout.BeginHorizontal();
        {
            SerializedProperty invertColor = m_InputTextureSettings[index].FindPropertyRelative("invertColor");
            invertColor.boolValue = EditorGUILayout.Toggle(Styles.invertColor, invertColor.boolValue);
            SerializedProperty channelSource = m_InputTextureSettings[index].FindPropertyRelative("channel");
            channelSource.intValue = GUILayout.Toolbar(channelSource.intValue, Styles.channelSourceLabels);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
    }

    void DrawTextureImporterSettings()
    {
        SerializedProperty maxTextureSize = m_TexturePlatformSettingsProperty.FindPropertyRelative("m_MaxTextureSize");
        SerializedProperty resizeAlgorithm =
            m_TexturePlatformSettingsProperty.FindPropertyRelative("m_ResizeAlgorithm");
        SerializedProperty textureCompression =
            m_TexturePlatformSettingsProperty.FindPropertyRelative("m_TextureCompression");
        SerializedProperty textureCompressionCrunched =
            m_TexturePlatformSettingsProperty.FindPropertyRelative("m_CrunchedCompression");

        
        EditorGUI.BeginChangeCheck();
        int sizeOption = EditorGUILayout.Popup("Texture Size", (int)Mathf.Log(maxTextureSize.intValue, 2) - 5, Styles.textureSizeOptions);
        if (EditorGUI.EndChangeCheck())
            maxTextureSize.intValue = 32 << sizeOption;

        EditorGUI.BeginChangeCheck();
        int resizeOption = EditorGUILayout.Popup("Resize Algorithm", resizeAlgorithm.intValue, Styles.resizeAlgorithmOptions);
        if (EditorGUI.EndChangeCheck())
            resizeAlgorithm.intValue = resizeOption;

        EditorGUILayout.LabelField("Compression", EditorStyles.boldLabel);
        using (new EditorGUI.IndentLevelScope())
        {
            EditorGUI.BeginChangeCheck();
            bool explicitFormat = EditorGUILayout.Toggle(Styles.useExplicitTextureFormat, m_UseExplicitTextureFormat.boolValue);
            if (EditorGUI.EndChangeCheck())
                m_UseExplicitTextureFormat.boolValue = explicitFormat;

            using (new EditorGUI.DisabledScope(explicitFormat))
            {
                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                int compressionOption = EditorGUILayout.Popup("Texture Type", textureCompression.intValue, Styles.textureCompressionOptions);
                if (EditorGUI.EndChangeCheck())
                    textureCompression.intValue = compressionOption;

                EditorGUI.BeginChangeCheck();
                var oldWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 100f;
                bool crunchOption = EditorGUILayout.Toggle(Styles.crunchCompression, textureCompressionCrunched.boolValue);
                EditorGUIUtility.labelWidth = oldWidth;
                if (EditorGUI.EndChangeCheck())
                    textureCompressionCrunched.boolValue = crunchOption;
                GUILayout.EndHorizontal();
            }

            using (new EditorGUI.DisabledScope(!explicitFormat))
            {
                EditorGUI.BeginChangeCheck();
                int format = EditorGUILayout.EnumPopup("Texture Format", (TextureFormat)m_TextureFormat.intValue).GetHashCode();//("Compression", m_TextureFormat.enumValueIndex, Styles.textureFormat);
                if (EditorGUI.EndChangeCheck())
                    m_TextureFormat.intValue = format;
            }
        }
        
    }
    
    void CacheSerializedProperties()
    {
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
        m_AnisotropiceLevelPropery = serializedObject.FindProperty("m_AnisotricLevel");

        m_TexturePlatformSettingsProperty = serializedObject.FindProperty("m_TexturePlatformSettings");
        m_TextureFormat = serializedObject.FindProperty("m_TextureFormat");
        m_UseExplicitTextureFormat = serializedObject.FindProperty("m_UseExplicitTextureFormat");
    }
}
