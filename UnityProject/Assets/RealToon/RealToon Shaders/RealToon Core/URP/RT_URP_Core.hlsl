//RealToon URP - Core
//MJQStudioWorks

//=========================

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Assets/RealToon/RealToon Shaders/RealToon Core/URP/RT_URP_PROP.hlsl"

half RTD_LVLC_F(float3 Light_Color_f3)
{

	#ifdef SHADER_API_MOBILE

		return saturate(dot(Light_Color_f3.rgb, float3(0.3, 0.59, 0.11)));

	#else

		float4 node_3149_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
		float4 node_3149_p = lerp(float4(float4(Light_Color_f3.rgb, 0.0).zy, node_3149_k.wz), float4(float4(Light_Color_f3.rgb, 0.0).yz, node_3149_k.xy), step(float4(Light_Color_f3.rgb, 0.0).z, float4(Light_Color_f3.rgb, 0.0).y));
		float4 node_3149_q = lerp(float4(node_3149_p.xyw, float4(Light_Color_f3.rgb, 0.0).x), float4(float4(Light_Color_f3.rgb, 0.0).x, node_3149_p.yzx), step(node_3149_p.x, float4(Light_Color_f3.rgb, 0.0).x));
		float node_3149_d = node_3149_q.x - min(node_3149_q.w, node_3149_q.y);
		float node_3149_e = 1.0e-10;
		half3 node_3149 = float3(abs(node_3149_q.z + (node_3149_q.w - node_3149_q.y) / (6.0 * node_3149_d + node_3149_e)), node_3149_d / (node_3149_q.x + node_3149_e), node_3149_q.x);

		return saturate(node_3149.b);

	#endif

}

half3 AL_GI(float3 N)
{

	return SampleSH(N);

}

float3 calcNorm(float3 pos)
{
	float3 vecTan = normalize(cross(pos, float3(1.01, 1.0, 1.0)));
	float3 vecBitan = normalize(cross(vecTan, pos));

	return normalize(cross(vecTan, vecBitan));
}

//Dither
void Dither_Float(float In, float4 ScreenPosition, out float Out)
{
	float2 uv = ScreenPosition.xy * _ScreenParams.xy;
	float DITHER_THRESHOLDS[16] =
	{
		1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
		13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
		4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
		16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
	};
	uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
	Out = In - DITHER_THRESHOLDS[index];
}

//Remap
void Remap_Float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
	Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

//EdgDet/SSOL
//Most of the lines are based on unity hdrp example
float EdgDet(float2 uv)
{

    float2 _ScreenSize = (1.0) / float2(_ScreenParams.r, _ScreenParams.g);

    float halfScaleFloor = floor(_OutlineWidth * 0.5);
    float halfScaleCeil = ceil(_OutlineWidth * 0.5);

    float2 bottomLeftUV = uv - float2(_ScreenSize.x, _ScreenSize.y) * halfScaleFloor;
    float2 topRightUV = uv + float2(_ScreenSize.x, _ScreenSize.y) * halfScaleCeil;
    float2 bottomRightUV = uv + float2(_ScreenSize.x * halfScaleCeil, -_ScreenSize.y * halfScaleFloor);
    float2 topLeftUV = uv + float2(-_ScreenSize.x * halfScaleFloor, _ScreenSize.y * halfScaleCeil);

	float depth0 = SampleSceneDepth(bottomLeftUV);
    float depth1 = SampleSceneDepth(topRightUV);
    float depth2 = SampleSceneDepth(bottomRightUV);
    float depth3 = SampleSceneDepth(topLeftUV);

    float depthDerivative0 = depth1 - depth0;
    float depthDerivative1 = depth3 - depth2;

    float edgeDepth = sqrt(pow(depthDerivative0, 2.0) + pow(depthDerivative1, 2.0)) * 100;
    edgeDepth = edgeDepth > (depth0 * (_DepthThreshold * 0.01)) ? 1 : 0;

    #ifdef N_F_CO_ON
		return edgeDepth;
	#elif !N_F_CO_ON && N_F_TRANS_ON
		return 0.0;
	#else
		return edgeDepth;
	#endif

}

//DOTS_LinBlenSki
uniform StructuredBuffer<float3x4> _SkinMatrices; 

void DOTS_LiBleSki(uint4 indices, float4 weights, float3 positionIn, float3 normalIn, float3 tangentIn, out float3 positionOut, out float3 normalOut, out float3 tangentOut)
{
	for (int i = 0; i < 4; ++i)
	{
		float3x4 skinMatrix = _SkinMatrices[indices[i] + asint(UNITY_ACCESS_HYBRID_INSTANCED_PROP(_SkinMatrixIndex, float))];
		float3 vtransformed = mul(skinMatrix, float4(positionIn, 1));
		float3 ntransformed = mul(skinMatrix, float4(normalIn, 0));
		float3 ttransformed = mul(skinMatrix, float4(tangentIn, 0));

		positionOut += vtransformed * weights[i];
		normalOut += ntransformed * weights[i];
		tangentOut += ttransformed * weights[i];
	}
}

