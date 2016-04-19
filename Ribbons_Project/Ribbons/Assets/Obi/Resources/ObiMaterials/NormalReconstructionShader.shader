// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'

Shader "Hidden/NormalReconstructionShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{

		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		// should read from depth buffer, but not write to it.

		Pass
		{

			//Name "Final"
			Tags {"LightMode" = "ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_fwdbase
			
			#include "ObiParticles.cginc"
			#include "UnityStandardBRDF.cginc"

			struct vin
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				LIGHTING_COORDS(6,7)
			};

			v2f vert (vin v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.pos);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;	
			sampler2D _Refraction;
			sampler2D _Thickness;
			sampler2D _CameraDepthTexture;
			float4 _MainTex_TexelSize;
			//fixed4 _LightColor0;

			float3 NormalFromEyePos(float2 uv, float3 eyePos)
			{
				// get sample coordinates:
				float2 sx = uv + float2(_MainTex_TexelSize.x,0);
				float2 sy = uv + float2(0,_MainTex_TexelSize.y);

				float2 sx2 = uv - float2(_MainTex_TexelSize.x,0);
				float2 sy2 = uv - float2(0,_MainTex_TexelSize.y);

				// get eye space from depth at these coords, and compute derivatives:
				float3 dx = EyePosFromDepth(sx,tex2D(_MainTex, sx).x) - eyePos;
				float3 dy = EyePosFromDepth(sy,tex2D(_MainTex, sy).x) - eyePos;

				float3 dx2 = eyePos - EyePosFromDepth(sx2,tex2D(_MainTex, sx2).x);
				float3 dy2 = eyePos - EyePosFromDepth(sy2,tex2D(_MainTex, sy2).x);

				if (abs(dx.z) > abs(dx2.z))
					dx = dx2;

				if (abs(dy2.z) < abs(dy.z))
					dy = dy2;

				return normalize(cross(dx,dy));
			}	

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = fixed4(0,0,0,1);
			
				float depth = tex2D(_MainTex, i.uv).r;
				float sceneDepth = tex2D(_CameraDepthTexture,i.uv).r;
				float thickness = tex2D(_Thickness,i.uv).r;

				// get eye space position from depth
				float3 eyePos = EyePosFromDepth(i.uv,depth);

				// reconstruct normal from eye space position:
				float3 n = NormalFromEyePos(i.uv,eyePos);

				// get eye space light direction
				float3 lightDir = normalize(EyeSpaceLightDir(eyePos));
				lightDir.z *= -1; //hack

				//ambient lighting:
				half3 amb = SampleSphereAmbientWorld(mul((float3x3)unity_CameraToWorld,n*half3(1,1,-1))); //hack

				// basic diffuse lighting:
				float ndotl = saturate( dot( n,lightDir ) );	

				UNITY_LIGHT_ATTENUATION(atten,i,mul((float3x3)unity_CameraToWorld,eyePos));

				// Reflection and refraction:
				float3 Reflection = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, mul((float3x3)unity_CameraToWorld,n) , 0);
				fixed4 Refraction = tex2D(_Refraction, i.uv + n.xy * thickness*0.005);

				// absorptance, transmittance and reflectance.
				half3 Absorbance = half3(.9,.5,.1) * 2 * -thickness;
				half3 Transmittance = Refraction * exp(Absorbance);

				// get R0 (reflectance at normal)
				float AirIOR = 1.0;
				float WaterIOR = 1.33;
				float R_0 = (AirIOR - WaterIOR) / (AirIOR + WaterIOR);
				R_0 *= R_0;

				float Reflectance = FresnelTerm(R_0,DotClamped(n,-normalize(eyePos)));

				// basic specular lightning:
				half3 h = normalize( lightDir - normalize(eyePos) );
				float nh = DotClamped( n, h );
    			float spec = pow( nh, 64 ) * col.a; //not entirely sure its right.

				fixed3 diffuse = fixed3(0.4,0.7,1) * (_LightColor0 * ndotl * atten + amb);
				fixed3 dielectric = lerp(Transmittance,Reflection,Reflectance);

				col.rgb = lerp(dielectric,diffuse,0.1) + spec*atten;

				// alpha testing:
				if (depth == 1 || thickness < 0.001)
					col.a = 0;

				// depth test:
				if (sceneDepth < depth)
					col.a = 0;

				return col;
			}
			ENDCG
		}

	}
}
