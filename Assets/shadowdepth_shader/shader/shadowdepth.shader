Shader "wataameya/shadowdepth"
{
	Properties
	{
		_color ("色", Color) = (0,0,0,0)
		_strength("影の強さ", range(0.0,0.99))=0.5
		_outline("アウトラインの暗さ", range(0.0,1.0))=0.5
		_area("にじみ範囲", range(0.0,1.0))=0.1
		[Toggle]
		_func("深度関数変更(Off:１次関数 On:2次関数)", int) = 0
		[Toggle]
		_check("有効範囲確認モード(アップロード時はOff)", int) = 1
	}

	SubShader
	{
		Tags
		{
			"Queue"="Overlay+5000"
			"VRCFallback"="Hidden"
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull front
			ZTest always
			ZWrite off
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex : POSITION;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD0;
			};
			
			fixed4 _color;
			float depth;
			float _area;
			float _strength;
			int _func;
			sampler2D _CameraDepthTexture;
			
			v2f vert (appdata v)
			{
				v.vertex.xyz *= 3;
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeGrabScreenPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float sX = 1/sqrt(pow(unity_WorldToObject[0].x, 2) + pow(unity_WorldToObject[0].y, 2) + pow(unity_WorldToObject[0].z, 2));
				float sY = 1/sqrt(pow(unity_WorldToObject[1].x, 2) + pow(unity_WorldToObject[1].y, 2) + pow(unity_WorldToObject[1].z, 2));
				float size = (sX + sY)/2;
				float distance = length(mul(unity_ObjectToWorld,float4(0,0,0,1))-_WorldSpaceCameraPos)+size/2;
				float ds = distance - size;
				float2 grabUV = (i.screenPos.xy / i.screenPos.w);
				depth = (LinearEyeDepth(tex2D(_CameraDepthTexture, grabUV))*(1-_strength)*20).x;
				UNITY_BRANCH
				if(_func == 1) depth *= depth*2;
				UNITY_BRANCH
				if(ds<=_area/3 && depth < 1) _color.a *= 1-lerp(depth,1,saturate(ds/(_area/3)));
				else discard;
				return _color;
			}
			ENDCG
		}
		
		Pass
        {
			Stencil
            {
                Ref 1
				Pass Replace
            }
			Cull front
			Zwrite off
			Colormask 0
        }
		
		Pass
		{
			Stencil
            {
                Ref 0
                Comp Equal
            }
			
			Blend SrcAlpha OneMinusSrcAlpha
			Cull front
			ZTest always
			ZWrite off
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex : POSITION;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD0;
			};
			
			fixed4 _color;
			float depth;
			float _outline;
			sampler2D _CameraDepthTexture;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeGrabScreenPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float sX = 1/sqrt(pow(unity_WorldToObject[0].x, 2) + pow(unity_WorldToObject[0].y, 2) + pow(unity_WorldToObject[0].z, 2));
				float sY = 1/sqrt(pow(unity_WorldToObject[1].x, 2) + pow(unity_WorldToObject[1].y, 2) + pow(unity_WorldToObject[1].z, 2));
				float size = (sX + sY)/2;
				float distance = length(mul(unity_ObjectToWorld,float4(0,0,0,1))-_WorldSpaceCameraPos)+size/2;
				float ds = (distance - size)/(0.1/3);
				UNITY_BRANCH
				if(ds<=1)
				{
					float2 grabUV = (i.screenPos.xy / i.screenPos.w);
					depth = (LinearEyeDepth(tex2D(_CameraDepthTexture, grabUV))).x;
					UNITY_BRANCH
					if(depth >= size) _color.a *= _outline*(saturate(1-ds));
					else discard;
				}
				else discard;
				return _color;
			}
			ENDCG
		}
		
		Pass
        {
			Stencil
            {
                Ref 1
				Comp Equal
				Pass Zero
            }
			Cull front
			Zwrite off
			Colormask 0
        }
		
        Pass
        {
			ZWrite off
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
			
			int _check;
            float4 vert (float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }
            fixed4 frag () : SV_Target
            {
				UNITY_BRANCH
				if(_check == 0) discard;
				return 0;
            }
            ENDCG
        }
	}
}