//DOTS_Compdef
struct DeformedVertexData
{
	float3 Position;
	float3 Normal;
	float3 Tangent;
};

uniform StructuredBuffer<DeformedVertexData> _DeformedMeshData : register(t1);

void DOTS_CompDef(uint vertexID, out float3 positionOut, out float3 normalOut, out float3 tangentOut)
{
	const DeformedVertexData vertexData = _DeformedMeshData[asuint(UNITY_ACCESS_HYBRID_INSTANCED_PROP(_ComputeMeshIndex, float)) + vertexID];
	positionOut = vertexData.Position;
	normalOut = vertexData.Normal;
	tangentOut = vertexData.Tangent;
}

//RT NM
float3 RT_NM(float2 uv)
{
#if N_F_NM_ON

	half3 _NormalMap_var = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, TRANSFORM_TEX(uv, _NormalMap)));
	float3 normalLocal = lerp(half3(0.0, 0.0, 1.0), _NormalMap_var.rgb, _NormalMapIntensity);

	return normalLocal;

#else

	return float3(0.0, 0.0, 1.0);

#endif

}
//

//RT_MCAP
half3 RT_MCAP(float2 uv, float3 normalDirection)
{
	#if N_F_MC_ON 

		half2 MUV = (mul(UNITY_MATRIX_V, float4(normalDirection, 0.0)).xyz.rgb.rg * 0.5 + 0.5);
		half4 _MatCap_var = SAMPLE_TEXTURE2D(_MCap, sampler_MCap, TRANSFORM_TEX(MUV, _MCap));
		half4 _MCapMask_var = SAMPLE_TEXTURE2D(_MCapMask, sampler_MCapMask, TRANSFORM_TEX(uv, _MCapMask));

		half3 RT_SPECMO_OO;
		if (!_SPECMODE)
		{
			RT_SPECMO_OO = (float3)1.0;
		}
		else
		{
			RT_SPECMO_OO = (float3)0.0;
		}

		float3 MCapOutP = lerp(RT_SPECMO_OO, lerp(RT_SPECMO_OO, _MatCap_var.rgb, _MCapIntensity), _MCapMask_var.rgb);

		return MCapOutP;

	#else

		return (half3)1.0;

	#endif
}
//

//RT_MCAP_SUB1
half3 RT_MCAP_SUB1(half3 MCapOutP, half4 _MainTex_var, half3 _RTD_MVCOL, out half3 RTD_TEX_COL)
{
	#if N_F_MC_ON 

		half3 RT_SPECMO_OO;
		if (!_SPECMODE)
		{
			RT_SPECMO_OO = (_MainColor.rgb * _MaiColPo) * MCapOutP;
		}
		else
		{
			RT_SPECMO_OO = (_MainColor.rgb * _MaiColPo) + (MCapOutP * _SPECIN);
		}

		half3 RT_SPECMO_OO_2;
		if (!_SPECMODE)
		{
			RT_SPECMO_OO_2 = MCapOutP;
		}
		else
		{
			RT_SPECMO_OO_2 = MCapOutP * _SPECIN;
		}

		half3 SPECMode_Sel;
		if (!_MCIALO)
		{
			SPECMode_Sel = RT_SPECMO_OO;
		}
		else
		{
			SPECMode_Sel = RT_SPECMO_OO_2;
		}

		RTD_TEX_COL = _MainTex_var.rgb * SPECMode_Sel * _RTD_MVCOL;

		half3 RTD_MCIALO_IL = RTD_TEX_COL;

		return RTD_MCIALO_IL;

	#else

		RTD_TEX_COL = _MainTex_var.rgb * (_MainColor.rgb * _MaiColPo) * MCapOutP * _RTD_MVCOL;

		half3 RTD_MCIALO_IL;
		if (!_MCIALO)
		{
			RTD_MCIALO_IL = RTD_TEX_COL;
		}
		else
		{
			RTD_MCIALO_IL = _MainTex_var.rgb * MCapOutP * _RTD_MVCOL;
		}

		return RTD_MCIALO_IL;

	#endif
}
//

