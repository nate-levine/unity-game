Shader "Custom/PointLight"
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

            // Light position in world space.
            float3 _LightPos;
            // Light radius;
            float _LightInnerRadius;
            float _LightOuterRadius;
            // Light color
            float3 _LightInnerColor;
            float3 _LightOuterColor;

            // Positions of camera corners in world space.
            float3 _TopRight;
            float3 _BottomLeft;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 worldUV;
                // Lerp the i.uv between points in world space.
                worldUV.x = lerp(_BottomLeft.x, _TopRight.x, i.uv.x);
                worldUV.y = lerp(_BottomLeft.y, _TopRight.y, i.uv.y);
                // Find the distance from the light in world space.
                float dist = distance(float3(worldUV, 0), _LightPos);

                fixed4 col;
                // If the point is within the inner light radius, set it as the light color.
                if (dist < _LightInnerRadius)
                {
                    col = fixed4(_LightInnerColor, 1);
                }
                // If the point is within the inner and outer light radius, lerp between the two radii properties.
                else if (dist > _LightInnerRadius && dist < _LightOuterRadius)
                {
                    // Map the distance [_LightInnerRadius, _LightOuterRadius] -> [0, 1].
                    float strength = (dist - _LightOuterRadius) / (_LightInnerRadius - _LightOuterRadius);
                    // Inverse square law to mimic real lighting.
                    strength = pow(strength, 2);
                    // Lerp the colors based on strength.
                    float r = (strength * (_LightInnerColor.r - _LightOuterColor.r)) + _LightOuterColor.r;
                    float g = (strength * (_LightInnerColor.g - _LightOuterColor.g)) + _LightOuterColor.g;
                    float b = (strength * (_LightInnerColor.b - _LightOuterColor.b)) + _LightOuterColor.b;
                    // Multiply by the color.
                    col = fixed4(float3(r, g, b) * strength, 1);
                }
                // If the point is outside the outer light radius, return no lighting.
                else
                {
                    col = fixed4(0, 0, 0, 1);
                }

                return col;
            }
            ENDCG
        }
    }
}
