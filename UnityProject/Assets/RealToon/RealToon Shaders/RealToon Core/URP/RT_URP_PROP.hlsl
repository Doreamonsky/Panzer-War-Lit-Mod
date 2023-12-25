//RealToon URP - RT_URP_PROP
//MJQStudioWorks

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

//===============================================================================
//CBUF
//===============================================================================

CBUFFER_START(UnityPerMaterial)

	//== Others
		uniform float4 _MainTex_ST;

		uniform half4 _MainColor;
		uniform half _MaiColPo;
		uniform half _MVCOL;
		uniform half _MCIALO;
		uniform half _TexturePatternStyle;
		uniform half4 _HighlightColor;
		uniform half _HighlightColorPower;
	//==


	//== N_F_O_ON
		uniform float4 _OutlineWidthControl_ST;
		uniform half _OutlineWidth;
		uniform int _OutlineExtrudeMethod;
		uniform half3 _OutlineOffset;
		uniform half _OutlineZPostionInCamera;
		uniform half4 _OutlineColor;
		uniform half _MixMainTexToOutline;
		uniform half _NoisyOutlineIntensity;
		uniform half _DynamicNoisyOutline;
		uniform half _LightAffectOutlineColor;
		uniform half _OutlineWidthAffectedByViewDistance;
		uniform half _FarDistanceMaxWidth;
		uniform half _VertexColorBlueAffectOutlineWitdh;
	//==


	//==  N_F_O_SSOL
		uniform float _DepthThreshold;
	//==


	//== N_F_MC_ON
		uniform half _MCapIntensity;

		uniform float4 _MCap_ST;

		uniform half _SPECMODE;
		uniform half _SPECIN;

		uniform float4 _MCapMask_ST;
	//==


	//== Transparency
		uniform float4 _MaskTransparency_ST;
		uniform half _Opacity;
		uniform half _TransparentThreshold;
	//==


	//== N_F_CO_ON
		uniform half _Cutout;
		uniform half _AlphaBaseCutout;
		uniform half _UseSecondaryCutout;
		
		uniform half _Glow_Edge_Width;
		uniform half4 _Glow_Color;

		uniform float4 _SecondaryCutout_ST;
	//==


	//== N_F_NM_ON
		uniform float4 _NormalMap_ST;

		uniform half _NormalMapIntensity;
	//==


	//== N_F_CA_ON
		uniform half _Saturation;
	//== 


	//== N_F_SL_ON
		uniform half _SelfLitIntensity;
		uniform half4 _SelfLitColor;
		uniform half _SelfLitPower;
		uniform half _TEXMCOLINT;
		uniform half _SelfLitHighContrast;

		uniform float4 _MaskSelfLit_ST;
	//==


	//== N_F_GLO_ON
		uniform half _GlossIntensity;
		uniform half _Glossiness;
		uniform half _GlossSoftness;
		uniform half4 _GlossColor;
		uniform half _GlossColorPower;

		uniform float4 _MaskGloss_ST;
	//==


	//== N_F_GLO_ON -> N_F_GLOT_ON
		uniform float4 _GlossTexture_ST;

		uniform half _GlossTextureSoftness;
		uniform half _PSGLOTEX;
		uniform half _GlossTextureRotate;
		uniform half _GlossTextureFollowObjectRotation;
		uniform half _GlossTextureFollowLight;
	//==


	//== Others
		uniform half4 _OverallShadowColor;
		uniform half _OverallShadowColorPower;

		uniform half _SelfShadowShadowTAtViewDirection;

		uniform half _ShadowHardness;
		uniform half _SelfShadowRealtimeShadowIntensity;
	//==


	//== N_F_SS_ON
		uniform half _SelfShadowThreshold;
		uniform half _VertexColorGreenControlSelfShadowThreshold;
		uniform half _SelfShadowHardness;
		uniform half _LigIgnoYNorDir;
		uniform half _SelfShadowAffectedByLightShadowStrength;
	//==


	//== N_F_SS_ON -> Transparency
		uniform half _SelfShadowIntensity;
		uniform half4 _SelfShadowColor;
		uniform half _SelfShadowColorPower;
	//==


	//== Others
		uniform half4 _SelfShadowRealTimeShadowColor;
		uniform half _SelfShadowRealTimeShadowColorPower;
	//==


	//== N_F_SON_ON
		uniform half _SmoothObjectNormal;
		uniform half _VertexColorRedControlSmoothObjectNormal;
		uniform float4 _XYZPosition;
		uniform half _ShowNormal;
	//==


	//== N_F_SCT_ON
		uniform float4 _ShadowColorTexture_ST;

		uniform half _ShadowColorTexturePower;
	//==


	//== N_F_ST_ON
		uniform half _ShadowTIntensity;

		uniform float4 _ShadowT_ST;

		uniform half _ShadowTLightThreshold;
		uniform half _ShadowTShadowThreshold;
		uniform half4 _ShadowTColor;
		uniform half _ShadowTColorPower;
		uniform half _ShadowTHardness;
		uniform half _STIL;
		uniform half _ShowInAmbientLightShadowIntensity;
		uniform half _ShowInAmbientLightShadowThreshold;
		uniform half _LightFalloffAffectShadowT;
	//==


	//==  N_F_PT_ON
		uniform float4 _PTexture_ST;
		uniform half4 _PTCol;
		uniform half _PTexturePower;
	//==


	//==  N_F_RELGI_ON
		uniform half _GIFlatShade;
		uniform half _GIShadeThreshold;
		uniform half _EnvironmentalLightingIntensity;
	//==
		

	//== Others
		uniform half _LightAffectShadow;
		uniform half _LightIntensity;
		uniform half _DirectionalLightIntensity;
		uniform half _PointSpotlightIntensity;
		uniform half _LightFalloffSoftness;
	//==


	//== N_F_CLD_ON
		uniform half _CustomLightDirectionIntensity;
		uniform half4 _CustomLightDirection;
		uniform half _CustomLightDirectionFollowObjectRotation;
	//==


	//== N_F_R_ON
		uniform half _ReflectionIntensity;
		uniform half _ReflectionRoughtness;
		uniform half _RefMetallic;

		uniform float4 _MaskReflection_ST;
	//==


	//== N_F_FR_ON
		float4 _FReflection_ST;
	//==


	//== N_F_RL_ON
		uniform half _RimLigInt;
		uniform half _RimLightUnfill;
		uniform half _RimLightSoftness;
		uniform half _LightAffectRimLightColor;
		uniform half4 _RimLightColor;
		uniform half _RimLightColorPower;
		uniform half _RimLightInLight;
	//==

	//== N_F_NFD_ON
		uniform half _MinFadDistance;
		uniform half _MaxFadDistance;
	//==

	//== Others
		uniform half4 _SSAOColor;

		uniform half _ReduSha;
		uniform sampler3D _DitherMaskLOD;

		float _SkinMatrixIndex;
		float _ComputeMeshIndex;
	//==

