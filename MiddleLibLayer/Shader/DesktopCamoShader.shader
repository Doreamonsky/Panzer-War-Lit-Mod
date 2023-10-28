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
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