//RT TRANS CO
void RT_TRANS_CO( float2 uv , half4 _MainTex_var , out half RTD_TRAN_OPA_Sli , half RTD_CO, inout half3 GLO_OUT)
{

	RTD_TRAN_OPA_Sli = 1.0;
	GLO_OUT = (half3)0.0;

	#if N_F_TRANS_ON

		#if N_F_CO_ON

			half4 _SecondaryCutout_var = SAMPLE_TEXTURE2D(_SecondaryCutout, sampler_SecondaryCutout ,TRANSFORM_TEX(uv,_SecondaryCutout));

			half RT_USSECCUT_OO;
			if (!_UseSecondaryCutout)
			{
				RT_USSECCUT_OO = _MainTex_var.r * _SecondaryCutout_var.r;
			}
			else
			{
				RT_USSECCUT_OO = _SecondaryCutout_var.r;
			}

			half RT_USSECCUT_OO_2;
			if (!_UseSecondaryCutout)
			{
				RT_USSECCUT_OO_2 = _MainTex_var.a * _SecondaryCutout_var.r;
			}
			else
			{
				RT_USSECCUT_OO_2 = _SecondaryCutout_var.a;
			}

			half RTD_CO_ON = (half)lerp((RT_USSECCUT_OO + lerp(0.5, (-1.0), _Cutout)), saturate(((1.0 - _Cutout) > 0.5 ? (1.0 - (1.0 - 2.0 * ((1.0 - _Cutout) - 0.5)) * ( 1.0 - RT_USSECCUT_OO_2)) : (2.0 * (1.0 - _Cutout) * RT_USSECCUT_OO_2))), _AlphaBaseCutout);
			RTD_CO = RTD_CO_ON;

			//GLOW
			#ifdef N_F_COEDGL_ON
				half _Glow_Edge_Width_Val = (1.0 - _Glow_Edge_Width);
				half _Glow_Edge_Width_Add_Input_Value = (_Glow_Edge_Width_Val + RTD_CO);
				half _Remapping = (_Glow_Edge_Width_Add_Input_Value * 8.0 + -4.0);
				half _Pre_Output = (1.0 - saturate(_Remapping));
				half3 _Final_Output = (_Pre_Output * lerp(0.0, _Glow_Color.rgb, saturate(_Cutout * 200.0))  );
				GLO_OUT = _Final_Output;
			#endif

			clip(RTD_CO - 0.5);

		#else

			half4 _MaskTransparency_var = SAMPLE_TEXTURE2D(_MaskTransparency, sampler_MaskTransparency, TRANSFORM_TEX(uv, _MaskTransparency));

			//Backup (Old)
			//half RTD_TRAN_MAS = (smoothstep(clamp(-20.0, 1.0, _TransparentThreshold), 1.0, _MainTex_var.a) * _MaskTransparency_var.r);
			//RTD_TRAN_OPA_Sli = lerp(RTD_TRAN_MAS, smoothstep(clamp(-20.0, 1.0, _TransparentThreshold), 1.0, _MainTex_var.a), _Opacity);

			#ifdef N_F_SIMTRANS_ON
				RTD_TRAN_OPA_Sli = _MainTex_var.a * _Opacity;
			#else
				RTD_TRAN_OPA_Sli = lerp(smoothstep(clamp(-20.0, 1.0, _TransparentThreshold), 1.0, _MainTex_var.a) * _Opacity, 1.0, _MaskTransparency_var.r);
			#endif

		#endif

	#endif

}
//

//RT_CO
void RT_CO(float2 uv, half4 _MainTex_var)
{
	#if N_F_TRANS_ON

		#if N_F_CO_ON

			half4 _SecondaryCutout_var = SAMPLE_TEXTURE2D(_SecondaryCutout, sampler_SecondaryCutout, TRANSFORM_TEX(uv, _SecondaryCutout));

			half RT_USSECCUT_OO;
			if (!_UseSecondaryCutout)
			{
				RT_USSECCUT_OO = _MainTex_var.r * _SecondaryCutout_var.r;
			}
			else
			{
				RT_USSECCUT_OO = _SecondaryCutout_var.r;
			}

			half RT_USSECCUT_OO_2;
			if (!_UseSecondaryCutout)
			{
				RT_USSECCUT_OO_2 = _MainTex_var.a * _SecondaryCutout_var.r;
			}
			else
			{
				RT_USSECCUT_OO_2 = _SecondaryCutout_var.a;
			}

			half RTD_CO_ON = (half)lerp(( RT_USSECCUT_OO + lerp(0.5, (-1.0), _Cutout)), saturate(((1.0 - _Cutout) > 0.5 ? (1.0 - (1.0 - 2.0 * ((1.0 - _Cutout) - 0.5)) * (1.0 - RT_USSECCUT_OO_2 )) : (2.0 * (1.0 - _Cutout) * RT_USSECCUT_OO_2 ))), _AlphaBaseCutout);

			clip(RTD_CO_ON - 0.5);

		#endif

	#endif
}

//RT SON
float3 RT_SON(float4 vertexColor, float3 calNorm, float3 normalDirection, out float3 RTD_SON_CHE_1)
{

	RTD_SON_CHE_1 = float3(1.0, 1.0, 1.0);

#if N_F_SON_ON

	float RTD_SON_VCBCSON_OO;
	if (!_VertexColorRedControlSmoothObjectNormal)
	{
		RTD_SON_VCBCSON_OO = _SmoothObjectNormal;
	}
	else
	{
		RTD_SON_VCBCSON_OO = _SmoothObjectNormal * (1.0 - vertexColor.r);
	}

	float3 RTD_SON_ON_OTHERS = lerp(normalDirection, TransformObjectToWorldNormal(-calNorm), RTD_SON_VCBCSON_OO);

	float3 RTD_SNorm_OO;
	if (!_ShowNormal)
	{
		RTD_SNorm_OO = (float3)1.0;
	}
	else
	{
		RTD_SNorm_OO = smoothstep(0.0, 0.01, RTD_SON_ON_OTHERS);
	}

	RTD_SON_CHE_1 = RTD_SNorm_OO;

	float3 RTD_SON = RTD_SON_ON_OTHERS;

	return RTD_SON;

#else

	float3 RTD_SON = normalDirection;

	return RTD_SON;

#endif

}
//

