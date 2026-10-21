Shader "SonicV/TileCheckerPattern"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
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
                if(((floor(i.worldPos.x) % 2 == 0) && (floor(i.worldPos.y) % 2 == 0)) || ((floor(i.worldPos.x + 1) % 2 == 0) && (floor(i.worldPos.y + 1) % 2 == 0)))
                {
                    float alphaStore = col.a;
                    col = col * 0.9;
                    col.a = alphaStore;
                }

                return col;
            }
            ENDCG
        }
    }
}