CBUFFER_END

//===============================================================================
//DOTS Instancing
//===============================================================================
#ifdef UNITY_DOTS_INSTANCING_ENABLED
UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)

UNITY_DOTS_INSTANCED_PROP(float4, _MainColor)
UNITY_DOTS_INSTANCED_PROP(float, _MaiColPo)
UNITY_DOTS_INSTANCED_PROP(float, _MVCOL)
UNITY_DOTS_INSTANCED_PROP(float, _MCIALO)
UNITY_DOTS_INSTANCED_PROP(float, _TexturePatternStyle)
UNITY_DOTS_INSTANCED_PROP(float4, _HighlightColor)
UNITY_DOTS_INSTANCED_PROP(float, _HighlightColorPower)

UNITY_DOTS_INSTANCED_PROP(float, _OutlineWidth)
UNITY_DOTS_INSTANCED_PROP(int, _OutlineExtrudeMethod)
UNITY_DOTS_INSTANCED_PROP(float3, _OutlineOffset)
UNITY_DOTS_INSTANCED_PROP(float, _OutlineZPostionInCamera)
UNITY_DOTS_INSTANCED_PROP(float4, _OutlineColor)
UNITY_DOTS_INSTANCED_PROP(float, _MixMainTexToOutline)
UNITY_DOTS_INSTANCED_PROP(float, _NoisyOutlineIntensity)
UNITY_DOTS_INSTANCED_PROP(float, _DynamicNoisyOutline)
UNITY_DOTS_INSTANCED_PROP(float, _LightAffectOutlineColor)
UNITY_DOTS_INSTANCED_PROP(float, _OutlineWidthAffectedByViewDistance)
UNITY_DOTS_INSTANCED_PROP(float, _FarDistanceMaxWidth)
UNITY_DOTS_INSTANCED_PROP(float, _VertexColorBlueAffectOutlineWitdh)