//RT_RELGI
float3 RT_RELGI( float3 RTD_SON )
{

	#if N_F_RELGI_ON

		half3 RTD_GI_ST_Sli = (RTD_SON*_GIShadeThreshold);

		half3 RTD_GI_FS_OO;
		if (!_GIFlatShade)
		{
			RTD_GI_FS_OO = RTD_GI_ST_Sli;
		}
		else
		{
			RTD_GI_FS_OO = half3(smoothstep(float2(0.0, 0.0), float2(0.01, 0.01), (RTD_SON.rb * _GIShadeThreshold)), 0.0);
		}


		return RTD_GI_FS_OO;

	#else

		half3 RTD_GI_FS_OO = RTD_SON;

		return RTD_GI_FS_OO;

	#endif

}

//RT_SCT
half3 RT_SCT( float2 uv , half3 RTD_MCIALO_IL )
{

	#ifndef N_F_OFLMB_ON

		#if N_F_SCT_ON
            	
			half4 _ShadowColorTexture_var = SAMPLE_TEXTURE2D(_ShadowColorTexture, sampler_ShadowColorTexture ,TRANSFORM_TEX(uv,_ShadowColorTexture)); 
			half3 RTD_SCT_ON = lerp(_ShadowColorTexture_var.rgb,(_ShadowColorTexture_var.rgb*_ShadowColorTexture_var.rgb),_ShadowColorTexturePower);

			half3 RT_MCIALO_OO;
			if (!_MCIALO)
			{
				RT_MCIALO_OO = (_MainColor.rgb * _MaiColPo);
			}
			else
			{
				RT_MCIALO_OO = (half3)1.0;
			}

			half3 RTD_SCT = RTD_SCT_ON * RT_MCIALO_OO;

			return RTD_SCT;
            
		#else
            
			half3 RTD_SCT = RTD_MCIALO_IL;

			return RTD_SCT;
            
		#endif

	#else

		half3 RTD_SCT = RTD_MCIALO_IL;

		return RTD_SCT;

	#endif

}
//

//RT_PT
half RT_PT( float2 RTD_VD_Cal , out half3 RTD_PT_COL )
{

	RTD_PT_COL = half3(1.0,1.0,1.0);

	#ifndef N_F_OFLMB_ON

		#if N_F_PT_ON

			half4 _PTexture_var = SAMPLE_TEXTURE2D(_PTexture, sampler_PTexture ,TRANSFORM_TEX(RTD_VD_Cal,_PTexture));  
			half RTD_PT_ON = lerp((1.0 - _PTexturePower),1.0,_PTexture_var.r);
			RTD_PT_COL = _PTCol.rgb;
            
			half RTD_PT = RTD_PT_ON;

			return RTD_PT;
            
		#else
            
			half RTD_PT = 1.0;

			return RTD_PT;
            
		#endif

	#else

		half RTD_PT = 1.0;

		return RTD_PT;

	#endif
}
//

//RT_CLD
float3 RT_CLD( float3 lightDirection )
{

	#ifndef N_F_OFLMB_ON

		#if N_F_CLD_ON

			float3 RTD_CLD_CLDFOR_OO;
			if (!_CustomLightDirectionFollowObjectRotation)
			{
				RTD_CLD_CLDFOR_OO = _CustomLightDirection.xyz;
			}
			else
			{
				RTD_CLD_CLDFOR_OO = mul(unity_ObjectToWorld, float4(_CustomLightDirection.xyz, 0.0)).xyz;
			}
			
			float3 RTD_CLD_CLDI_Sli = lerp(lightDirection,RTD_CLD_CLDFOR_OO,_CustomLightDirectionIntensity); 
			float3 RTD_CLD = RTD_CLD_CLDI_Sli;

			return RTD_CLD;
            
		#else
            
			float3 RTD_CLD = lightDirection;

			return RTD_CLD;
            
		#endif

	#else

		float3 RTD_CLD = lightDirection;

		return RTD_CLD;

	#endif

}
//

