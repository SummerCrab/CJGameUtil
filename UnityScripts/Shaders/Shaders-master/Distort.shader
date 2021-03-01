Shader "TA/SC/Distort"
{
	Properties
	{
		_Noise("Noise", 2D) = "white" {}
		_distortFactorTime("FactorTime",Range(0,5)) = 0.5
		_distortFactor("factor",Range(0.04,1)) = 0
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
		LOD 100
		Cull Off Lighting Off ZWrite Off
		//截屏操作（带字符串参数可以使同名的Pass在每帧至多执行一次，减少性能开销）
		GrabPass
		{
			"_BackgroundTexture"
		}
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
				float4 grabPos:TEXCOORD1;
			};

			sampler2D _Noise;
			float4 _Noise_ST;
			fixed _distortFactorTime;
			fixed _distortFactor;
			sampler2D _BackgroundTexture;
			v2f vert(appdata v)
			{
				v2f o;
				//MVP变换
				o.vertex = UnityObjectToClipPos(v.vertex);
				//噪波贴图的offset及tiling
				o.uv = TRANSFORM_TEX(v.uv, _Noise);
				//内置宏把全屏的贴图裁切到模型上
				o.grabPos = ComputeGrabScreenPos(o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//UV根据时间偏移采样（简单）采用法线贴图效果更好但是开销更大
				fixed4 col = tex2D(_Noise, i.uv += _Time.xy*_distortFactorTime);
				//将偏移叠加到抓图
				i.grabPos.xy += col.xy*_distortFactor;
				//使用Grabpass{"_BackgroundTexture"}减小开销，并根据偏移的uv采样
				half4 bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos);
				//输出最终颜色
				return bgcolor;
			}
			ENDCG
		}
	}
}