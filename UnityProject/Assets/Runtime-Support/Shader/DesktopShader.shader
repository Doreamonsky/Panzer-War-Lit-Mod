Shader "Custom/DesktopShader"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" { }
		_Color("Main Color", Color) = (1,1,1,1)
			//_MetallicGlossMap("Metallic", 2D) = "white" { }
	}
	
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows

		#pragma target 3.0

		sampler2D _MainTex;
		//sampler2D _MetallicGlossMap;
		sampler2D _BumpMap;
		float4 _Color;

		struct Input
		{
			float2 uv_MainTex;
		};



		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex)*_Color;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

			o.Albedo = c.rgb;
			o.Metallic = 0;//m.rgb;
			o.Smoothness = 0;//m.a;
			o.Alpha = c.a;
		}
		ENDCG
	}
	
	FallBack "Diffuse"
}