//RT_GLO
void RT_GLO( float2 uv , float2 RTD_VD_Cal , float3 halfDirection , float3 normalDirection, float3 viewDirection , out half RTD_GLO , out half3 RTD_GLO_COL)
{

	#ifndef N_F_OFLMB_ON

		#if N_F_GLO_ON

			#if N_F_GLOT_ON

				//#ifndef SHADER_API_MOBILE
					float _5992_ang = _GlossTextureRotate;
					float _5992_spd = 1.0;
					float _5992_cos = cos(_5992_spd*_5992_ang);
					float _5992_sin = sin(_5992_spd*_5992_ang);
					float2 _5992_piv = float2(0.5,0.5);
				//#endif

					half3 RTD_GT_FL_Sli;
					if (!_GlossTextureFollowLight)
					{
						RTD_GT_FL_Sli = viewDirection;
					}
					else
					{
						RTD_GT_FL_Sli = halfDirection;
					}

					half3 RefGlo = reflect(RTD_GT_FL_Sli,normalDirection);

					half3 RTD_GT_FOR_OO;
					if (!_GlossTextureFollowObjectRotation)
					{
						RTD_GT_FOR_OO = RefGlo;
					}
					else
					{
						RTD_GT_FOR_OO = mul(unity_WorldToObject, float4(RefGlo, 0.0)).xyz;
					}


				//#ifndef SHADER_API_MOBILE
					half2 glot_rot_cal = (mul(float2((-1* RTD_GT_FOR_OO.r), RTD_GT_FOR_OO.g)-_5992_piv,float2x2( _5992_cos, -_5992_sin, _5992_sin, _5992_cos))+_5992_piv);
					half2 glot_rot_out = (glot_rot_cal*0.5+0.5);
				//#endif

				//#ifdef SHADER_API_MOBILE
					//half4 _GlossTexture_var = SAMPLE_TEXTURE2D_LOD(_GlossTexture, sampler_GlossTexture, TRANSFORM_TEX(lerp((float2((-1 * RefGlo.r), RefGlo.g) * 0.5 + 0.5), RTD_VD_Cal, _PSGLOTEX), _GlossTexture), _GlossTextureSoftness);
				//#else
					half2 PSGLOTEX_Sel;
					if (!_PSGLOTEX)
					{
						PSGLOTEX_Sel = glot_rot_out;
					}
					else
					{
						PSGLOTEX_Sel = RTD_VD_Cal;
					}

					half4 _GlossTexture_var = SAMPLE_TEXTURE2D_LOD(_GlossTexture, sampler_GlossTexture, TRANSFORM_TEX(PSGLOTEX_Sel, _GlossTexture), _GlossTextureSoftness);
				//#endif

				half RTD_GT_ON = _GlossTexture_var.r;

				half3 RTD_GT = RTD_GT_ON;
            
			#else

				half RTD_GLO_MAIN_Sof_Sli = lerp( 0.1 , 1.0 ,_GlossSoftness);
				half RTD_NDOTH = saturate(dot(halfDirection, normalDirection));
				half RTD_GLO_MAIN = smoothstep( 0.1, RTD_GLO_MAIN_Sof_Sli, pow(RTD_NDOTH,exp2(lerp(-2.0,15.0,_Glossiness))) );

				half3 RTD_GT = RTD_GLO_MAIN;
            
			#endif

			half RTD_GLO_I_Sli = lerp(0.0, (half)RTD_GT,_GlossIntensity);
			half4 _MaskGloss_var = SAMPLE_TEXTURE2D(_MaskGloss, sampler_MaskGloss, TRANSFORM_TEX(uv, _MaskGloss));

			//
			#ifdef UNITY_COLORSPACE_GAMMA
				_GlossColor = float4(LinearToGamma22(_GlossColor.rgb), _GlossColor.a);
			#endif

			RTD_GLO_COL = (_GlossColor.rgb*_GlossColorPower); 
			//

			half RTD_GLO_MAS = lerp( 0.0, RTD_GLO_I_Sli ,_MaskGloss_var.r);

			RTD_GLO = RTD_GLO_MAS;

		#else

			RTD_GLO_COL = (half3)1.0;
			RTD_GLO = 0.0;
 
		#endif

	#else

		RTD_GLO_COL = (half3)1.0;
		RTD_GLO = 0.0;

	#endif

}
//

//RT_RL
half RT_RL(float3 viewDirection , float3 normalDirection , half3 lightColor , out half3 RTD_RL_LARL_OO , out half RTD_RL_MAIN)
{

	RTD_RL_MAIN = 0.0;

	#if N_F_RL_ON

		//
		#ifdef UNITY_COLORSPACE_GAMMA
			_RimLightColor = float4(LinearToGamma22(_RimLightColor.rgb), _RimLightColor.a);
		#endif

		half3 RT_LARLC_OO;
		if (!_LightAffectRimLightColor)
		{
			RT_LARLC_OO = _RimLightColor.rgb;
		}
		else
		{
			RT_LARLC_OO = lerp(half3(0.0, 0.0, 0.0), _RimLightColor.rgb, lightColor);
		}

		RTD_RL_LARL_OO = RT_LARLC_OO * _RimLightColorPower;
		//


		half RTD_RL_S_Sli = lerp(1.70,0.29,_RimLightSoftness);
		RTD_RL_MAIN = lerp(0.0, 1.0 ,smoothstep( 1.71, RTD_RL_S_Sli, pow(abs( 1.0-max(0,dot(normalDirection, viewDirection) ) ), (1.0 - _RimLightUnfill) ) ) );
					
		half RTD_RL_IL_OO = lerp( 0.0, RTD_RL_MAIN, _RimLigInt);

		half RTD_RL_CHE_1 = RTD_RL_IL_OO;

		return RTD_RL_CHE_1;
            
	#else
					
		RTD_RL_LARL_OO = (half3)1.0;

		half RTD_RL_CHE_1 = 0.0;

		return RTD_RL_CHE_1;
            
	#endif

}

