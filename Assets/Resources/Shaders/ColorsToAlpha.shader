Shader "SonicV/ColorsToAlpha"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _ReplaceCol0   ("toggleReplacingThis0", int) = 1
        _ReplaceCol1   ("toggleReplacingThis1", int) = 1
        _ReplaceCol2   ("toggleReplacingThis2", int) = 1
        _ReplaceCol3   ("toggleReplacingThis3", int) = 1

        _AlphaMaskCol0 ("Color0", Color) = (1, 0, 1, 1)
        _AlphaMaskCol1 ("Color1", Color) = (1, 0, 1, 1)
        _AlphaMaskCol2 ("Color2", Color) = (1, 0, 1, 1)
        _AlphaMaskCol3 ("Color3", Color) = (1, 0, 1, 1)
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            int _ReplaceCol0;
            int _ReplaceCol1;
            int _ReplaceCol2;
            int _ReplaceCol3;

            float4 _AlphaMaskCol0;
            float4 _AlphaMaskCol1;
            float4 _AlphaMaskCol2;
            float4 _AlphaMaskCol3;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            bool checkColor(fixed4 col, float4 replaceCol)
            {
                float4 newCol = col;
                if ((col.r > replaceCol.r - 0.005 && col.r < replaceCol.r + 0.005) && (col.g > replaceCol.g - 0.005 && col.g < replaceCol.g + 0.005) &&
                    (col.b > replaceCol.b - 0.005 && col.b < replaceCol.b + 0.005) && (col.a > replaceCol.a - 0.005 && col.a < replaceCol.a + 0.005))
                    return true;
                else 
                    return false;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                for(int j = 0; j < 4; j++)
                {
                    if(j == 0 && checkColor(col, _AlphaMaskCol0) && _ReplaceCol0 == 1) 
                        return float4(0, 0, 0, 0); 
                    if(j == 1 && checkColor(col, _AlphaMaskCol1) && _ReplaceCol1 == 1) 
                        return float4(0, 0, 0, 0); 
                    if(j == 2 && checkColor(col, _AlphaMaskCol2) && _ReplaceCol2 == 1) 
                        return float4(0, 0, 0, 0); 
                    if(j == 3 && checkColor(col, _AlphaMaskCol3) && _ReplaceCol3 == 1) 
                        return float4(0, 0, 0, 0); 
                }

                return col;
            }
            ENDCG
        }
    }
}
