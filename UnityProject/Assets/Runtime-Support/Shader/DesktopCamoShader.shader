Shader "Custom/DesktopCamoShader"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" { }
		_Color("Main Color", Color) = (1,1,1,1)

		// Camo
		_CamoBlackTint("Camo Pattern Black Tint", Color) = (0.41, 0.41, 0.21, 1.0)
		_CamoRedTint("Camo Pattern Red Tint", Color) = (0.19, 0.20, 0.13, 1.0)
		_CamoGreenTint("Camo Pattern Green Tint", Color) = (0.75, 0.64, 0.31, 1.0)
		_CamoBlueTint("Camo Pattern Blue Tint", Color) = (0.34, 0.23, 0.10, 1.0)
		_CamoPatternMap("Camo Pattern (RGB)", 2D) = "black" {}

		// Camo Texture
		[ToggleOff] _CamoColor("Camo Color",int) = 0
		_CamoColorTex("Camo Texture",2D) = "white" {}

		[KeywordEnum(UV1, UV2)] _UV_CHANNEL("Pattern UV-Channel", Float) = 0
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

		// Camo
		fixed4 _CamoBlackTint;
		fixed4 _CamoRedTint;
		fixed4 _CamoGreenTint;
		fixed4 _CamoBlueTint;
		sampler2D _CamoPatternMap;
		
		float4 _Color;

		sampler2D _CamoColorTex;
		int _CamoColor;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_CamoColorTex;

	#if _UV_CHANNEL_UV1
			fixed2 uv_CamoPatternMap;
	#else
			fixed2 uv2_CamoPatternMap;
	#endif
		};

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		fixed OverlayBlendMode(fixed basePixel, fixed blendPixel) {
			if (basePixel < 0.5) {
				return (2.0 * basePixel * blendPixel);
			} else {
				return (1.0 - 2.0 * (1.0 - basePixel) * (1.0 - blendPixel));
			}
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			//fixed4 m = tex2D(_MetallicGlossMap, IN.uv_MainTex);
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

	#if _UV_CHANNEL_UV1
			fixed4 camoPattern = tex2D(_CamoPatternMap, IN.uv_CamoPatternMap);
	#else
			fixed4 camoPattern = tex2D(_CamoPatternMap, IN.uv2_CamoPatternMap);
	#endif
			
			if(_CamoColor)
			{
				fixed4 camo = tex2D(_CamoColorTex,IN.uv_CamoColorTex);
				
				camo.r = OverlayBlendMode(camo.r,c.r);
				camo.g = OverlayBlendMode(camo.g,c.g);
				camo.b = OverlayBlendMode(camo.b,c.b);

				o.Albedo = camo * _Color;
			}
			else
			{
				// Camo 
				fixed4 camo = lerp(_CamoBlackTint, _CamoRedTint, camoPattern.r);
				camo = lerp(camo, _CamoGreenTint, camoPattern.g);
				camo = lerp(camo, _CamoBlueTint, camoPattern.b);

				o.Albedo = lerp(c.rgb,camo,camo.a*0.5f)*_Color;
			}

			o.Metallic = 0;//m.rgb;
			o.Smoothness = 0;//m.a;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
