Shader "Hidden/PackChannel"
{
    Properties
    {
        _RedChannel("RedChannel", 2D) = "white" {}
        _GreenChannel("GreenChannel", 2D) = "white" {}
        _BlueChannel("BlueChannel", 2D) = "white" {}
        _AlphaChannel("AlphaChannel", 2D) = "white" {}
        [HideInInspector] _InvertColor("_InvertColor", Vector) = (1, 1, 1, 1)
        [HideInInspector] _ChannelMapR("_ChannelMapR", Vector) = (1, 0, 0, 0)
        [HideInInspector] _ChannelMapG("_ChannelMapG", Vector) = (0, 1, 0, 0)
        [HideInInspector] _ChannelMapB("_ChannelMapB", Vector) = (0, 0, 1, 0)
        [HideInInspector] _ChannelMapA("_ChannelMapA", Vector) = (0, 0, 0, 1)
    }

    HLSLINCLUDE
    Texture2D _RedChannel;
    Texture2D _GreenChannel;
    Texture2D _BlueChannel;
    Texture2D _AlphaChannel;
    sampler sampler_linear_clamp;

    half4 _InvertColor;
    half4 _ChannelMapR;
    half4 _ChannelMapG;
    half4 _ChannelMapB;
    half4 _ChannelMapA;

    struct attributes
    {
        float4 positionOS : POSITION;
        float2 uv         : TEXCOORD0;
    };

    struct varyings
    {
        float4 positionCS : SV_POSITION;
        float2 uv         : TEXCOORD;
    };
    
    half4 frag(varyings i) : SV_Target
    {
        float2 uv = i.uv;
        
        half r = dot(_RedChannel.Sample(sampler_linear_clamp, uv), _ChannelMapR);
        half g = dot(_GreenChannel.Sample(sampler_linear_clamp, uv), _ChannelMapG);
        half b = dot(_BlueChannel.Sample(sampler_linear_clamp, uv), _ChannelMapB);
        half a = dot(_AlphaChannel.Sample(sampler_linear_clamp, uv), _ChannelMapA);
        
        half4 rgba = half4(r, g, b, a);
        rgba = lerp(rgba, 1-rgba, _InvertColor);
        
        return rgba;
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline"}

        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            varyings vert(attributes i)
            {
                varyings o;
                
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = i.uv;
                
                return o;
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            varyings vert(attributes i)
            {
                varyings o;
                
                o.positionCS = UnityObjectToClipPos(i.positionOS.xyz);
                o.uv = i.uv;
                
                return o;
            }
            ENDHLSL
        }
    }
}