UNITY_DOTS_INSTANCED_PROP(float, _DepthThreshold)

UNITY_DOTS_INSTANCED_PROP(float, _MCapIntensity);
UNITY_DOTS_INSTANCED_PROP(float, _SPECMODE)
UNITY_DOTS_INSTANCED_PROP(float, _SPECIN)

UNITY_DOTS_INSTANCED_PROP(float, _Opacity)
UNITY_DOTS_INSTANCED_PROP(float, _TransparentThreshold)

UNITY_DOTS_INSTANCED_PROP(float, _Cutout)
UNITY_DOTS_INSTANCED_PROP(float, _AlphaBaseCutout)
UNITY_DOTS_INSTANCED_PROP(float, _UseSecondaryCutout)

UNITY_DOTS_INSTANCED_PROP(float4, _Glow_Color)
UNITY_DOTS_INSTANCED_PROP(float, _Glow_Edge_Width)

UNITY_DOTS_INSTANCED_PROP(float, _NormalMapIntensity)

UNITY_DOTS_INSTANCED_PROP(float, _Saturation)

UNITY_DOTS_INSTANCED_PROP(float, _SelfLitIntensity)
UNITY_DOTS_INSTANCED_PROP(float4, _SelfLitColor)
UNITY_DOTS_INSTANCED_PROP(float, _SelfLitPower)
UNITY_DOTS_INSTANCED_PROP(float, _TEXMCOLINT)
UNITY_DOTS_INSTANCED_PROP(float, _SelfLitHighContrast)

UNITY_DOTS_INSTANCED_PROP(float, _GlossIntensity)
UNITY_DOTS_INSTANCED_PROP(float, _Glossiness)
UNITY_DOTS_INSTANCED_PROP(float, _GlossSoftness)
UNITY_DOTS_INSTANCED_PROP(float4, _GlossColor)
UNITY_DOTS_INSTANCED_PROP(float, _GlossColorPower)

UNITY_DOTS_INSTANCED_PROP(float, _GlossTextureSoftness)
UNITY_DOTS_INSTANCED_PROP(float, _PSGLOTEX)
UNITY_DOTS_INSTANCED_PROP(float, _GlossTextureRotate)
UNITY_DOTS_INSTANCED_PROP(float, _GlossTextureFollowObjectRotation)
UNITY_DOTS_INSTANCED_PROP(float, _GlossTextureFollowLight)

UNITY_DOTS_INSTANCED_PROP(float4, _OverallShadowColor)
UNITY_DOTS_INSTANCED_PROP(float, _OverallShadowColorPower)
UNITY_DOTS_INSTANCED_PROP(float, _SelfShadowShadowTAtViewDirection)
UNITY_DOTS_INSTANCED_PROP(float, _ShadowHardness)
UNITY_DOTS_INSTANCED_PROP(float, _SelfShadowRealtimeShadowIntensity)

UNITY_DOTS_INSTANCED_PROP(float, _SelfShadowThreshold)
UNITY_DOTS_INSTANCED_PROP(float, _VertexColorGreenControlSelfShadowThreshold)
UNITY_DOTS_INSTANCED_PROP(float, _SelfShadowHardness)
UNITY_DOTS_INSTANCED_PROP(float, _LigIgnoYNorDir)
UNITY_DOTS_INSTANCED_PROP(float, _SelfShadowAffectedByLightShadowStrength)

UNITY_DOTS_INSTANCED_PROP(float, _SelfShadowIntensity)
UNITY_DOTS_INSTANCED_PROP(float4, _SelfShadowColor)
UNITY_DOTS_INSTANCED_PROP(float, _SelfShadowColorPower)

UNITY_DOTS_INSTANCED_PROP(float4, _SelfShadowRealTimeShadowColor)
UNITY_DOTS_INSTANCED_PROP(float, _SelfShadowRealTimeShadowColorPower)

UNITY_DOTS_INSTANCED_PROP(float, _SmoothObjectNormal)
UNITY_DOTS_INSTANCED_PROP(float, _VertexColorRedControlSmoothObjectNormal)
UNITY_DOTS_INSTANCED_PROP(float4, _XYZPosition)
UNITY_DOTS_INSTANCED_PROP(float, _ShowNormal)

