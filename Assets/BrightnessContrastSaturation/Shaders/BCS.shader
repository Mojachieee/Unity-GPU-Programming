Shader "PostProcess/BCS"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		// _Brightness ("Brightness", Range(-1, 1)) = 0.0
		// _Contrast ("Contrast", Range(0, 2)) = 1
		// _Saturation ("Saturation", Range (0, 2)) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _Brightness;
			float _Contrast;
			float _Saturation;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

#ifndef UNITY_COLORSPACE_GAMMA
				col.rgb = LinearToGammaSpace(col.rgb);
#endif
				// Brightness
				col.rgb = max(float3(0,0,0), col.rgb + _Brightness);

				// Contrast
				col.rgb = (col.rgb - 0.5) * _Contrast + 0.5;
				

				// Saturation
				float grayScale = dot(col.rgb, float3(0.22, 0.707, 0.071));
				col.rgb = lerp(grayScale, col.rgb, _Saturation);

#ifndef UNITY_COLORSPACE_GAMMA
				col.rgb = GammaToLinearSpace(col.rgb);
#endif

				return col;
			}
			ENDCG
		}
	}
}
