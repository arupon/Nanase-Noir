Shader "wataameya/shadowdepth_body"
{
	Properties
	{
		_color ("色", Color) = (0,0,0,0)
		_strength("影の強さ", range(0.0,0.99))=0.5
		_area("にじみ範囲", range(0.0,1.0))=0.5
		_back("裏面の暗さ", range(0.0,1.0))=0
		[SToggle]
		_func("深度関数変更(Off:１次関数 On:2次関数)", int) = 0
        [Toggle(_CANCEL)]
        _cancel("距離クリッピングキャンセラー(lilToonなど)", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue"="Overlay+2000"
			"VRCFallback"="Hidden"
		}
		
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Zwrite off
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _CANCEL _
			#include "UnityCG.cginc"
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 vert : TEXCOORD0;
			};
			
			fixed4 _color;
			float depth;
			float _area;
			float _strength;
			int _func;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(vertex);
				o.vert = mul(UNITY_MATRIX_M, vertex);
				#ifdef _CANCEL
				if(0 < o.vertex.w && o.vertex.w < _ProjectionParams.y * 1.01)  o.vertex.z = o.vertex.z * 0.0001 + o.vertex.w * 0.999;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				depth = saturate(length(i.vert-_WorldSpaceCameraPos)*4/_area);
				UNITY_BRANCH
				if(_func == 1) depth *= depth*2;
				_color.a = saturate(lerp(1,0,depth+(1-_strength)/2));
				return _color;
			}
			ENDCG
		}
		
        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			Zwrite off
			cull front
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile _CANCEL _
            #include "UnityCG.cginc"
			
			float _back;
			
            float4 vert (float4 vertex : POSITION) : SV_POSITION
            {
				float4 v = UnityObjectToClipPos(vertex);
				#ifdef _CANCEL
				if(0 < v.w && v.w < _ProjectionParams.y * 1.01)  v.z = v.z * 0.0001 + v.w * 0.999;
				#endif
                return v;
            }
            fixed4 frag () : SV_Target
            {
				return fixed4(0,0,0,_back*0.96);
            }
            ENDCG
        }
	}
}