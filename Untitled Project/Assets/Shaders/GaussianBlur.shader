Shader "Custom/GaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        // x-axis blur.
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define stdDev 128

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 MainTex = tex2D(_MainTex, i.uv);

                fixed4 col = fixed4(0, 0, 0, 1);
                float sum = 0;

                for (int x = -8; x < 8; x++)
                {
                    float xPos = (float)x + 0.5;

                    float smoothStepWeight = smoothstep(-stdDev, stdDev, x);

                    sum += smoothStepWeight;

                    float2 uv = i.uv + float2(xPos / _ScreenParams.x, 0);

                    col += smoothStepWeight * tex2D(_MainTex, uv);
                }

                if (col.r > MainTex.r)
                {
                    return col / sum;
                }
                else
                {
                    return MainTex;
                }

            }
            ENDCG
        }
        // y-axis blur.
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define stdDev 128

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 MainTex = tex2D(_MainTex, i.uv);

                fixed4 col = fixed4(0, 0, 0, 1);
                float sum = 0;

                for (int y = -8; y < 8; y++)
                {
                    float yPos = (float)y + 0.5;

                    float smoothStepWeight = smoothstep(-stdDev, stdDev, y);

                    sum += smoothStepWeight;

                    float2 uv = i.uv + float2(0, yPos / _ScreenParams.y);

                    col += smoothStepWeight * tex2D(_MainTex, uv);
                }

                if (col.r > MainTex.r)
                {
                    return col / sum;
                }
                else
                {
                    return MainTex;
                }

            }
            ENDCG
        }
    }
}
