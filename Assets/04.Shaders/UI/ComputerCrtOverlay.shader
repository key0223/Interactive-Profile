Shader "InteractiveProfile/UI/Computer CRT Overlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.28
        _ScanlineDensity ("Scanline Density", Float) = 220
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.06
        _FlickerIntensity ("Flicker Intensity", Range(0, 1)) = 0.035
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.42
        _TintColor ("CRT Tint Color", Color) = (0.46, 1, 0.78, 0.18)
        _TimeScale ("Time Scale", Float) = 1
        _OverlayTime ("Overlay Time", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _ScanlineIntensity;
            float _ScanlineDensity;
            float _NoiseIntensity;
            float _FlickerIntensity;
            float _VignetteIntensity;
            fixed4 _TintColor;
            float _TimeScale;
            float _OverlayTime;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            float Hash(float2 value)
            {
                return frac(sin(dot(value, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 baseColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                float2 uv = IN.texcoord;
                float time = _OverlayTime * _TimeScale;

                float scanline = 1.0 - _ScanlineIntensity * (0.5 + 0.5 * sin((uv.y + time * 0.015) * _ScanlineDensity));
                float noise = (Hash(floor(uv * float2(420.0, 240.0)) + floor(time * 32.0)) - 0.5) * _NoiseIntensity;
                float flicker = 1.0 + sin(time * 48.0) * _FlickerIntensity;

                float2 centered = uv * 2.0 - 1.0;
                float vignette = saturate(1.0 - dot(centered, centered) * _VignetteIntensity);
                float curve = saturate(dot(centered, centered));
                float curvatureAlpha = 1.0 - smoothstep(0.88, 1.18, curve) * 0.35;

                fixed3 overlayColor = _TintColor.rgb * scanline * flicker;
                overlayColor += noise;
                overlayColor *= vignette;

                fixed alpha = saturate(baseColor.a + _TintColor.a + _ScanlineIntensity * 0.16 + _NoiseIntensity * 0.25);
                alpha *= curvatureAlpha;
                alpha *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                return fixed4(overlayColor, alpha);
            }
            ENDCG
        }
    }
}
