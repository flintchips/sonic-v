Shader "SonicV/BackgroundGrid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        ZTest False
        ZWrite Off
        Cull off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 vertex : SV_POSITION;
                float3 worldPos : uv;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul (unity_ObjectToWorld, v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                
                float width = (0.0625);
                float4 gridColor = col;
                gridColor.a = col.a;
                			// checker pattern
                if (((floor(i.worldPos.x / 8) % 2 == 0) && (floor(i.worldPos.y / 8) % 2 == 0)) ||
                    ((floor(i.worldPos.x / 8 + 1) % 2 == 0) && (floor(i.worldPos.y / 8 + 1) % 2 == 0)))
                {
                    gridColor = col;
                    float alphaStore = col.a;
                    col = col * 0.8;
                    col.a = alphaStore;
                }
                else{
                    gridColor = col * 0.8;
                    gridColor.a = col.a;
                }
                
                // 1x1 gridLines
                if (((floor(i.worldPos.x / width) * width) % 1 == 0) ||
                    ((floor(i.worldPos.y / width) * width) % 1 == 0))
                {
                    float alphaStore = col.a;
                    col = col * 0.9;
                    col.a = alphaStore;
                }

                 // 1x1 gridLines
                if (((floor(i.worldPos.x / width) * width) % 8 == 0) ||
                    ((floor(i.worldPos.y / width) * width) % 8 == 0))
                {
                    float alphaStore = col.a;
                    col = i.color * 1.5;
                    col.a = alphaStore;
                }
                return col;
            }
            ENDCG
        }
    }
}
