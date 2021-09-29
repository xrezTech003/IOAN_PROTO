﻿Shader "Unlit/StarImageShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		_TextureLoaded ("Texture Loaded", int) = 0
		_Alpha ("Alpha", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _TextureLoaded;
			float _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				if (_TextureLoaded == 0) discard;

                fixed4 col = tex2D(_MainTex, i.uv);
				if (col.a != 0.0) col.a = _Alpha;
				if (col.a == 0.0) discard;

                return col;
            }
            ENDCG
        }
    }
}
