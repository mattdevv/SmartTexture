using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

// TODO: Add option to pack using compute shader
// TODO: Convert from sRGB to linear color space if necessary.
// TODO: Texture compression / Format
// TODO: Mipmaps

public enum ChannelSource
{
    R = 0,
    G = 1,
    B = 2,
    A = 3
};

[System.Serializable]
/// <summary>
/// Containts settings that apply color modifiers to each channel.
/// </summary>
public struct TexturePackingSettings
{
    /// <summary>
    /// Outputs the inverted color (1.0 - color)
    /// </summary>
    public bool invertColor;

    /// <summary>
    /// Which color channel to use from the source
    /// </summary>
    public ChannelSource channel;

    public TexturePackingSettings(bool shouldInvert, ChannelSource channelSource)
    {
        invertColor = shouldInvert;
        channel = channelSource;
    }
}

public static class TextureExtension
{
    static Material s_PackChannelMaterial = null;

    private static Material packChannelMaterial
    {
        get
        {
            if (s_PackChannelMaterial == null)
            {
                Shader packChannelShader = Shader.Find("Hidden/PackChannel");
                if (packChannelShader == null)
                    return null;
                
                s_PackChannelMaterial = new Material(packChannelShader);
            }

            return s_PackChannelMaterial;
        }
    }

    public static Vector4 GetChannelMask(ChannelSource channel)
    {
        Vector4 mask = Vector4.zero;
        
        if (channel < 0 || (int)channel > 3)
        {
            Debug.LogError("Channel must be in range 0..3 (RGBA)");
        }
        else
        {
            mask[(int)channel] = 1;
        }

        return mask;
    }

    public static bool IsTextureSrgb(Texture texture)
    {
        return GraphicsFormatUtility.IsSRGBFormat(texture.graphicsFormat);
    }
    
    public static void PackChannels(Texture2D mask, Texture2D[] textures, TexturePackingSettings[] settings = null)
    {
        if (textures == null || textures.Length != 4)
        {
            Debug.LogError("Invalid parameter to PackChannels. An array of 4 textures is expected");
        }

        if (settings == null)
        {
            settings = new TexturePackingSettings[4];
            for (int i = 0; i < settings.Length; ++i)
            {
                settings[i].invertColor = false;
                settings[i].channel = (ChannelSource)i;
            }
        }
        
        int width = mask.width;
        int height = mask.height;
        int pixelCount = width * height;
        
        bool isSrgb = IsTextureSrgb(mask);
        
        float[] invertColor =
        {
            settings[0].invertColor ? 1.0f : 0.0f,
            settings[1].invertColor ? 1.0f : 0.0f,
            settings[2].invertColor ? 1.0f : 0.0f,
            settings[3].invertColor ? 1.0f : 0.0f,
        };

        packChannelMaterial.SetTexture("_RedChannel", textures[0] != null ? textures[0] : Texture2D.blackTexture);
        packChannelMaterial.SetTexture("_GreenChannel", textures[1] != null ? textures[1] : Texture2D.blackTexture);
        packChannelMaterial.SetTexture("_BlueChannel", textures[2] != null ? textures[2] : Texture2D.blackTexture);
        packChannelMaterial.SetTexture("_AlphaChannel", textures[3] != null ? textures[3] : Texture2D.blackTexture);
        packChannelMaterial.SetVector("_InvertColor", new Vector4(invertColor[0], invertColor[1], invertColor[2], invertColor[3]));
        
        packChannelMaterial.SetVector("_ChannelMapR", GetChannelMask(settings[0].channel));
        packChannelMaterial.SetVector("_ChannelMapG", GetChannelMask(settings[1].channel));
        packChannelMaterial.SetVector("_ChannelMapB", GetChannelMask(settings[2].channel));
        packChannelMaterial.SetVector("_ChannelMapA", GetChannelMask(settings[3].channel));
        
        var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, isSrgb ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        CommandBuffer cmd = new CommandBuffer();
        cmd.Blit(null, rt, packChannelMaterial);
        cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        Graphics.ExecuteCommandBuffer(cmd);
        mask.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        mask.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
    }
}
