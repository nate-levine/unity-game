Shader "Custom/Shadows"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct WriteVertex
            {
                float3 position;
            };
            struct WriteTriangle
            {
                WriteVertex vertices[3];
                float3 normal;
            };
            StructuredBuffer<WriteTriangle> _WriteTriangles;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (uint vertexID : SV_VertexID)
            {
                v2f output;

                WriteTriangle writeTriangle = _WriteTriangles[vertexID / 3];
                WriteVertex input = writeTriangle.vertices[vertexID % 3];

                output.vertex = UnityObjectToClipPos(float4(input.position, 1));
                output.normal = writeTriangle.normal;

                return output;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = fixed4(0, 0, 0, 1);
                return col;
            }
            ENDCG
        }
    }
}
