using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

[ScriptedImporter(k_SmartTextureVersion, k_SmartTextureExtesion)]
public class SmartTextureImporter : ScriptedImporter
{
    internal enum TextureSizeMode
    {
        Maximum,
        Minimum,
        Explicit
    }
    public const string k_SmartTextureExtesion = "smartex";
    public const int k_SmartTextureVersion = 1;
    public const int k_MenuPriority = 320;

    // Input Texture Settings
    [SerializeField] Texture2D[] m_InputTextures = new Texture2D[4];
    [SerializeField] TexturePackingSettings[] m_InputTextureSettings =
    {
        new TexturePackingSettings(false, TextureChannel.R),
        new TexturePackingSettings(false, TextureChannel.G), 
        new TexturePackingSettings(false, TextureChannel.B), 
        new TexturePackingSettings(false, TextureChannel.A)
    };
    
    // Output Texture Settings
    [SerializeField] TextureSizeMode m_ResolutionMode = TextureSizeMode.Maximum;
    [SerializeField] Vector2Int m_TargetResolution = new Vector2Int(2048, 2048);
    
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
        Texture2D texture;
        
        Texture2D[] textures = m_InputTextures;
        TexturePackingSettings[] settings = m_InputTextureSettings;

        // check if there is at least 1 valid texture
        bool canGenerateTexture = false;
        foreach (var t in textures)
        {
            if (t != null)
            {
                canGenerateTexture = true;
                break;
            }
        }
        
        //Only attempt to apply any settings if the inputs exist
        if (canGenerateTexture)
        {
            int width;
            int height;

            // calculate resolution
            switch (m_ResolutionMode)
            {
                case TextureSizeMode.Explicit:
                    width = m_TargetResolution.x;
                    height = m_TargetResolution.y;
                    break;
                case TextureSizeMode.Maximum:
                    GetMaxTextureSize(textures, out width, out height);
                    break;
                case TextureSizeMode.Minimum:
                    GetMinTextureSize(textures, out width, out height);
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            // clamp maximum resolution to platform settings
            width = Mathf.Clamp(width, 1, m_TexturePlatformSettings.maxTextureSize);
            height = Mathf.Clamp(height, 1, m_TexturePlatformSettings.maxTextureSize);
            
            bool hasAlpha = textures[3] != null;
            texture = new Texture2D(width, height, hasAlpha ? TextureFormat.ARGB32 : TextureFormat.RGB24,
                m_EnableMipMap, !m_sRGBTexture)
            {
                alphaIsTransparency = m_AlphaIsTransparency,
                filterMode = m_FilterMode,
                wrapMode = m_WrapMode,
                anisoLevel = m_AnisotricLevel,
            };
            TextureExtension.PackChannels(texture, textures, settings);

            // TODO: Seems like we need to call TextureImporter.SetPlatformTextureSettings to register/apply platform
            // settings. However we can't subclass from TextureImporter... Is there other way?
            
            // Find recommended TextureFormat for import settings and compress
            TextureFormat format = m_UseExplicitTextureFormat
                ? m_TextureFormat
                : TextureFormatUtilities.GetRecommendedTextureFormat((TextureCompressionLevel)m_TexturePlatformSettings.textureCompression, hasAlpha, false, m_TexturePlatformSettings.crunchedCompression);
            int quality = m_TexturePlatformSettings.crunchedCompression
                ? m_TexturePlatformSettings.compressionQuality
                : 100;
            EditorUtility.CompressTexture(texture, format, quality);

            ApplyPropertiesViaSerializedObj(texture);
            
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
        }
        else
        {
            // need asset to contain a valid texture to prevent errors
            texture = new Texture2D(16, 16);
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

    // Returns the largest resolution of given textures
    void GetMaxTextureSize(Texture2D[] textures, out int width, out int height)
    {
        int maxWidth = -1;
        int maxHeight = -1;
        
        foreach (Texture2D t in textures)
        {
            if (t == null)
                continue;

            maxWidth = Mathf.Max(t.width, maxWidth);
            maxHeight = Mathf.Max(t.height, maxHeight);
        }

        width = maxWidth;
        height = maxHeight;
    }
    
    // Returns the smallest resolution of given textures
    void GetMinTextureSize(Texture2D[] textures, out int width, out int height)
    {
        int maxWidth = -1;
        int maxHeight = -1;
        
        // use first valid texture as base resolution
        foreach (Texture2D t in textures)
        {
            if (t == null)
                continue;

            maxWidth = t.width;
            maxHeight = t.height;
            break;
        }
        
        // find smallest texture
        foreach (Texture2D t in textures)
        {
            if (t == null)
                continue;

            maxWidth = Mathf.Min(t.width, maxWidth);
            maxHeight = Mathf.Min(t.height, maxHeight);
        }

        width = maxWidth;
        height = maxHeight;
    }
}
