Shader "Hidden/DepthBlurShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			float2 MeanCurvature(float2 pos)
			{
				// size of one pixel
				float2 px = float2(_MainTex_TexelSize.x, 0.0f);
				float2 py = float2(0.0f, _MainTex_TexelSize.y);
			
				// depth samples:
				float zc = tex2D(_MainTex, pos).x;
				float zr = tex2D(_MainTex, pos + px).x;
				float zl = tex2D(_MainTex, pos - px).x;
				float zu = tex2D(_MainTex, pos + py).x;
				float zd = tex2D(_MainTex, pos - py).x;

				//derivatives:
				float dx = (zr - zl)*0.5;
				float dy = (zu - zd)*0.5;

				// boundary conditions:
				if (abs(dx) > 0.001 || abs(dy) > 0.001)
					return float2(zc,0);

				//second derivatives:
				float dxx = zl - 2*zc + zr;
				float dyy = zd - 2*zc + zu;

				float Cx = 2/_ScreenParams.x * UNITY_MATRIX_P[0][0]; // 1/tan(fov/2)
				float Cy = 2/_ScreenParams.y * UNITY_MATRIX_P[1][1];

				// constants:	
				const float dx2 = dx * dx; const float dy2 = dy * dy;
				const float Cx2 = Cx * Cx; const float Cy2 = Cy * Cy;
			
				// calculate curvature:
				float D = Cy2*dx2 + Cx2*dy2 + Cx2*Cy2*zc*zc;
				float H = Cy*dxx*D - Cy*dx*(Cy2*dx*dxx + Cx2*Cy2*zc*dx)
						+  Cx*dyy*D - Cx*dy*(Cx2*dy*dyy + Cx2*Cy2*zc*dy);
				H /= pow(D,3.0/2.0);
			
				return float2(zc,H);
			}	

			float4 frag (v2f i) : SV_Target
			{
				float4 o = float4(0,0,0,1);

				float2 depthCurvature = MeanCurvature(i.uv);

				return depthCurvature.x + 0.0008 * depthCurvature.y;
			}
			ENDCG
		}
	}
}
