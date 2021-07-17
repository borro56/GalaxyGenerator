Shader "PCG/Planet"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MainTex2("Texture", 2D) = "white" {}
		_Normals("Normals", 2D) = "white" {}
		_NormalsOffset("Normals Offset", float) = 1
		_Tiling ("Tiling", float) = 1
		_AmbientColor ("Ambient", Color) = (0,0,0,0)
	}

	SubShader
	{
		Tags { "Queue" = "Transparent"}

		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "Includes/PlanetUtils.cginc"
			#include "Includes/PlanetNoises.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

			struct PS_INPUT
			{
				float4 position : SV_POSITION;
				float3 fieldPos : TEXCOORD2;
			};

			StructuredBuffer<FieldVertex> vertexes;
			sampler2D _MainTex;
			sampler2D _MainTex2;
			sampler2D _Normals;
			float4 _AmbientColor;
			float3 posOffset;
			float3 lightDir;
			float4 lightCol;
			float3 halfSize;
			float _NormalsOffset;
			float _Tiling;

			//Noise parameters
			float3 areaSize;
			float surfaceFrequency;
			float surfaceHeight;
			float athmosphereHeight;
			float smoothRange;
			float cavesFrequency;
			float3 noiseOffset;

			float getNoise(float3 pos)
			{
				return planetNoise(pos,
					areaSize,
					surfaceFrequency,
					surfaceHeight,
					athmosphereHeight,
					noiseOffset,
					smoothRange,
					cavesFrequency);
			}

			//TODO: Reduce unneeded vertexes draw through Geometry Shader (check performance gain)
			PS_INPUT vert(uint vertex_id : SV_VertexID)
			{
				//Vertex Transformation
				PS_INPUT o = (PS_INPUT)0;
				o.fieldPos = vertexes[vertex_id].position;
				o.position = UnityObjectToClipPos(float4(o.fieldPos + posOffset,1));
				return o;
			}

			//TODO: Parametrize tiling & offset
			float4 frag(PS_INPUT i) : COLOR
			{
				//Calculate Normals
				float cenV = getNoise(i.fieldPos);
				float rigV = getNoise(i.fieldPos + float3(_NormalsOffset, 0, 0));
				float upwV = getNoise(i.fieldPos + float3(0, _NormalsOffset, 0));
				float fwdV = getNoise(i.fieldPos + float3(0, 0, _NormalsOffset));
				float3 normal = normalize(float3(rigV - cenV, upwV - cenV, fwdV - cenV));

				//Texturing
				float3 centerPos = areaSize / 2;
				float3 centerDir = normalize(i.fieldPos - centerPos);
				float texDot = max(0 , dot(normal, -centerDir));
				float3 blending = abs(normal);

				float4 tex1 = tex2D(_MainTex, i.fieldPos.yz * _Tiling) * blending.x +
							  tex2D(_MainTex, i.fieldPos.xz * _Tiling) * blending.y +
							  tex2D(_MainTex, i.fieldPos.xy * _Tiling) * blending.z;
			

				float4 tex2 = tex2D(_MainTex2, i.fieldPos.yz * _Tiling) * blending.x +
							  tex2D(_MainTex2, i.fieldPos.xz * _Tiling) * blending.y +
							  tex2D(_MainTex2, i.fieldPos.xy * _Tiling) * blending.z;

				//Lighting
				float diffuse = max(0, dot(normal, lightDir));
				float lighting = min(1, diffuse);

				//Combining
				return lerp(tex1, tex2, texDot) * lighting * (0.5 + lightCol * 0.5) + _AmbientColor;
			}

			ENDCG
		}
	}
}