//RT_ST
half RT_ST ( float2 uv , half RTD_NDOTL, half attenuation , half RTD_LVLC , half3 RTD_PT_COL , half3 lightColint , half3 RTD_SCT , half3 RTD_OSC , half RTD_PT , out half3 RTD_SHAT_COL , out half RTD_STIAL , out half RTD_ST_IS , out half3 RTD_ST_LAF)
{

	#ifndef N_F_OFLMB_ON

		#if N_F_ST_ON

			float4 _ShadowT_var = SAMPLE_TEXTURE2D(_ShadowT, sampler_ShadowT , TRANSFORM_TEX(uv,_ShadowT));


			//
			#ifdef UNITY_COLORSPACE_GAMMA
				_ShadowTColor = float4(LinearToGamma22(_ShadowTColor.rgb), _ShadowTColor.a);
			#endif

			RTD_SHAT_COL = lerp( RTD_PT_COL, (_ShadowTColor.rgb*_ShadowTColorPower) * RTD_SCT * RTD_OSC, RTD_PT);
			//

			if (!_LightAffectShadow)
			{
				RTD_ST_LAF = RTD_SHAT_COL * RTD_LVLC;
			}
			else
			{
				RTD_ST_LAF = RTD_SHAT_COL * lightColint;
			}

			half RTD_ST_H_Sli = lerp(0.0,0.22,_ShadowTHardness);

			half RTD_ST_IS_ON = (half)smoothstep( RTD_ST_H_Sli, 0.22, (_ShowInAmbientLightShadowThreshold*_ShadowT_var.rgb) );

			#if N_F_STIAL_ON

				half RTD_ST_ALI_Sli = lerp(1.0,RTD_ST_IS_ON,_ShowInAmbientLightShadowIntensity);
				half RTD_STIAL_ON = (half)lerp(RTD_ST_ALI_Sli,half3(1.0,1.0,1.0),clamp((RTD_LVLC*8.0),0.0,1.0));

				RTD_STIAL = RTD_STIAL_ON;
            
			#else
            
				RTD_STIAL = 1.0;
            
			#endif

			#if N_F_STIS_ON
            
				RTD_ST_IS = lerp(1.0,RTD_ST_IS_ON,_ShowInAmbientLightShadowIntensity);
            
			#else
            
				RTD_ST_IS = 1.0;
            
			#endif

			half RT_LFOAST_OO;
			if (!_TexturePatternStyle)
			{
				RT_LFOAST_OO = RTD_NDOTL;
			}
			else
			{
				RT_LFOAST_OO = attenuation * RTD_NDOTL;
			}

			half RTD_ST_LFAST_OO;
			if (!_STIL)
			{
				RTD_ST_LFAST_OO = RT_LFOAST_OO;
			}
			else
			{
				RTD_ST_LFAST_OO = 1.0;
			}

			half RTD_ST_In_Sli = lerp( 1.0 ,smoothstep( RTD_ST_H_Sli, 0.22, ((_ShadowT_var.r*(1.0 - _ShadowTShadowThreshold))*(RTD_ST_LFAST_OO *_ShadowTLightThreshold*0.01)) ),_ShadowTIntensity);
			half RTD_ST_ON = RTD_ST_In_Sli;

			half RTD_ST = RTD_ST_ON;

			return RTD_ST;
            
		#else
            
			half RTD_ST = 1.0;
			RTD_SHAT_COL = (half3)1.0;
			RTD_ST_LAF = (half3)1.0;
			RTD_STIAL = 1.0;
			RTD_ST_IS = 1.0;

			return RTD_ST;
            
		#endif

	#else

		half RTD_ST = 1.0;
		RTD_SHAT_COL = (half3)1.0;
		RTD_ST_LAF = (half3)1.0;
		RTD_STIAL = 1.0;
		RTD_ST_IS = 1.0;

		return RTD_ST;

	#endif
}
//

//RT_SS
half RT_SS( float4 vertexColor , float3 RTD_NDOTL , half attenuation , float dim_val )
{

	#ifndef N_F_OFLMB_ON

		#if N_F_SS_ON
 
			half RTD_SS_SSH_Sil = lerp(0.3,1.0,_SelfShadowHardness);
			half RTD_SS_SSTH_Sli = lerp(-1.0, 1.0, _SelfShadowThreshold);

			half RTD_SS_VCGCSSS_OO;
			if (!_VertexColorGreenControlSelfShadowThreshold)
			{
				RTD_SS_VCGCSSS_OO = RTD_SS_SSTH_Sli;
			}
			else
			{
				RTD_SS_VCGCSSS_OO = RTD_SS_SSTH_Sli * (1.0 - vertexColor.g);
			}

			half RTD_SS_SST = smoothstep( RTD_SS_SSH_Sil, 1.0, ((float)RTD_NDOTL * lerp(7.0, RTD_SS_VCGCSSS_OO ,RTD_SS_SSTH_Sli)) );
			half RTD_SS_SSABLSS_OO = lerp( RTD_SS_SST, lerp(RTD_SS_SST,1.0, (1.0 - dim_val)  ), _SelfShadowAffectedByLightShadowStrength );
			half RTD_SS_ON = lerp(1.0,(RTD_SS_SSABLSS_OO*attenuation),_SelfShadowRealtimeShadowIntensity);

			half RTD_SS = RTD_SS_ON;

			return RTD_SS;
            
		#else
            
			half RTD_SS_OFF = lerp(1.0,attenuation,_SelfShadowRealtimeShadowIntensity);

			half RTD_SS = RTD_SS_OFF;

			return RTD_SS;
            
		#endif

	#else

		half RTD_SS_OFF = lerp(1.0, attenuation, _SelfShadowRealtimeShadowIntensity);

		half RTD_SS = RTD_SS_OFF;

		return RTD_SS;

	#endif

}
//