UNITY_DOTS_INSTANCED_PROP(float, _ShadowColorTexturePower)

UNITY_DOTS_INSTANCED_PROP(float, _ShadowTIntensity)
UNITY_DOTS_INSTANCED_PROP(float, _ShadowTLightThreshold)
UNITY_DOTS_INSTANCED_PROP(float, _ShadowTShadowThreshold)
UNITY_DOTS_INSTANCED_PROP(float4, _ShadowTColor)
UNITY_DOTS_INSTANCED_PROP(float, _ShadowTColorPower)
UNITY_DOTS_INSTANCED_PROP(float, _ShadowTHardness)
UNITY_DOTS_INSTANCED_PROP(float, _STIL)
UNITY_DOTS_INSTANCED_PROP(float, _ShowInAmbientLightShadowIntensity)
UNITY_DOTS_INSTANCED_PROP(float, _ShowInAmbientLightShadowThreshold)
UNITY_DOTS_INSTANCED_PROP(float, _LightFalloffAffectShadowT)

UNITY_DOTS_INSTANCED_PROP(float4, _PTCol)
UNITY_DOTS_INSTANCED_PROP(float, _PTexturePower)

UNITY_DOTS_INSTANCED_PROP(float, _GIFlatShade)
UNITY_DOTS_INSTANCED_PROP(float, _GIShadeThreshold)
UNITY_DOTS_INSTANCED_PROP(float, _EnvironmentalLightingIntensity)

UNITY_DOTS_INSTANCED_PROP(float, _LightAffectShadow)
UNITY_DOTS_INSTANCED_PROP(float, _LightIntensity)
UNITY_DOTS_INSTANCED_PROP(float, _DirectionalLightIntensity)
UNITY_DOTS_INSTANCED_PROP(float, _PointSpotlightIntensity)
UNITY_DOTS_INSTANCED_PROP(float, _LightFalloffSoftness)

UNITY_DOTS_INSTANCED_PROP(float, _CustomLightDirectionIntensity)
UNITY_DOTS_INSTANCED_PROP(float4, _CustomLightDirection)
UNITY_DOTS_INSTANCED_PROP(float, _CustomLightDirectionFollowObjectRotation)

UNITY_DOTS_INSTANCED_PROP(float, _ReflectionIntensity)
UNITY_DOTS_INSTANCED_PROP(float, _ReflectionRoughtness)
UNITY_DOTS_INSTANCED_PROP(float, _RefMetallic)

UNITY_DOTS_INSTANCED_PROP(float, _RimLigInt)
UNITY_DOTS_INSTANCED_PROP(float, _RimLightUnfill)
UNITY_DOTS_INSTANCED_PROP(float, _RimLightSoftness)
UNITY_DOTS_INSTANCED_PROP(float, _LightAffectRimLightColor)
UNITY_DOTS_INSTANCED_PROP(float4, _RimLightColor)
UNITY_DOTS_INSTANCED_PROP(float, _RimLightColorPower)
UNITY_DOTS_INSTANCED_PROP(float, _RimLightInLight)

UNITY_DOTS_INSTANCED_PROP(float, _MinFadDistance)
UNITY_DOTS_INSTANCED_PROP(float, _MaxFadDistance)

UNITY_DOTS_INSTANCED_PROP(float, _ReduceShadowSpotDirectionalLight)

UNITY_DOTS_INSTANCED_PROP(float4, _SSAOColor)

UNITY_DOTS_INSTANCED_PROP(float, _SkinMatrixIndex)
UNITY_DOTS_INSTANCED_PROP(float, _ComputeMeshIndex)

UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)


#define _MainColor										UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_MainColor)
#define _MaiColPo										UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_MaiColPo)
#define _MVCOL											UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_MVCOL)
#define _MCIALO											UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_MCIALO)
#define _TexturePatternStyle							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_TexturePatternStyle)
#define _HighlightColor									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_HighlightColor)
#define _HighlightColorPower							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_HighlightColorPower)

