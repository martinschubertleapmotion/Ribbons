Shader "Hidden/FluidThicknessShader"
{

	SubShader { 

		Blend One One  

		ZWrite Off ZTest Always

		Pass { 
			Name "Particle"
			Tags {"Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Transparent"}
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "ObiParticles.cginc"

			struct vin{
				float4 pos   : POSITION;
				float3 corner   : NORMAL;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float2 texcoord2  : TEXCOORD1;
			};

			struct v2f
			{
				float4 pos   : POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float2 data  : TEXCOORD1;
			};

			v2f vert(vin v)
			{ 
				v2f o;
				float4 worldpos = mul(UNITY_MATRIX_MV, v.pos) + float4(v.corner.x, v.corner.y, 0, 0);
				o.pos = mul(UNITY_MATRIX_P, worldpos);
				o.texcoord = v.texcoord;
				o.color = v.color;
				o.data = v.texcoord2;

				return o;
			} 

			fixed4 frag(v2f i) : SV_Target
			{
				return BillboardSphereThickness(i.texcoord) * i.data.y;
			}
			 
			ENDCG

		}

	} 
FallBack "Diffuse"
}
