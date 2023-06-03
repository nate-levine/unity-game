Shader "Hidden/Composite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        // blur x-axis
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define SAMPLES 5

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _LightingTex;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 lightingCol = tex2D(_LightingTex, i.uv);

                // blur x-axis
                float4 sum = 0;
                for (float index = 0; index < SAMPLES; index++) {
                    //get uv coordinate of sample
                    float2 uv = i.uv + float2((index / (SAMPLES-1) - 0.5) * 0.25 * (_ScreenParams.y / _ScreenParams.x), 0);
                    //add color at position to color
                    sum += tex2D(_LightingTex, uv);
                }
                lightingCol.rgb = sum / SAMPLES;

                // composite
                fixed4 col = clamp((lightingCol + tex2D(_LightingTex, i.uv)), 0, 1);

                return col;
            }
            ENDCG
        }
        // blur y-axis
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define SAMPLES 5

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _LightingTex;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 lightingCol = tex2D(_LightingTex, i.uv);

                // blur y-axis
                float4 sum = 0;
                for (float index = 0; index < SAMPLES; index++) {
                    //get uv coordinate of sample
                    float2 uv = i.uv + float2(0, (index / (SAMPLES-1) - 0.5) * 0.25 * (_ScreenParams.y / _ScreenParams.x));
                    //add color at position to color
                    sum += tex2D(_LightingTex, uv);
                }

                //divide the sum of values by the amount of samples
                lightingCol.rgb = sum / SAMPLES;

                fixed4 col = clamp((lightingCol + tex2D(_LightingTex, i.uv)), 0, 1);

                return col;
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _LightingTexX;
            sampler2D _LightingTexY;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 mainCol = tex2D(_MainTex, i.uv);
                fixed4 lightingColX = tex2D(_LightingTexX, i.uv);
                fixed4 lightingColY = tex2D(_LightingTexY, i.uv);

                fixed4 col = mainCol * clamp((lightingColX + lightingColY), 0, 1);

                return col;
            }
            ENDCG
        }
    }
}
