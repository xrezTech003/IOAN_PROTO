// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/VertexColor3" {
	Properties {
		_sSlider("sSlider",float) = 0
		_pPositionX("pPositionX",float) = 0
		_pPositionY("pPositionY",float) = 0
		_cPositionX("cPositionX",float) = 0
		_cPositionY("cPositionY",float) = 0
	}
    SubShader {
    Tags{"Queue"="Transparent" "RenderType"="Opaque"}
    Pass {
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
         
        CGPROGRAM
        #pragma vertex vert alpha
        #pragma fragment frag
 
        struct VertexInput {
            float4 v : POSITION;
            float4 color: COLOR;
            float4 uv : TEXCOORD0;
        };
         
        struct VertexOutput {
            float4 pos : SV_POSITION;
            float4 col : COLOR;
        };

        float _sSlider;
        float _pPositionX;
        float _pPositionY;
        float newPosX;
        float newPosY;
        float posDist;
        float _cPositionX;
        float _cPositionY;
        float cnewPosX;
        float cnewPosY;
        float cposDist;
         
        VertexOutput vert(VertexInput v) {
         
            VertexOutput o;
            newPosX = _pPositionX + 3.0;
            newPosY = _pPositionY + 18.0;
            posDist = sqrt(pow(abs(v.v.x - newPosX),2) + pow(abs(v.v.y - newPosY),2));
            cnewPosX = _cPositionX + 3.0;
            cnewPosY = _cPositionY + 18.0;
            cposDist = sqrt(pow(abs(v.v.x - cnewPosX),2) + pow(abs(v.v.y - cnewPosY),2));
            if(posDist < 3.0) {
            	o.pos = UnityObjectToClipPos(float4(v.v.x,v.v.y,v.v.z - min(max((v.uv.x-1.0+1.1*_sSlider),0.0)*20,1.0) * 0.5,v.v.w).xyzw);
            	o.col.rgb = float3(0.75+((v.uv.x-0.75)),0.75-((v.uv.x-0.25)),0.75+((v.uv.x-0.75))).rgb;
            }
            //else if (cposDist < 3.0) {
            //	o.pos = mul(UNITY_MATRIX_MVP, float4(v.v.x,v.v.y,v.v.z - min(max((v.uv.x-1.0+1.1*_sSlider),0.0)*20,1.0) * 0.5,v.v.w).xyzw);
            //	o.col.rgb = float3(0.75+((v.uv.x-0.75)),0.75-((v.uv.x-0.25)),0.75+((v.uv.x-0.75))).rgb;
            //}
            else {
            	o.pos = UnityObjectToClipPos(v.v);
            	o.col = v.color;
            }
            o.col.a = v.color.a;
             
            return o;
        }
         
        float4 frag(VertexOutput o) : COLOR {
            return o.col;
        }
 
        ENDCG
        } 
    }
 
}