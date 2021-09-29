// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "Unlit/TextSphereNewShader" {
 Properties {
     _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
     _AlphaMod("Alpha Mod", float) = 1
     _Flip("Flip", float) = 0
     _FlipY("FlipY", float) = 0
 }
 
 SubShader {
     Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
     LOD 100
     
     ZWrite Off
     Blend SrcAlpha OneMinusSrcAlpha 
     
     Pass {  
         CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             //#pragma multi_compile_fog
             
             #include "UnityCG.cginc"
 
             struct appdata_t {
                 float4 vertex : POSITION;
                 float2 texcoord : TEXCOORD0;
             };
 
             struct v2f {
                 float4 vertex : SV_POSITION;
                 half2 texcoord : TEXCOORD0;
                 //UNITY_FOG_COORDS(1)
             };
 
             sampler2D _MainTex;
             float4 _MainTex_ST;
             float _AlphaMod;
             float _Flip;
             float _FlipY;
             
             v2f vert (appdata_t v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                 //UNITY_TRANSFER_FOG(o,o.vertex);
                 return o;
             }
             
             fixed4 frag (v2f i) : SV_Target
             {
                 //fixed4 col = float4(tex2D(_MainTex, i.texcoord).rgb,_AlphaMod);
                 float aRange = 0.05;
                 float myAMod = 0.01;

                 //myAMod += pow(sin((_Time[1] * 0.85) + i.texcoord.x * 3.14159 * 0.5 * 1.0 + 3.14159 * 0.5 * (1.0 * _Flip)) * 0.5 + 0.5,21.0) * aRange * _AlphaMod * (sin(_Time[1] * 0.33) *0.5 + 0.5) * 0.1;
                 //myAMod += pow(sin((_Time[1] * 0.69) - (i.texcoord.y - i.texcoord.x) * 3.14159 * 0.5 * 1.0 - 3.14159 * 0.5 * (1.0 * _FlipY) + 3.14159 * 0.5 * (1.0 * _Flip)) * 0.5 + 0.5,
                 //	101.0 + 71.0 * (sin(_Time[1] * 0.47) *0.5 + 0.5)) * aRange * _AlphaMod * (sin(_Time[1] * 0.22) *0.5 + 0.5) * 0.1;
				 //myAMod += pow(sin((_Time[1] * 0.32) + (1.5 * i.texcoord.y - 0.5 * i.texcoord.x) * 3.14159 *0.5 * 2.0 + 3.14159 *0.5 * (1.5) * (2.0 * _FlipY) - 3.14159 *0.5 * (0.5) * (2.0 * _Flip)) * 0.5 + 0.5,
                 //	81.0 + 91.0 * (sin(_Time[1] * 0.66) *0.5 + 0.5)) * aRange * _AlphaMod * (sin(_Time[1] * 0.36) *0.5 + 0.5) * 0.1;
                 //myAMod += pow(sin((_Time[1] * 0.54) - (-0.7 * i.texcoord.y + 1.3 * i.texcoord.x) * 3.14159 *0.5 * 0.77 + 3.14159 *0.5 * 0.7 * (0.77 * _FlipY) - 3.14159 *0.5 * 1.3 * (0.77 * _Flip)) * 0.5 + 0.5,
                 //	93.0 + 112.0 * (sin(_Time[1] * 0.39) *0.5 + 0.5)) * aRange * _AlphaMod * (sin(_Time[1] * 0.44) *0.5 + 0.5) * 0.1;
                 //myAMod += pow(sin((_Time[1] * 0.4) - i.texcoord.y * 3.14159 *0.5 * 0.375 - 3.14159 *0.5 * (0.375 * _FlipY)) * 0.5 + 0.5,101.0) * aRange * _AlphaMod * 1.0;

                 myAMod += pow(sin((_Time[1] * 0.85 * 0.5) + i.texcoord.x * 3.14159 * 0.5 * 1.0 + 3.14159 * 0.5 * (1.0 * _Flip)) * 0.5 + 0.5,21.0) * aRange * _AlphaMod * (sin(_Time[1] * 0.33) *0.5 + 0.5) * 0.1  * 20.0;
                 myAMod += pow(sin((_Time[1] * 0.78 * 0.5) + (i.texcoord.y + i.texcoord.x) * 3.14159 * 0.5 * 1.0 + 3.14159 * 0.5 * (1.0 * _FlipY) + 3.14159 * 0.5 * (1.0 * _Flip)) * 0.5 + 0.5,
                 	101.0 + 71.0 * (sin(_Time[1] * 0.47) *0.5 + 0.5)) * aRange * _AlphaMod * (sin(_Time[1] * 0.22) *0.5 + 0.5) * 0.1  * 20.0 * 0.5;
				 myAMod += pow(sin((_Time[1] * 0.32 * 0.5) - (1.5 * i.texcoord.y - 0.5 * i.texcoord.x) * 3.14159 *0.5 * 2.0 - 3.14159 *0.5 * (1.5) * (2.0 * _FlipY) + 3.14159 *0.5 * (0.5) * (2.0 * _Flip)) * 0.5 + 0.5,
                 	81.0 + 91.0 * (sin(_Time[1] * 0.66) *0.5 + 0.5)) * aRange * _AlphaMod * (sin(_Time[1] * 0.36) *0.5 + 0.5) * 0.1  * 20.0 * 0.5;
                 myAMod += pow(sin((_Time[1] * 0.48 * 0.5) - (-0.7 * i.texcoord.y + 1.3 * i.texcoord.x) * 3.14159 *0.5 * 0.77 + 3.14159 *0.5 * 0.7 * (0.77 * _FlipY) - 3.14159 *0.5 * 1.3 * (0.77 * _Flip)) * 0.5 + 0.5,
                 	93.0 + 112.0 * (sin(_Time[1] * 0.39 * 0.5) *0.5 + 0.5)) * aRange * _AlphaMod * (sin(_Time[1] * 0.44) *0.5 + 0.5) * 0.1 * 20.0 * 0.5;
                 myAMod += pow(sin((_Time[1] * 0.65 * 0.5) - (i.texcoord.y - i.texcoord.x) * 3.14159 * 0.5 * 1.0 - 3.14159 * 0.5 * (1.0 * _FlipY) + 3.14159 * 0.5 * (1.0 * _Flip)) * 0.5 + 0.5,
                 	111.0 + 71.0 * (sin(_Time[1] * 0.36) *0.5 + 0.5)) * aRange * _AlphaMod * (sin(_Time[1] * 0.30) *0.5 + 0.5) * 0.1 * 20.0 * 0.5;
				 myAMod += pow(sin((_Time[1] * 0.52 * 0.5) + (1.5 * i.texcoord.y + 0.5 * i.texcoord.x) * 3.14159 *0.5 * 2.0 + 3.14159 *0.5 * (1.5) * (2.0 * _FlipY) + 3.14159 *0.5 * (0.5) * (2.0 * _Flip)) * 0.5 + 0.5,
                 	71.0 + 151.0 * (sin(_Time[1] * 0.59) *0.5 + 0.5)) * aRange * _AlphaMod * (sin(_Time[1] * 0.43) *0.5 + 0.5) * 0.1 * 20.0 * 0.5;
                 myAMod += pow(sin((_Time[1] * 0.4 * 0.5 + 0.38625) - i.texcoord.y * 3.14159 *0.5 * 0.375 - 3.14159 *0.5 * (0.375 * _FlipY)) * 0.5 + 0.5,141.0) * aRange * _AlphaMod * 0.1 * 20.0;
                 myAMod += pow(sin((_Time[1] * 0.125) - i.texcoord.y * 3.14159 *0.5 * 0.375 - 3.14159 *0.5 * (0.375 * _FlipY)) * 0.5 + 0.5,91.0) * aRange * _AlphaMod * 1.0 * 20.0;

                 //myAMod = 1.0;
                 //myAMod = 0.0;

                 //myAMod += pow(sin((_Time[1] * 0.79 * 1.) + (i.texcoord.y + i.texcoord.x) * 3.14159 * 0.5 * 1.0 + 3.14159 * 0.5 * (1.0 * _FlipY) + 3.14159 * 0.5 * (1.0 * _Flip)) * 0.5 + 0.5,
                 //	101.0 + 71.0 * (sin(_Time[1] * 0.47) *0.5 + 0.5)) * aRange * _AlphaMod * (sin(_Time[1] * 0.22) *0.5 + 0.5) * 0.1  * 100.0;
                 //myAMod += pow(sin((_Time[1] * 0.48 * 1.) - (-0.7 * i.texcoord.y + 1.3 * i.texcoord.x) * 3.14159 *0.5 * 0.77 + 3.14159 *0.5 * 0.7 * (0.77 * _FlipY) - 3.14159 *0.5 * 1.3 * (0.77 * _Flip)) * 0.5 + 0.5,
                 //	93.0 + 112.0 * (sin(_Time[1] * 0.39 * 0.5) *0.5 + 0.5)) * aRange * _AlphaMod * (sin(_Time[1] * 0.44) *0.5 + 0.5) * 0.1 * 100.0;
                 //myAMod += pow(sin((_Time[1] * 0.32 * 1.) - (1.5 * i.texcoord.y - 0.5 * i.texcoord.x) * 3.14159 *0.5 * 2.0 - 3.14159 *0.5 * (1.5) * (2.0 * _FlipY) + 3.14159 *0.5 * (0.5) * (2.0 * _Flip)) * 0.5 + 0.5,
                 //	81.0 + 91.0 * (sin(_Time[1] * 0.66) *0.5 + 0.5)) * aRange * _AlphaMod * (sin(_Time[1] * 0.36) *0.5 + 0.5) * 0.1  * 100.0;
                 
                 fixed4 col = tex2D(_MainTex, i.texcoord);
                 col.a *= myAMod;
                 //UNITY_APPLY_FOG(i.fogCoord, col);
                 return col;
             }
         ENDCG
     }
 }
 
 }