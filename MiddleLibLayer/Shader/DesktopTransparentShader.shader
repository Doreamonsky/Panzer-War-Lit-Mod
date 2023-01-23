Shader "Custom/DesktopTransparentShader"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Alpha("Alpha", Range(0,1)) = 0.5
		_Color("Main Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows alpha:fade 

		#pragma target 3.0

		sampler2D _MainTex;
		float _Alpha;
		float4 _Color;

		struct Input
		{
			float2 uv_MainTex;
		};

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

			o.Albedo = c.rgb;
			o.Alpha = _Alpha;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
