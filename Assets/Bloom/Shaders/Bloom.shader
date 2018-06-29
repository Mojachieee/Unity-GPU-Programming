Shader "Bloom"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	CGINCLUDE
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
		float4 _MainTex_TexelSize;

		float4 BoxBlur(float2 uv, float kernelSize) {
			float2 offset = _MainTex_TexelSize.xy * kernelSize;
			float4 col = 0;
			col += tex2D(_MainTex, uv + float2(offset.x, offset.y));
			col += tex2D(_MainTex, uv + float2(offset.x, -offset.y));
			col += tex2D(_MainTex, uv + float2(-offset.x, -offset.y));
			col += tex2D(_MainTex, uv + float2(-offset.x, offset.y));

			return col * 0.25;
		}
	ENDCG

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		
		Pass // Prefilter
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float _Threshold;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 col = BoxBlur(i.uv, 1.0);

				float brightness = max(col.r, max(col.g, col.b));
				float factor = max(0, brightness - _Threshold);
				factor /= max(brightness, 0.001);
				return col * factor;
			}
			ENDCG
		}

		Pass	// Downsample
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			

			fixed4 frag (v2f i) : SV_Target
			{
				return BoxBlur(i.uv, 1.0);
			}
			ENDCG
		}

		Pass	// Upsample
		{
			Blend One One
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			

			fixed4 frag (v2f i) : SV_Target
			{
				return BoxBlur(i.uv, 0.5);
			}
			ENDCG
		}

		Pass	// Apply Bloom
		{
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _SourceTex;
			float _Intensity;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 src = tex2D(_SourceTex, i.uv);
				float4 bloom = tex2D(_MainTex, i.uv);

				return lerp(src, src + bloom, _Intensity);
			}
			ENDCG
		}
	}
}
