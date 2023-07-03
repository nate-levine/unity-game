Shader "Custom/CompositeLightMasks"
{
    Properties
    {
        _MainTex("Tex", 2DArray) = "" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            /* texture arrays are not available everywhere,
             * so only compile shader on platforms where they are.\
             */
            #pragma require 2darray

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

            // Declare the texture 2d array.
            UNITY_DECLARE_TEX2DARRAY(_MainTex);
            // The depth is defined as the number of light maps in the array.
            int _Depth;
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
                fixed4 col = 0;

                // Composite the light maps together.
                for (int index = 0; index < _Depth; index++)
                {
                    col.rgb += UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(i.uv, index)).rgb;
                }

                return col;
            }
            ENDCG
        }
    }
}