//RT_RELGI_SUB1
half3 RT_RELGI_SUB1(float2 uv, half3 RTD_GI_FS_OO , half3 RTD_SHAT_COL , half3 RTD_MCIALO , half RTD_STIAL, Light mainLight, float3 normalDirection)
{
	half3 RTD_SL_OFF_OTHERS = float3(1.0, 1.0, 1.0);

	#if N_F_RELGI_ON 

		#if defined(LIGHTMAP_ON)//

			half3 baked_GI = SampleLightmap(uv, lerp(float3(0.0, 0.0, 0.0), float3(1.0, 1.0, 1.0), RTD_GI_FS_OO));
			MixRealtimeAndBakedGI(mainLight, normalDirection, baked_GI, half4(0, 0, 0, 0));
			half3 RTD_B_GI_AND_AL_GI = baked_GI * 2; //

			MixRealtimeAndBakedGI(mainLight, normalDirection, baked_GI , half4(0, 0, 0, 0));

		//#elif ( defined(LIGHTMAP_ON) & defined(_MIXED_LIGHTING_SUBTRACTIVE) ) || (!defined(LIGHTMAP_ON))

			//half3 RTD_B_GI_AND_AL_GI = float3(0.0, 0.0, 0.0);

		#else

			half3 RTD_B_GI_AND_AL_GI = (AL_GI(lerp(float3(0.0, 0.0, 0.0), float3(1.0, 1.0, 1.0), RTD_GI_FS_OO)));

		#endif

		RTD_SL_OFF_OTHERS = lerp(RTD_SHAT_COL, RTD_MCIALO, RTD_STIAL) * (RTD_B_GI_AND_AL_GI * ((_EnvironmentalLightingIntensity)));
		

		return RTD_SL_OFF_OTHERS;

	#else

		RTD_SL_OFF_OTHERS = half3(0.0,0.0,0.0);

		return RTD_SL_OFF_OTHERS;

	#endif

}
//

half3 RT_R( float2 uv , float3 viewReflectDirection , float3 viewDirection , float3 normalDirection , half3 RTD_TEX_COL , half3 RTD_R_OFF_OTHERS, float3 positionWS)
{

	#if N_F_R_ON

	half3 RTD_FR_OFF_OTHERS = GlossyEnvironmentReflection(viewReflectDirection, positionWS, _ReflectionRoughtness, 1.0);

		#if N_F_FR_ON
            
			half2 ref_cal = reflect(viewDirection,normalDirection).rg;
			half2 ref_cal_out = (float2(ref_cal.r,(-1.0*ref_cal.g))*0.5+0.5);
			half4 _FReflection_var = SAMPLE_TEXTURE2D_LOD(_FReflection, sampler_FReflection, TRANSFORM_TEX(ref_cal_out, _FReflection), _ReflectionRoughtness);
			half3 RTD_FR_ON = _FReflection_var.rgb;

			half3 RTD_FR = RTD_FR_ON;
            
		#else
            
			half3 RTD_FR = RTD_FR_OFF_OTHERS;

		#endif

		half4 _MaskReflection_var = SAMPLE_TEXTURE2D(_MaskReflection, sampler_MaskReflection , TRANSFORM_TEX(uv, _MaskReflection));
		half3 RTD_R_MET_Sli = lerp((half3)1.0,(9.0 * (RTD_TEX_COL - (9.0 * 0.005) ) ) , _RefMetallic);
		half3 RTD_R_MAS = lerp(RTD_R_OFF_OTHERS, (RTD_FR * RTD_R_MET_Sli) ,_MaskReflection_var.r);
		half3 RTD_R_ON = lerp(RTD_R_OFF_OTHERS, RTD_R_MAS ,_ReflectionIntensity);

		half3 RTD_R = RTD_R_ON;

		return RTD_R;
            
	#else
            
		half3 RTD_R = RTD_R_OFF_OTHERS;
        
		return RTD_R;

	#endif

}
//

