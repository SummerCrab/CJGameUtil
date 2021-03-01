// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TA/SC/RimLight"
{
	//属性
	Properties{
		[HDR]_RimColor("RimColor", Color) = (1,1,1,1)
		_RimPower("RimPower", Range(0.000001, 3.0)) = 0.1
		_Threshold("_Threshold", Range(0.0,1.0)) = 0.1
	}

		//子着色器	
		SubShader
	{
		Pass
	{
		//定义Tags
		Tags{ "RenderType" = "Transparent" }
		ZWrite Off
		Blend One One

		CGPROGRAM
		//引入头文件
		#include "Lighting.cginc"
		//定义Properties中的变量
		fixed4 _Diffuse;
	
	fixed4 _RimColor;
	float _RimPower;
	fixed _Threshold;

	//定义结构体：vertex shader阶段输出的内容
	struct v2f
	{
		float4 pos : SV_POSITION;
		float3 worldNormal : TEXCOORD0;
		float2 uv : TEXCOORD1;
		//在vertex shader中计算观察方向传递给fragment shader
		float3 worldViewDir : TEXCOORD2;
	};

	//定义顶点shader,参数直接使用appdata_base（包含position, noramal, texcoord）
	v2f vert(appdata_base v)
	{
		v2f o;
		//内置宏初始化变量
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		//MVP变换
		o.pos = UnityObjectToClipPos(v.vertex);
		//法线会受到缩放影响，需要特殊处理
		o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);
		//顶点转化到世界空间
		float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
		//可以把计算计算ViewDir的操作放在vertex shader阶段，毕竟逐顶点计算比较省
		o.worldViewDir = _WorldSpaceCameraPos.xyz - worldPos;
		return o;
	}

	//定义片元shader
	fixed4 frag(v2f i) : SV_Target
	{
		//归一化法线，即使在vert归一化也不行，从vert到frag阶段有差值处理，传入的法线方向并不是vertex shader直接传出的
		fixed3 worldNormal = normalize(i.worldNormal);
		//把光照方向归一化
		fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
		//把视线方向归一化
		float3 worldViewDir = normalize(i.worldViewDir);
		//计算视线方向与法线方向的夹角，夹角越大，dot值越接近0，说明视线方向越偏离该点，也就是平视，该点越接近边缘
		float rim = 1 - max(0, dot(worldViewDir, worldNormal));
		//计算rimLight
		fixed3 rimColor = _RimColor * pow(rim, 1 / _RimPower);

		return fixed4(rimColor, _RimColor.a);
	}

	//使用vert函数和frag函数
	#pragma vertex vert
	#pragma fragment frag	

		ENDCG
	}
	}

		FallBack Off
}
