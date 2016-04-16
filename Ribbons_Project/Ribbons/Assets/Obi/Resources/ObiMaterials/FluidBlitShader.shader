Shader "Hidden/FluidBlitShader" {
	Properties { _MainTex ("Texture", any) = "" {} }
	SubShader { 
		Pass {
 			ZTest Always Cull Off ZWrite Off
			
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			
			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord.xy;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.texcoord);
				//float depth = UNITY_SAMPLE_DEPTH(tex2D(_MainTex, i.texcoord));
				//return fixed4(depth,depth,depth,1);
			}
			ENDCG 

		}
	}
	Fallback Off 
}
