using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Object = UnityEngine.Object;

[ScriptedImporter(k_SmartTextureVersion, k_SmartTextureExtesion)]
public class SmartTextureImporter : ScriptedImporter
{
    public const string k_SmartTextureExtesion = "smartex";
    public const int k_SmartTextureVersion = 1;
    public const int k_MenuPriority = 320;

    // Input Texture Settings
    [SerializeField] Texture2D[] m_InputTextures = new Texture2D[4];
    [SerializeField] TexturePackingSettings[] m_InputTextureSettings =
    {
        new TexturePackingSettings(false, ChannelSource.R),
        new TexturePackingSettings(false, ChannelSource.G), 
        new TexturePackingSettings(false, ChannelSource.B), 
        new TexturePackingSettings(false, ChannelSource.A)
    };
    
    // Output Texture Settings
    [SerializeField] bool m_UseExplicitResolution = false;
    [SerializeField] Vector2Int m_Resolution = new Vector2Int(2048, 2048);
    
    [SerializeField] bool m_AlphaIsTransparency = false;
    [SerializeField] bool m_IsReadable = false;
    [SerializeField] bool m_sRGBTexture = false;
    
    [SerializeField] bool m_EnableMipMap = true;
    [SerializeField] bool m_StreamingMipMaps = false;
    [SerializeField] int m_StreamingMipMapPriority = 0;
    
    // TODO: MipMap Generation, is it possible to configure?
    //[SerializeField] bool m_BorderMipMaps = false;
    //[SerializeField] TextureImporterMipFilter m_MipMapFilter = TextureImporterMipFilter.BoxFilter;
    //[SerializeField] bool m_MipMapsPreserveCoverage = false;
    //[SerializeField] bool m_FadeoutMipMaps = false;

    [SerializeField] FilterMode m_FilterMode = FilterMode.Bilinear;
    [SerializeField] TextureWrapMode m_WrapMode = TextureWrapMode.Repeat;
    [SerializeField] int m_AnisotricLevel = 1;

    [SerializeField] TextureImporterPlatformSettings m_TexturePlatformSettings = new TextureImporterPlatformSettings();

    [SerializeField] TextureFormat m_TextureFormat = TextureFormat.ARGB32;
    [SerializeField] bool m_UseExplicitTextureFormat = false;

    [MenuItem("Assets/Create/Smart Texture", priority = k_MenuPriority)]
    static void CreateSmartTextureMenuItem()
    {
        // Asset creation code from pschraut Texture2DArrayImporter
        // https://github.com/pschraut/UnityTexture2DArrayImportPipeline/blob/master/Editor/Texture2DArrayImporter.cs#L360-L383
        string directoryPath = "Assets";
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            directoryPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(directoryPath) && File.Exists(directoryPath))
            {
                directoryPath = Path.GetDirectoryName(directoryPath);
                break;
            }
        }

        directoryPath = directoryPath.Replace("\\", "/");
        if (directoryPath.Length > 0 && directoryPath[directoryPath.Length - 1] != '/')
            directoryPath += "/";
        if (string.IsNullOrEmpty(directoryPath))
            directoryPath = "Assets/";

        var fileName = string.Format("SmartTexture.{0}", k_SmartTextureExtesion);
        directoryPath = AssetDatabase.GenerateUniqueAssetPath(directoryPath + fileName);
        ProjectWindowUtil.CreateAssetWithContent(directoryPath,
            "Smart Texture Asset for Unity. Allows to channel pack textures by using a ScriptedImporter. Requires Smart Texture Package from https://github.com/phi-lira/SmartTexture. Developed by Felipe Lira.");
    }
    
    public override void OnImportAsset(AssetImportContext ctx)
    {
        int width = m_TexturePlatformSettings.maxTextureSize;
        int height = m_TexturePlatformSettings.maxTextureSize;
        Texture2D[] textures = m_InputTextures;
        TexturePackingSettings[] settings = m_InputTextureSettings;
        
        var canGenerateTexture = GetMaxOuputTextureSize(textures, out var inputW, out var inputH);
        
        // optional resolution override
        if (m_UseExplicitResolution)
        {
            inputW = m_Resolution.x;
            inputH = m_Resolution.y;
        }
        
        //Mimic default importer. We use max size unless assets are smaller
        width = width < inputW ? width : inputW;
        height = height < inputH ? height : inputH;

        bool hasAlpha = textures[3] != null;
        Texture2D texture = new Texture2D(width, height, hasAlpha ? TextureFormat.ARGB32 : TextureFormat.RGB24, m_EnableMipMap, !m_sRGBTexture)
        {
            alphaIsTransparency = m_AlphaIsTransparency,
            filterMode = m_FilterMode,
            wrapMode = m_WrapMode,
            anisoLevel = m_AnisotricLevel,
        };

        //Only attempt to apply any settings if the inputs exist
        if (canGenerateTexture)
        {
            TextureExtension.PackChannels(texture, textures, settings);

            // Mark all input textures as dependency to the texture array.
            // This causes the texture to get re-generated when any input texture changes or when the build target changed.
            foreach (Texture2D t in textures)
            {
                if (t != null)
                {
                    var path = AssetDatabase.GetAssetPath(t);
                    ctx.DependsOnSourceAsset(path);
                }
            }

            // TODO: Seems like we need to call TextureImporter.SetPlatformTextureSettings to register/apply platform
            // settings. However we can't subclass from TextureImporter... Is there other way?
            
            //Currently just supporting one compression format in liew of TextureImporter.SetPlatformTextureSettings
            if (m_UseExplicitTextureFormat)
                EditorUtility.CompressTexture(texture, m_TextureFormat, 100);
            else if (m_TexturePlatformSettings.textureCompression != TextureImporterCompression.Uncompressed)
                texture.Compress(m_TexturePlatformSettings.textureCompression == TextureImporterCompression.CompressedHQ);
            
            ApplyPropertiesViaSerializedObj(texture);
        }
        
		//If we pass the tex to the 3rd arg we can have it show in an Icon as normal, maybe configurable?
		ctx.AddObjectToAsset("mask", texture, texture);
        ctx.SetMainObject(texture);
    }

    void ApplyPropertiesViaSerializedObj(Texture tex)
    {
        var so = new SerializedObject(tex);
        
        so.FindProperty("m_IsReadable").boolValue = m_IsReadable;
        so.FindProperty("m_StreamingMipmaps").boolValue = m_StreamingMipMaps;
        so.FindProperty("m_StreamingMipmapsPriority").intValue = m_StreamingMipMapPriority;
        //Set ColorSpace on ctr instead
        //so.FindProperty("m_ColorSpace").intValue = (int)(m_sRGBTexture ? ColorSpace.Gamma : ColorSpace.Linear);

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Returns the largest resolution of given textures, and whether the function succeeded
    bool GetMaxOuputTextureSize(Texture2D[] textures, out int width, out int height)
    {
        width = -1;
        height = -1;
        
        foreach (Texture2D t in textures)
        {
            if (t == null)
                continue;

            width = Mathf.Max(t.width, width);
            height = Mathf.Max(t.height, height);
        }

        if (width == -1 || height == -1)
        {
            var defaultTexture = Texture2D.blackTexture;
            width = defaultTexture.width;
            height = defaultTexture.height;
            return false;
        }
        
        return true;
    }
}
