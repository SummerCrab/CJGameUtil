Shader "TA/SC/Depth/IntersectionHighlight"
{
	Properties
	{
		_MainTex("Texture", 2D) = "black" {}
		[HDR]_IntersectionColor("Intersection Color", Color) = (1,1,0,0)
		_IntersectionWidth("Intersection Width", Range(0, 5)) = 0.1
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" }
		LOD 100
		Zwrite Off
		Blend One One

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
			float4 screenPos : TEXCOORD1;
			float eyeZ : TEXCOORD2;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		sampler2D _CameraDepthTexture;
		fixed4 _IntersectionColor;
		float _IntersectionWidth;

		v2f vert(appdata v)
		{
			v2f o;
			//使用Unity内置宏进行MVP矩阵的转换
			o.vertex = UnityObjectToClipPos(v.vertex);
			//计算主贴图的Offset
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			//计算顶点在屏幕坐标系中的位置
			o.screenPos = ComputeScreenPos(o.vertex);
			//利用COMPUTE_EYEDEPTH内置宏计算顶点深度
			COMPUTE_EYEDEPTH(o.eyeZ);
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			//主贴图采样
			fixed4 col = tex2D(_MainTex, i.uv);
			//获取屏幕像素深度值
			float screenZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
			//计算距离相交处的宽度
			float halfWidth = _IntersectionWidth / 2;
			//计算相交处的颜色 saturate 把输入值限制到[0, 1]之间
			float diff = saturate(abs(i.eyeZ - screenZ) / halfWidth);
			//根据深度进行插值计算
			fixed4 finalColor = lerp(_IntersectionColor, col, diff);
			//输出最终颜色
			return finalColor;
		}
			ENDCG
		}
	}
}
