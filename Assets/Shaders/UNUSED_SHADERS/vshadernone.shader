// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/vshadernone" {
	Properties {
		_controllerHeldCDTimer("controllerHeldCDTimer",float) = 0
		_lControllerPos("lControllerPos",Vector) = (0,0,0)
		_controllerHeldCDTimerB("controllerHeldCDTimerB",float) = 0
		_lControllerPosB("lControllerPosB",Vector) = (0,0,0)
		_controllerHeldCDTimerC("controllerHeldCDTimerC",float) = 0
		_lControllerPosC("lControllerPosC",Vector) = (0,0,0)
		_controllerHeldCDTimerD("controllerHeldCDTimerD",float) = 0
		_lControllerPosD("lControllerPosD",Vector) = (0,0,0)
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

        float _controllerHeldCDTimer;
        Vector _lControllerPos;
        float _controllerHeldCDTimerB;
        Vector _lControllerPosB;
        float _controllerHeldCDTimerC;
        Vector _lControllerPosC;
        float _controllerHeldCDTimerD;
        Vector _lControllerPosD;

        VertexOutput vert(VertexInput v) {
         
            VertexOutput o;
            float aDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPos.x)),2) + pow(abs(v.v.z - (_lControllerPos.z)),2))),0.5) * 1.0;
            float bDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosB.x)),2) + pow(abs(v.v.z - (_lControllerPosB.z)),2))),0.5) * 1.0;
			float cDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosC.x)),2) + pow(abs(v.v.z - (_lControllerPosC.z)),2))),0.5) * 1.0;
			float dDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosD.x)),2) + pow(abs(v.v.z - (_lControllerPosD.z)),2))),0.5) * 1.0;

            if(_controllerHeldCDTimer < 0){
            	_controllerHeldCDTimer = 0.0;
            }
            if(_controllerHeldCDTimerB < 0){
            	_controllerHeldCDTimerB = 0.0;
            }
            if(_controllerHeldCDTimerC < 0){
            	_controllerHeldCDTimerC = 0.0;
            }
            if(_controllerHeldCDTimerD < 0){
            	_controllerHeldCDTimerD = 0.0;
            }


           	aDist = (_lControllerPos.y - v.v.y) - aDist;
           	bDist = (_lControllerPosB.y - v.v.y) - bDist;
           	cDist = (_lControllerPosC.y - v.v.y) - cDist;
           	dDist = (_lControllerPosD.y - v.v.y) - dDist;
           	aDist = aDist * _controllerHeldCDTimer;
           	bDist = bDist * _controllerHeldCDTimerB;
           	cDist = cDist * _controllerHeldCDTimerC;
           	dDist = dDist * _controllerHeldCDTimerD;
           	if (aDist < 0){
           		aDist = 0.0;
           	}
           	if (bDist < 0){
           		bDist = 0.0;
           	}
           	if (cDist < 0){
           		cDist = 0.0;
           	}
           	if (dDist < 0){
           		dDist = 0.0;
           	}

            float aRatio = 0.0;
            float bRatio = 0.0;
            float cRatio = 0.0;
            float dRatio = 0.0;

            if((aDist + bDist + cDist + dDist) != 0) {
            	aRatio = aDist / (aDist + bDist + cDist + dDist);
            	bRatio = bDist / (aDist + bDist + cDist + dDist);
            	cRatio = cDist / (aDist + bDist + cDist + dDist);
            	dRatio = dDist / (aDist + bDist + cDist + dDist);
            }
            else{
            	aRatio = 0.0;
            	bRatio = 0.0;
            	cRatio = 0.0;
            	dRatio = 0.0;
            }

            //float aRatio = 0.0;
            //float bRatio = 0.0;
            v.v.y = max(v.v.y,v.v.y + ( pow(abs(aRatio),0.75) * aDist) + ( pow(abs(bRatio),0.75) * bDist) + ( pow(abs(cRatio),0.75) * cDist) + ( pow(abs(dRatio),0.75) * dDist));

            //v.v.y = max(v.v.y,v.v.y + (_controllerHeldCDTimer * (((_lControllerPos.y)-v.v.y) - ;
            //v.v.y = max(v.v.y,v.v.y + (_controllerHeldCDTimerB * (((_lControllerPosB.y)-v.v.y) - )));
            //v.v.y = max(v.v.y,v.v.y + (_controllerHeldCDTimerC * (((_lControllerPosC.y)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosC.x)),2) + pow(abs(v.v.z - (_lControllerPosC.z)),2))),0.75))));
            //v.v.y = max(v.v.y,v.v.y + (_controllerHeldCDTimerD * (((_lControllerPosD.y)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosD.x)),2) + pow(abs(v.v.z - (_lControllerPosD.z)),2))),0.75))));

            o.pos = UnityObjectToClipPos(v.v);
            o.col = v.color;
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