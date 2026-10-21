Shader "SonicV/AspectCorrection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RenderTexture ("RenderTexture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "ShaderGraphShader"="true" "ShaderGraphTargetId"="UniversalFullscreenSubTarget"}

        ZTest False
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Blit"
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

            sampler2D _RenderTexture;
            float4 _RenderTexture_ST;

            float RenderedAspect;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                float2 uv = i.uv - 0.5;
                float aspect = (float)16/9;
                if(RenderedAspect > aspect) uv.x *= RenderedAspect / aspect;
                else if(RenderedAspect < aspect) uv.y *= aspect / RenderedAspect;                
                fixed4 col = tex2D(_MainTex, uv + 0.5);

                if(uv.x < -0.5 || uv.x > 0.5 || uv.y < -0.5 || uv.y > 0.5 ) col.rgb *= 0;
                
                return col;
            }
            ENDCG
        }
    }
}
