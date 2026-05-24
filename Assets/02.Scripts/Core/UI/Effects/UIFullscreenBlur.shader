Shader "UI/FullscreenBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurIntensity ("Blur Intensity", Range(0, 1)) = 1
        _BlurTexelSize ("Blur Texel Size", Vector) = (0.003, 0.003, 0, 0)
        _SampleCount ("Sample Count", Range(1, 9)) = 9
        _TintColor ("Tint Color", Color) = (0, 0, 0, 0.2)
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

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "UIFullscreenBlur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BlurIntensity;
            float4 _BlurTexelSize;
            float _SampleCount;
            fixed4 _TintColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 offset = _BlurTexelSize.xy;

                fixed4 original = tex2D(_MainTex, i.uv);
                fixed4 color = original;
                float weight = 1.0;

                if (_SampleCount > 1.5)
                {
                    color += tex2D(_MainTex, i.uv + float2(offset.x, 0)) * 0.75;
                    color += tex2D(_MainTex, i.uv - float2(offset.x, 0)) * 0.75;
                    color += tex2D(_MainTex, i.uv + float2(0, offset.y)) * 0.75;
                    color += tex2D(_MainTex, i.uv - float2(0, offset.y)) * 0.75;
                    weight += 3.0;
                }

                if (_SampleCount > 5.5)
                {
                    color += tex2D(_MainTex, i.uv + offset) * 0.5;
                    color += tex2D(_MainTex, i.uv - offset) * 0.5;
                    color += tex2D(_MainTex, i.uv + float2(offset.x, -offset.y)) * 0.5;
                    color += tex2D(_MainTex, i.uv + float2(-offset.x, offset.y)) * 0.5;
                    weight += 2.0;
                }

                color /= weight;
                color = lerp(original, color, saturate(_BlurIntensity));
                color.rgb = lerp(color.rgb, _TintColor.rgb, _TintColor.a);
                color.a = i.color.a;
                return color;
            }
            ENDCG
        }
    }
}
