Shader "Custom/DesktopShader"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" { }
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

	struct Input
	{
		float2 uv_MainTex;
	};


	fixed4 _Color;

	UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_INSTANCING_BUFFER_END(Props)

	void surf(Input IN, inout SurfaceOutputStandard o)
	{
		// Albedo comes from a texture tinted by color
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		//fixed4 m = tex2D(_MetallicGlossMap, IN.uv_MainTex);
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
