// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "Unlit/TextSphereNewShader1" {
 Properties {
     _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
     _AlphaMod("Alpha Mod", float) = 1
     _Flip("Flip", float) = 1
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
                 float myAMod = 0.0;
                 if(_Flip==1){
                 	myAMod += pow(sin(_Time[1] + i.texcoord.x * 3.14159 * 0.25),11.0) * aRange * _AlphaMod;
                 } else {
                 	myAMod += pow(sin(_Time[1] + i.texcoord.x * 3.14159 * 0.25 + 3.14159 * 0.25),11.0) * aRange * _AlphaMod;
                 }
                 //myAMod += pow(sin((_Time[1] * 1.47) + i.texcoord.y * 3.14159 * 2.0),7.0) * aRange * _AlphaMod;
                 fixed4 col = tex2D(_MainTex, i.texcoord);
                 col.a *= myAMod;
                 //UNITY_APPLY_FOG(i.fogCoord, col);
                 return col;
             }
         ENDCG
     }
 }
 
 }