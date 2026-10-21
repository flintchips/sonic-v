Shader"EditorBackgroundGrid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
		_camScale ("camScale", Float) = 5
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
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	o.color = v.color;
	return o;
}

float _camScale;

float roundToNearest (float a, float b)
{
	return ((a / b) * b);
}

fixed4 frag (v2f i) : SV_Target
{
				// sample the texture
	fixed4 col = tex2D(_MainTex, i.uv) * float4(0.55, 0.55, 0.78, 1);
                
				// checker pattern
	if (((floor(i.worldPos.x / 8) % 2 == 0) && (floor(i.worldPos.y / 8) % 2 == 0)) ||
		((floor(i.worldPos.x / 8 + 1) % 2 == 0) && (floor(i.worldPos.y / 8 + 1) % 2 == 0)))
	{
		float alphaStore = col.a;
		col = col * 0.8;
		col.a = alphaStore;
	}
	
	// 1x1 gridLines
	float width = 0.008 * (_camScale * 0.75);
	float4 gridColor = float4(1, 1, 1, 0.25);

	if (((abs(i.worldPos.x) + width / 2) % 1 <= width) ||
		((abs(i.worldPos.y) + width / 2) % 1 <= width))
	{
		float4 newCol = float4(0, 0, 0, 0);
		newCol.a = (1 - gridColor.a) * col.a + gridColor.a;
		newCol.r = ((1 - gridColor.a) * col.a * col.r + gridColor.a * gridColor.r) / newCol.a;
		newCol.g = ((1 - gridColor.a) * col.a * col.g + gridColor.a * gridColor.g) / newCol.a;
		newCol.b = ((1 - gridColor.a) * col.a * col.b + gridColor.a * gridColor.b) / newCol.a;

		col = newCol;
	}
	
	// 8x8 gridLines
	width = 0.016 * (_camScale * 0.75);
	gridColor = float4(1, 1, 1, 0.4);


	if (((abs(i.worldPos.x) + width / 2) % 8 <= width) ||
		((abs(i.worldPos.y) + width / 2) % 8 <= width))
	{
		float4 newCol = float4(0, 0, 0, 0);
		newCol.a = (1 - gridColor.a) * col.a + gridColor.a;
		newCol.r = ((1 - gridColor.a) * col.a * col.r + gridColor.a * gridColor.r) / newCol.a;
		newCol.g = ((1 - gridColor.a) * col.a * col.g + gridColor.a * gridColor.g) / newCol.a;
		newCol.b = ((1 - gridColor.a) * col.a * col.b + gridColor.a * gridColor.b) / newCol.a;

		col = newCol;
	}
    
	return col;
}
            ENDCG
        }
    }
}
