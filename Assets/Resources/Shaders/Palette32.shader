Shader "SonicV/Palette32"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _2ndColor ("Second Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags
		{
            "CanUseSpriteAtlas" = "True"
        }

        ZWrite Off
        Cull off
        Blend SrcAlpha OneMinusSrcAlpha

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
            
            float4 _from_colors[32];
            float4 _to_colors[32];

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            bool equalColors(float3 a, float3 b){
                if(abs(a.r-b.r) < .005){
                if(abs(a.b-b.b) < .005){
                if(abs(a.g-b.g) < .005){
                    return true;
                }}}
                return false;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                bool end = false;
                for(int i = 0; i < 32; i++){
                    if(!end){
                        if(equalColors(col.rgb, _from_colors[i].rgb))
                        {
                            col = _to_colors[i];
                            end = true;
                        }
                    }
                }

                return col;
            }
            ENDCG
        }
    }
}
