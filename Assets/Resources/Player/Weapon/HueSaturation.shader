Shader "Custom/HueSaturation"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Hue ("Hue", Range(-1,1)) = 0  // Hue shift (-1 to 1 is equivalent to -180° to 180°)
        _Saturation ("Saturation", Range(0,2)) = 1 // Normalized (1 = normal)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off Lighting Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float _Hue;
            float _Saturation;

            v2f vert(appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            // Convert RGB to HSV
            float3 RGBtoHSV(float3 c) {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            // Convert HSV back to RGB
            float3 HSVtoRGB(float3 c) {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color; // Get original color
                float3 hsv = RGBtoHSV(col.rgb);
                hsv.x = frac(hsv.x + _Hue); // Shift hue
                hsv.y = saturate(hsv.y * _Saturation); // Adjust saturation
                col.rgb = HSVtoRGB(hsv);
                return col;
            }
            ENDCG
        }
    }
}
