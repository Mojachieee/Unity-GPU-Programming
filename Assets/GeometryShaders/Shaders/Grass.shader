Shader "Geometry/Grass"
{
	Properties
	{
		_GroundColor("Ground Color", Color) = (0.2, 0.8, 0.2, 1.0)
		_GrassColor("Grass Color", Color) = (0.5, 0.8, 0.5, 1.0)
		_GrassWidth("Grass Width", float) = 0.5
		_GrassMinHeight("Grass Min Height", float) = 1.5
		_GrassMaxHeight("Grass Max Height", float) = 2.5
		_GrassTiltAmount("Grass Tilt Amount", float) = 0.5
		_GrassWindDirection("Grass Wind Direction", vector) = (1.0, 0, 0, 1)
		_GrassWindPulseSpeed("Grass Wind Pulse Speed", float) = 1
		
		
		
		
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : TEXCOORD0;
			};

			appdata vert (appdata v)
			{
				return v;
			}

			float4 _GroundColor;
			float4 _GrassColor;
			float _GrassWidth;
			float _GrassMaxHeight;
			float _GrassMinHeight;
			float _GrassTiltAmount;
			float _GrassWindPulseSpeed;
			vector _GrassWindDirection;
			
			
			float random(float2 seed) {
				return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
			}

			v2f geomVert(float3 position, float4 color) {
				v2f o;
				o.vertex = UnityObjectToClipPos(position);
				o.color = color;
				return o;
			}

			[maxvertexcount(12)]
			void geom(triangle appdata input[3], inout TriangleStream<v2f> stream) {
				
				stream.Append(geomVert(input[0].vertex, _GroundColor));
				stream.Append(geomVert(input[1].vertex, _GroundColor));
				stream.Append(geomVert(input[2].vertex, _GroundColor));
				stream.RestartStrip();

				float3 centre = input[0].vertex + input[1].vertex + input[2].vertex;
				centre /= 3;

				float3 up = cross(input[0].vertex - input[1].vertex, input[0].vertex - input[2].vertex);
				up = normalize(up);

				float randomRotation = random(input[1].vertex.xy * 123.321) * 6.28;
				float tiltDirection = float3(cos(randomRotation), 0.0, sin(randomRotation));
				tiltDirection = normalize(tiltDirection) * _GrassTiltAmount;
				up += tiltDirection;
				up = normalize(up);

				float3 windDirection = UnityWorldToObjectDir(_GrassWindDirection.xyz);
				windDirection = normalize(windDirection) * _GrassWindDirection.w;
				float windPulse = sin(_Time.y * _GrassWindPulseSpeed) * 0.5 + 0.5;

				up += windDirection * windPulse;

				float randomHeightFactor = random(input[0].vertex.xy * 321.2);
				up *= lerp(_GrassMinHeight, _GrassMaxHeight, randomHeightFactor);

				float3 tangent = input[0].vertex - centre;
				tangent = normalize(tangent) * _GrassWidth;

				float3 otherTangent = input[1].vertex - centre;
				otherTangent = normalize(otherTangent) * _GrassWidth;

				float3 p0, p1, p2, p3;
				p0 = centre - tangent;
				p1 = centre + tangent;
				p2 = centre + up;
				p3 = centre + otherTangent;

				stream.Append(geomVert(p0, _GroundColor));
				stream.Append(geomVert(p1, _GroundColor));
				stream.Append(geomVert(p2, _GrassColor));
				stream.RestartStrip();
				
				stream.Append(geomVert(p1, _GroundColor));
				stream.Append(geomVert(p3, _GroundColor));
				stream.Append(geomVert(p2, _GrassColor));
				stream.RestartStrip();
				
				stream.Append(geomVert(p3, _GroundColor));
				stream.Append(geomVert(p0, _GroundColor));
				stream.Append(geomVert(p2, _GrassColor));
				stream.RestartStrip();

			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}
