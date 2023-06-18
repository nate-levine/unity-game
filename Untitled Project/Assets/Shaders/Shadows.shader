Shader "Custom/Shadows"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            // Prevent backface culling with Cull Off.
            Cull Off
            // Define blend mode of the old and new pixel colors.
            Blend SrcAlpha OneMinusSrcAlpha
            // Don't disable any objects behind the material, because those background objects may not be fully occluded.
            ZWrite Off
            // Check for transparency overlap. If there is, don't add the alphas.
            Stencil {
                Ref 0
                Comp Equal
                Pass IncrSat
                Fail IncrSat
            }

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
