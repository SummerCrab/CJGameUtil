Shader "TA/SC/Dissolve"
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		// 噪声贴图
		_BurnMap("Burn Map", 2D) = "white" {}
		// 控制消融程度
		_BurnAmount("Burn Amount", Range(-1, 2)) = 0
		// 控制模型烧焦效果的线宽
		_LineWidth("Burn Line Width", Range(0.0, 0.2)) = 0.1
		// 烧焦边缘的颜色
		[HDR]_BurnColor("Burn Color", Color) = (1, 0, 0, 1)
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }

			// 所有面都不被剔除，因为消融的时候应该可以看到物体的内部。
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// 为得到正确的光照设置的编译指令
			#pragma multi_compile_fwdbase

			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			sampler2D _MainTex;
			sampler2D _BurnMap;
			float4 _MainTex_ST;
			float4 _BurnMap_ST;
			fixed _BurnAmount;
			fixed _LineWidth;
			fixed4 _BurnColor;

			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				fixed4 tangent : TANGENT;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uvMainTex : TEXCOORD0;
				float2 uvBurnMap : TEXCOORD2;
				// 切线视图的光照方向
				float3 lightDir : TEXCOORD3;
				float3 worldPos : TEXCOORD4;
				// Shader 阴影宏：用于对阴影纹理采样的坐标
				// 参数：一个可用的差值寄存器的索引值，用世界的顶点坐标。即5（从0开始）
				SHADOW_COORDS(5)
			};

			v2f vert(a2v v)
			{
				v2f o;
				//MVP转换
				o.pos = UnityObjectToClipPos(v.vertex);
				//贴图的offset和tiling
				o.uvMainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uvBurnMap = TRANSFORM_TEX(v.texcoord, _BurnMap);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//噪波贴图的采样
				fixed3 burn = tex2D(_BurnMap, i.uvBurnMap);
				// 透明裁切，阈值越大裁切越多
				clip(burn.r - _BurnAmount);
				//主贴图采样
				fixed3 albedo = tex2D(_MainTex, i.uvMainTex).rgb;

				// smoothsetp(e1,e2,x)平滑过度函数
				// 原理：if (x < e1) 结果 = 0
				// if (x > e2) 结果 = 1
				// if (e1 < x < e2) 结果 = 3 * pow(x, 2) - 2 * pow(x, 3)
				// t = 0时，该像素为正常的模型颜色
				// t = 1时，该像素位于消融的边界处
				fixed t = 1 - smoothstep(0.0, _LineWidth, burn.r - _BurnAmount);

				// 最终颜色，在消融和正常中差值，系数为t
				fixed3 finalColor = lerp(albedo, _BurnColor, t);
				return fixed4(finalColor, 1);
			}
			ENDCG
		}

		// 此Pass是处理阴影的，我们需要按照正常的Pass的处理来剔除片元或进行顶点动画。以便阴影可以和物体正常渲染的结果相匹配。
		Pass
		{
			// 光照模式定义为阴影
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// 处理阴影的编译指令
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			fixed _BurnAmount;
			sampler2D _BurnMap;
			float4 _BurnMap_ST;

			struct a2v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent: TANGENT;
			};

			struct v2f
			{
				// 定义阴影投射需要定义的变量
				V2F_SHADOW_CASTER;
				float2 uvBurnMap : TEXCOORD0;
			};

			v2f vert(a2v v)
			{
				v2f o;

				// 填充V2F_SHADOW_CASTER定义的变量
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				o.uvBurnMap = TRANSFORM_TEX(v.texcoord, _BurnMap);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed3 burn = tex2D(_BurnMap, i.uvBurnMap);
				clip(burn - _BurnAmount);
				// Unity投射阴影的内置宏
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}