#define _OutlineWidth									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_OutlineWidth)
#define _OutlineExtrudeMethod							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(int, Metadata_OutlineExtrudeMethod)
#define _OutlineOffset									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float3, Metadata_OutlineOffset)
#define _OutlineZPostionInCamera						UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_OutlineZPostionInCamera)
#define _OutlineColor									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_OutlineColor)
#define _MixMainTexToOutline							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_MixMainTexToOutline)
#define _NoisyOutlineIntensity							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_NoisyOutlineIntensity)
#define _DynamicNoisyOutline							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_DynamicNoisyOutline)
#define _LightAffectOutlineColor						UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_LightAffectOutlineColor)
#define _OutlineWidthAffectedByViewDistance				UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_OutlineWidthAffectedByViewDistance)
#define _FarDistanceMaxWidth							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_FarDistanceMaxWidth)
#define _VertexColorBlueAffectOutlineWitdh				UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_VertexColorBlueAffectOutlineWitdh)

#define _DepthThreshold									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_DepthThreshold)

#define _MCapIntensity									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_MCapIntensity)
#define _SPECMODE										UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SPECMODE)
#define _SPECIN											UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SPECIN)

#define _Opacity										UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_Opacity)
#define _TransparentThreshold							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_TransparentThreshold)

#define _Cutout											UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_Cutout)
#define _AlphaBaseCutout								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_AlphaBaseCutout)
#define _UseSecondaryCutout								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_UseSecondaryCutout)

#define _Glow_Color										UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_Glow_Color)
#define _Glow_Edge_Width								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_Glow_Edge_Width)

#define _NormalMapIntensity								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_NormalMapIntensity)

#define _Saturation										UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_Saturation)

#define _SelfLitIntensity								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SelfLitIntensity)
#define _SelfLitColor									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_SelfLitColor)
#define _SelfLitPower									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SelfLitPower)
#define _TEXMCOLINT										UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_TEXMCOLINT)
#define _SelfLitHighContrast							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SelfLitHighContrast)

#define _GlossIntensity									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_GlossIntensity)
#define _Glossiness										UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_Glossiness)
#define _GlossSoftness									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_GlossSoftness)
#define _GlossColor										UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_GlossColor)
#define _GlossColorPower								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_GlossColorPower)

#define _GlossTextureSoftness							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_GlossTextureSoftness)
#define _PSGLOTEX										UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_PSGLOTEX)
#define _GlossTextureRotate								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_GlossTextureRotate)
#define _GlossTextureFollowObjectRotation				UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_GlossTextureFollowObjectRotation)
#define _GlossTextureFollowLight						UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_GlossTextureFollowLight)

#define _OverallShadowColor								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_OverallShadowColor)
#define _OverallShadowColorPower						UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_OverallShadowColorPower)
#define _SelfShadowShadowTAtViewDirection				UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SelfShadowShadowTAtViewDirection)
#define _ShadowHardness									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ShadowHardness)
#define _SelfShadowRealtimeShadowIntensity				UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SelfShadowRealtimeShadowIntensity)

#define _SelfShadowThreshold							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SelfShadowThreshold)
#define _VertexColorGreenControlSelfShadowThreshold		UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_VertexColorGreenControlSelfShadowThreshold)
#define _SelfShadowHardness								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SelfShadowHardness)
#define _LigIgnoYNorDir									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_LigIgnoYNorDir)
#define _SelfShadowAffectedByLightShadowStrength        UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SelfShadowAffectedByLightShadowStrength)

#define _SelfShadowIntensity							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SelfShadowIntensity)
#define _SelfShadowColor								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_SelfShadowColor)
#define _SelfShadowColorPower							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SelfShadowColorPower)

#define _SelfShadowRealTimeShadowColor					UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_SelfShadowRealTimeShadowColor)
#define _SelfShadowRealTimeShadowColorPower             UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SelfShadowRealTimeShadowColorPower)

#define _SmoothObjectNormal								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_SmoothObjectNormal)
#define _VertexColorRedControlSmoothObjectNormal        UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_VertexColorRedControlSmoothObjectNormal)
#define _XYZPosition									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_XYZPosition)
#define _ShowNormal										UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ShowNormal)

#define _ShadowColorTexturePower						UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ShadowColorTexturePower)