//RT_SL
half3 RT_SL( float2 uv , half3 RTD_SL_OFF_OTHERS , half3 RTD_TEX_COL , half3 RTD_R , out half3 RTD_SL_CHE_1)
{

	#if N_F_SL_ON

		half3 RTD_SL_HC_OO;
		if (!_SelfLitHighContrast)
		{
			RTD_SL_HC_OO = (half3)1.0;
		}
		else
		{
			RTD_SL_HC_OO = RTD_TEX_COL;
		}

		float4 _MaskSelfLit_var = SAMPLE_TEXTURE2D(_MaskSelfLit, sampler_MaskSelfLit ,TRANSFORM_TEX(uv, _MaskSelfLit)); 


		//
		#ifdef UNITY_COLORSPACE_GAMMA
			_SelfLitColor = float4(LinearToGamma22(_SelfLitColor.rgb), _SelfLitColor.a);
		#endif

		half3 RTD_SL_MAS = lerp(RTD_SL_OFF_OTHERS,((_SelfLitColor.rgb * RTD_TEX_COL * RTD_SL_HC_OO)*_SelfLitPower),_MaskSelfLit_var.r);
		//
					
					
		half3 RTD_SL_ON = lerp(RTD_SL_OFF_OTHERS,RTD_SL_MAS,_SelfLitIntensity);

		half3 RTD_SL = RTD_SL_ON;

		half3 RTD_R_SEL = lerp(RTD_R,lerp(RTD_R,RTD_TEX_COL*_TEXMCOLINT,_MaskSelfLit_var.r),_SelfLitIntensity);
		RTD_SL_CHE_1 = RTD_R_SEL;

		return RTD_SL;
            
	#else
            
		half3 RTD_SL = RTD_SL_OFF_OTHERS;
		RTD_SL_CHE_1 = RTD_R;

		return RTD_SL;
     
	#endif

}
//

//RT_RL_SUB1
half3 RT_RL_SUB1(half3 RTD_SL_CHE_1 , half3 RTD_RL_LARL_OO , half3 RTD_RL_MAIN)
{

	#if N_F_RL_ON

		#ifndef N_F_OFLMB_ON
			half3 RT_RLIL_OO;
			if (!_RimLightInLight)
			{
				RT_RLIL_OO = lerp(RTD_SL_CHE_1, RTD_RL_LARL_OO, RTD_RL_MAIN);
			}
			else
			{
				RT_RLIL_OO = RTD_SL_CHE_1;
			}

			half3 RTD_RL_ON = lerp(RTD_SL_CHE_1, RT_RLIL_OO, _RimLigInt);

		#else
			half3 RTD_RL_ON = lerp(RTD_SL_CHE_1, lerp(RTD_SL_CHE_1, RTD_RL_LARL_OO, RTD_RL_MAIN), _RimLigInt);
		#endif
		
		half3 RTD_RL = RTD_RL_ON;

		return RTD_RL;
            
	#else
            
	half3 RTD_RL = RTD_SL_CHE_1;

		return RTD_RL;
            
	#endif

}
//

//RT_CA
half3 RT_CA( half3 color )
{

	#if N_F_CA_ON
            
		half3 RTD_CA_ON = lerp(color,dot(color,half3(0.3,0.59,0.11)),(1.0 - _Saturation));
		half3 RTD_CA = RTD_CA_ON;

		return RTD_CA;
            
	#else

		half3 RTD_CA = color;

		return RTD_CA;
            
	#endif

}
//

//RT_DC
void RT_DC(float4 positionCS, inout half4 _MainTex_var, inout half3 normalWS)
{
	half3 specular = 0;
	half metallic = 0;
	half occlusion = 0;
	half smoothness = 0;
	ApplyDecal(positionCS, _MainTex_var.rgb, specular, normalWS, metallic, occlusion, smoothness);
}
//

//RT_SSAO
half3 RT_SSAO(float4 positionCS)
{
	float3 RT_SSAmOc = 1.0;

	#if defined(_SCREEN_SPACE_OCCLUSION) && !defined(N_F_TRANS_ON)
		float2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);
		AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUV);
		RT_SSAmOc = lerp(_SSAOColor.rgb, 1.0, aoFactor.directAmbientOcclusion);
	#endif

	#ifdef N_F_ESSAO_ON
		return RT_SSAmOc;
	#else
		return 1.0;
	#endif

}
//

//RT_NFD
void RT_NFD(float4 positionCS)
{
#if UNITY_UV_STARTS_AT_TOP
	float2 PixelPositions = float2(positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - positionCS.y) : positionCS.y);
#else
	float2 PixelPositions = float2(positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - positionCS.y) : positionCS.y);
#endif

	float2 NDCPositions;
	NDCPositions = PixelPositions.xy / _ScaledScreenParams.xy;
	NDCPositions.y = 1.0f - NDCPositions.y;

	float dither_out;
	Dither_Float(1.3, float4(NDCPositions.xy, 0, 0), dither_out);
	float dit;
	Remap_Float(distance(_WorldSpaceCameraPos, UNITY_MATRIX_M._m03_m13_m23), float2(_MinFadDistance, _MaxFadDistance), float2 (0, 1), dit);
	clip(-(dither_out - dit));
}
//
