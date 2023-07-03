Shader "Custom/LightShadowComposite"
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _LightTex;
            // Definition to check if there exists a shadow mask.
            #define SHADOW_MASK_IS_SET
            sampler2D _ShadowTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col;

                // Sample the texture.
                fixed4 LightTex = tex2D(_LightTex, i.uv);
                // If there is exists a shadow mask, use the shadow mask.
                #if defined(SHADOW_MASK_IS_SET)
                    fixed4 ShadowTex = tex2D(_ShadowTex, i.uv);
                // If not, set the shadow mask to nothing.
                #else
                    fixed4 ShadowTex = fixed4(1, 1, 1, 0);
                #endif

                /* Composite the shadow mask and the light map into a single map.
                 * A small amount of the light is allowed to bleed through the shadow mask.
                 */
                col = fixed4((LightTex * ShadowTex).xyz + (LightTex * 0.2).xyz , 1);

                return col;
            }
            ENDCG
        }
    }
}