#define _ShadowTIntensity								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ShadowTIntensity)
#define _ShadowTLightThreshold							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ShadowTLightThreshold)
#define _ShadowTShadowThreshold							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ShadowTShadowThreshold)
#define _ShadowTColor									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_ShadowTColor)
#define _ShadowTColorPower								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ShadowTColorPower)
#define _ShadowTHardness								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ShadowTHardness)
#define _STIL											UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_STIL)
#define _ShowInAmbientLightShadowIntensity              UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ShowInAmbientLightShadowIntensity)
#define _ShowInAmbientLightShadowThreshold              UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ShowInAmbientLightShadowThreshold)
#define _LightFalloffAffectShadowT						UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_LightFalloffAffectShadowT)

#define _PTCol											UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_PTCol)
#define _PTexturePower									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_PTexturePower)

#define _GIFlatShade									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_GIFlatShade)
#define _GIShadeThreshold								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_GIShadeThreshold)
#define _EnvironmentalLightingIntensity					UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_EnvironmentalLightingIntensity)

#define _LightAffectShadow								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_LightAffectShadow)
#define _LightIntensity									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_LightIntensity)
#define _DirectionalLightIntensity						UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_DirectionalLightIntensity)
#define _PointSpotlightIntensity						UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_PointSpotlightIntensity)
#define _LightFalloffSoftness							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_LightFalloffSoftness)

#define _CustomLightDirectionIntensity					UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_CustomLightDirectionIntensity)
#define _CustomLightDirection							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_CustomLightDirection)
#define _CustomLightDirectionFollowObjectRotation       UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_CustomLightDirectionFollowObjectRotation)

#define _ReflectionIntensity							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ReflectionIntensity)
#define _ReflectionRoughtness							UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ReflectionRoughtness)
#define _RefMetallic									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_RefMetallic)

#define _RimLigInt										UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_RimLigInt)
#define _RimLightUnfill									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_RimLightUnfill)
#define _RimLightSoftness								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_RimLightSoftness)
#define _LightAffectRimLightColor						UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_LightAffectRimLightColor)
#define _RimLightColor									UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_RimLightColor)
#define _RimLightColorPower								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_RimLightColorPower)
#define _RimLightInLight								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_RimLightInLight)

#define _MinDissDistance								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_MinDissDistance)
#define _MaxDissDistance								UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_MaxFadDistance)

#define _ReduceShadowSpotDirectionalLight				UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_ReduceShadowSpotDirectionalLight)

#define _SSAOColor                                      UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_SSAOColor)

//=========
#define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(type, Metadata##var)
#else
#define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
//=========

#endif

//===============================================================================
//Non CBUF
//===============================================================================

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

TEXTURE2D(_MaskTransparency);
SAMPLER(sampler_MaskTransparency);

TEXTURE2D(_OutlineWidthControl);
SAMPLER(sampler_OutlineWidthControl);

#if N_F_MC_ON
	TEXTURE2D(_MCap);
	SAMPLER(sampler_MCap);

	TEXTURE2D(_MCapMask);
	SAMPLER(sampler_MCapMask);
#endif

#if N_F_CO_ON
	TEXTURE2D(_SecondaryCutout);
	SAMPLER(sampler_SecondaryCutout);
#endif

#if N_F_NM_ON
	TEXTURE2D(_NormalMap);
	SAMPLER(sampler_NormalMap);
#endif

#if N_F_SL_ON
	TEXTURE2D(_MaskSelfLit);
	SAMPLER(sampler_MaskSelfLit);
#endif

#if N_F_GLO_ON
	TEXTURE2D(_MaskGloss);
	SAMPLER(sampler_MaskGloss);
#endif

#if N_F_GLO_ON
	#if N_F_GLOT_ON
		TEXTURE2D(_GlossTexture);
		SAMPLER(sampler_GlossTexture);
	#endif
#endif

#if N_F_SCT_ON
	TEXTURE2D(_ShadowColorTexture);
	SAMPLER(sampler_ShadowColorTexture);
#endif

#if N_F_ST_ON
	TEXTURE2D(_ShadowT);
	SAMPLER(sampler_ShadowT);
#endif

#if N_F_PT_ON
	TEXTURE2D(_PTexture);
	SAMPLER(sampler_PTexture);
#endif

#if N_F_R_ON
	TEXTURE2D(_MaskReflection);
	SAMPLER(sampler_MaskReflection);
#endif

#if N_F_R_ON
	#if N_F_FR_ON
		TEXTURE2D(_FReflection);
		SAMPLER(sampler_FReflection);
	#endif
#endif