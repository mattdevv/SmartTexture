using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace SmartTexture
{
    [Flags]
    public enum TextureChannel
    {
        R = 1,
        G = 2,
        B = 4,
        A = 8
    };

    // Trying to match UnityEditor.TextureCompressionQuality
    public enum TextureCompressionLevel
    {
        Uncompressed,
        Compressed,
        CompressedHQ,
        CompressedLQ
    }

    public static class TextureFormatUtilities
    {
        public static TextureChannel GetTextureChannelMask(bool r = false, bool g = false, bool b = false, bool a = false)
        {
            TextureChannel mask = 0;
            if (r) mask |= TextureChannel.R;
            if (g) mask |= TextureChannel.G;
            if (b) mask |= TextureChannel.B;
            if (a) mask |= TextureChannel.A;
            return mask;
        }
        
        public static TextureChannel GetTextureChannelMask(GraphicsFormat graphicsFormat)
        {
            int colorChannels = (int) GraphicsFormatUtility.GetColorComponentCount(graphicsFormat);
            Debug.Assert(colorChannels <= 3);
            int alphaChannels = (int) GraphicsFormatUtility.GetAlphaComponentCount(graphicsFormat);
            Debug.Assert(alphaChannels <= 1);
            
            TextureChannel mask = 0;
            if (colorChannels > 0) mask |= TextureChannel.R;
            if (colorChannels > 1) mask |= TextureChannel.G;
            if (colorChannels > 2) mask |= TextureChannel.B;
            if (alphaChannels > 0) mask |= TextureChannel.A;
            return mask;
        }
        
        public static TextureChannel GetTextureChannelMask(TextureFormat textureFormat)
        {
            int colorChannels = (int) GraphicsFormatUtility.GetColorComponentCount(textureFormat);
            Debug.Assert(colorChannels <= 3);
            int alphaChannels = (int) GraphicsFormatUtility.GetAlphaComponentCount(textureFormat);
            Debug.Assert(alphaChannels <= 1);
            
            TextureChannel mask = 0;
            if (colorChannels > 0) mask |= TextureChannel.R;
            if (colorChannels > 1) mask |= TextureChannel.G;
            if (colorChannels > 2) mask |= TextureChannel.B;
            if (alphaChannels > 0) mask |= TextureChannel.A;
            return mask;
        }
        
        public static bool IsTextureSrgb(Texture texture)
        {
            return GraphicsFormatUtility.IsSRGBFormat(texture.graphicsFormat);
        }

        public static bool IsTextureCompressed(Texture texture)
        {
            return GraphicsFormatUtility.IsCompressedFormat(texture.graphicsFormat);
        }

        // Converts a TextureFormat to its crunched version if possible
        public static TextureFormat GetCrunchFormat(TextureFormat format)
        {
            if (format == TextureFormat.DXT1)
                format = TextureFormat.DXT1Crunched;
            else if (format == TextureFormat.DXT5)
                format = TextureFormat.DXT5Crunched;
            else if (format == TextureFormat.ETC_RGB4)
                format = TextureFormat.ETC_RGB4Crunched;
            else if (format == TextureFormat.ETC2_RGBA8)
                format = TextureFormat.ETC2_RGBA8Crunched;

            return format;
        }

        // from https://docs.unity3d.com/Manual/class-TextureImporterOverride.html#default-texture-compression-formats-by-platform
        public static TextureFormat GetRecommendedTextureFormat(TextureCompressionLevel compression, bool hasAlpha,
            bool isHDR, bool useCrunch)
        {
            TextureFormat none;
            TextureFormat normal;
            TextureFormat high;
            TextureFormat low;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            if (isHDR)
            {
                none = TextureFormat.RGBAHalf;
                normal = TextureFormat.BC6H;
                high = TextureFormat.BC6H;
                low = TextureFormat.BC6H;
            }
            else if (hasAlpha)
            {
                none = TextureFormat.RGBA32;
                normal = TextureFormat.DXT5;
                high = TextureFormat.BC7;
                low = TextureFormat.DXT5;
            }
            else
            {
                none = TextureFormat.RGB24;
                normal = TextureFormat.DXT1;
                high = TextureFormat.BC7;
                low = TextureFormat.DXT1;
            }
            
#elif UNITY_WEBGL
            if (hasAlpha)
            {
                none = TextureFormat.RGBA32;
                low = normal = high = TextureFormat.DXT5;
            }
            else
            {
                none = TextureFormat.RGB24;
                low = normal = high = TextureFormat.DXT1;
            }
#elif UNITY_ANDROID
            if (isHDR)
            {
                none = TextureFormat.RGBAHalf;
                normal = TextureFormat.ASTC_HDR_6x6;
                high = TextureFormat.ASTC_HDR_4x4;
                low = TextureFormat.ASTC_HDR_12x12;
            }
            else if (hasAlpha)
            {
                none = TextureFormat.RGBA32;
                normal = TextureFormat.ASTC_6x6;
                high = TextureFormat.ASTC_4x4;
                low = TextureFormat.ASTC_12x12;
            }
            else
            {
                none = TextureFormat.RGB24;
                normal = TextureFormat.ASTC_6x6;
                high = TextureFormat.ASTC_4x4;
                low = TextureFormat.ASTC_12x12;
            }
                
            if (useCrunch && !isHDR)
            {
                if (hasAlpha)
                    low = normal = high = TextureFormat.ETC2_RGBA8Crunched;
                else
                    low = normal = high = TextureFormat.ETC_RGB4Crunched;
            }
#elif UNITY_IOS
            if (isHDR)
            {
                none = TextureFormat.RGBAHalf;
                normal = TextureFormat.ASTC_HDR_6x6;
                high = TextureFormat.ASTC_HDR_4x4;
                low = TextureFormat.ASTC_HDR_12x12;
            }
            else if (hasAlpha)
            {
                none = TextureFormat.RGBA32;
                normal = TextureFormat.ASTC_6x6;
                high = TextureFormat.ASTC_4x4;
                low = TextureFormat.ASTC_12x12;
            }
            else
            {
                none = TextureFormat.RGB24;
                normal = TextureFormat.ASTC_6x6;
                high = TextureFormat.ASTC_4x4;
                low = TextureFormat.ASTC_12x12;
            }
                
            if (useCrunch && !isHDR)
            {
                if (hasAlpha)
                    low = normal = high = TextureFormat.ETC2_RGBA8Crunched;
                else
                    low = normal = high = TextureFormat.ETC_RGB4Crunched;
            }
#else
            Debug.LogError("Platform not defined, using default graphics format");
            if (hasAlpha)
                none = low = normal = high = GraphicsFormatUtility.GetTextureFormat(GraphicsFormat.R8G8B8A8_UNorm);
            else
                none = low = normal = high = GraphicsFormatUtility.GetTextureFormat(GraphicsFormat.R8G8B8_UNorm);
#endif

            TextureFormat format;
            switch (compression)
            {
                case TextureCompressionLevel.Uncompressed:
                    format = none;
                    break;
                case TextureCompressionLevel.CompressedLQ:
                    format = low;
                    break;
                case TextureCompressionLevel.Compressed:
                    format = normal;
                    break;
                case TextureCompressionLevel.CompressedHQ:
                    format = high;
                    break;
                default:
                    Debug.LogError("Exception, compression level was not defined");
                    format = none;
                    break;
            }

            if (useCrunch)
            {
                format = GetCrunchFormat(format);
            }

            return format;
        }


        public static GraphicsFormat GetRecommendedGraphicsFormat(TextureCompressionLevel compression, bool isSrgb,
            bool hasAlpha, bool isHDR)
        {
            return GraphicsFormatUtility.GetGraphicsFormat(
                GetRecommendedTextureFormat(compression, hasAlpha, isHDR, false), isSrgb);
        }
    }
}