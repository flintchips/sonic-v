Shader "Resources/Utilities/PixelLine"
{
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct LineData { float4 coords; float4 color; };
            StructuredBuffer<LineData> _LineBuffer;
            float4 _RTSize; 

            struct v2f {
                float4 pos : SV_POSITION;
                float4 col : COLOR;
                float4 lineData : TEXCOORD0;
                float2 pixelPos : TEXCOORD1;
            };

            v2f vert (uint vid : SV_VertexID, uint iid : SV_InstanceID)
            {
                v2f o;
                LineData data = _LineBuffer[iid];
                
                // Create a bounding box for the line (same convention you had)
                float2 p1 = data.coords.xy;
                float2 p2 = data.coords.zw;
                float2 minP = min(p1, p2) - 1.0;
                float2 maxP = max(p1, p2) + 1.0;

                uint tri = vid % 6u;

                float2 uv;
                if (tri == 0u) uv = float2(0, 0);
                else if (tri == 1u) uv = float2(1, 0);
                else if (tri == 2u) uv = float2(1, 1);
                else if (tri == 3u) uv = float2(0, 0);
                else if (tri == 4u) uv = float2(1, 1);
                else              uv = float2(0, 1);


                float2 pos = lerp(minP, maxP, uv);

                // Same pixel->clip mapping as your original
                o.pos = float4((pos * _RTSize.zw) * 2.0 - 1.0, 0, 1);
                o.col = data.color;
                o.lineData = data.coords;
                o.pixelPos = pos;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 p = floor(i.pixelPos) + 0.5; // Current pixel center (interpolated)
                float2 a = i.lineData.xy + 0.5;     // Start pixel center
                float2 b = i.lineData.zw + 0.5;     // End pixel center

                // Segment distance check (Classic GPU line rasterization)
                float2 ba = b - a;
                float2 pa = p - a;

                // avoid divide-by-zero for zero-length lines
                float denom = max(dot(ba, ba), 0.0001);
                float h = saturate(dot(pa, ba) / denom);
                float2 d = pa - ba * h;
                
                // Bresenham "Stair" Fix:
                // squared 0.5px threshold (avoid sqrt)
                if (dot(d, d) > 0.25) discard;

                return i.col;
            }
            ENDHLSL
        }
    }
}
