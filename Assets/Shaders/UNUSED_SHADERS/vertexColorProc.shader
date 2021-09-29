Shader "Custom/VertexColorProc" {
    Properties {
    	_MainTex ("Color (RGB) Alpha (A)", 2D) = "white" {}
    	_AltTexA ("Color (RGB) Alpha (A)", 2D) = "white" {}
    	_AltTexB ("Color (RGB) Alpha (A)", 2D) = "white" {}
    	_AShapeTex ("Color (RGB) Alpha (A)", 2D) = "white" {}
    	_RenderTex ("Color (RGB) Alpha (A)", 2D) = "white" {}
    	_LitTex ("Albedo", 2D) = "white" {}
		_wavPosA("wavPosA",Vector) = (0,0,0)
		_wavRadA("wavRadA",float) = 0
		_wavIntensityA("wavIntensityA",float) = 0
		_wavPosA("wavPosB",Vector) = (0,0,0)
		_wavRadA("wavRadB",float) = 0
		_wavIntensityA("wavIntensityB",float) = 0
		_wavPosA("wavPosC",Vector) = (0,0,0)
		_wavRadA("wavRadC",float) = 0
		_wavIntensityA("wavIntensityC",float) = 0
		_wavPosA("wavPosD",Vector) = (0,0,0)
		_wavRadA("wavRadD",float) = 0
		_wavIntensityA("wavIntensityD",float) = 0
		_wavPosA("wavPosE",Vector) = (0,0,0)
		_wavRadA("wavRadE",float) = 0
		_wavIntensityA("wavIntensityE",float) = 0
		_wavPosA("wavPosF",Vector) = (0,0,0)
		_wavRadA("wavRadF",float) = 0
		_wavIntensityA("wavIntensityF",float) = 0
		_wavPosA("wavPosG",Vector) = (0,0,0)
		_wavRadA("wavRadG",float) = 0
		_wavIntensityA("wavIntensityG",float) = 0
		_wavPosA("wavPosH",Vector) = (0,0,0)
		_wavRadA("wavRadH",float) = 0
		_wavIntensityA("wavIntensityH",float) = 0
		_controllerHeldCDTimer("controllerHeldCDTimer",float) = 0
		_lControllerPos("lControllerPos",Vector) = (0,0,0)
		_controllerHeldCDTimerB("controllerHeldCDTimerB",float) = 0
		_lControllerPosB("lControllerPosB",Vector) = (0,0,0)
		_controllerHeldCDTimerC("controllerHeldCDTimerC",float) = 0
		_lControllerPosC("lControllerPosC",Vector) = (0,0,0)
		_controllerHeldCDTimerD("controllerHeldCDTimerD",float) = 0
		_lControllerPosD("lControllerPosD",Vector) = (0,0,0)
		//_controllerHeld("controllerHeld",int) = 0
		_bodyPos0("bodyPos0",Vector) = (0,0,0)
		_bodyPos1("bodyPos1",Vector) = (0,0,0)
		_bodyPos2("bodyPos2",Vector) = (0,0,0)
		_bodyPos3("bodyPos3",Vector) = (0,0,0)
		_bodyPos4("bodyPos4",Vector) = (0,0,0)
		_bodyPos5("bodyPos5",Vector) = (0,0,0)
		_vertBounds("vertBounds",Vector) = (0,0,0,0)
		_clothHeight("clothHeight",float) = 0

	}
    SubShader {
    Tags{"Queue"="Transparent" "RenderType"="Transparent" "DisableBatching"="True"}
    //ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    //Tags{"DisableBatching"="True"}
    LOD 200
        
    Pass {
    	Lighting On
    	ColorMaterial AmbientAndDiffuse
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma geometry geom
        #pragma multi_compile_fog
        #pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap
        #pragma target 5.0

        #include "UnityCG.cginc"
 
        struct VertexInput {
            float4 v : POSITION;
            float4 color: COLOR;
            float4 uv : TEXCOORD0;
            float4 uvB : TEXCOORD1;
        };
         
        struct VertexOutput {
            float4 pos : SV_POSITION;
            float4 col : COLOR;
            float4 uv : TEXCOORD0;
            float4 tDriver : TEXCOORD1;
        };

        struct g2f {
        	float4 pos : SV_POSITION;
            float4 col : COLOR;
            //float3 tmp;
            float3 nrm : NORMAL;
            //float2 uv : TEXCOORD0;
            //float4 tDriver : TEXCOORD1;
            UNITY_FOG_COORDS(1)
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;
        sampler2D _AltTexA;
        float4 _AltTexA_ST;
        sampler2D _AltTexB;
        float4 _AltTexB_ST;
        sampler2D _LitTex;
        float4 _LitTex_ST;
        sampler2D _AShapeTex;
        float4 _AShapeTex_ST;
        sampler2D _RenderTex;
        float4 _RenderTex_ST;
        float _clothHeight;

        Vector _wavPosA;
        float _wavRadA;
        float _wavIntensityA;
        Vector _wavPosB;
        float _wavRadB;
        float _wavIntensityB;
        Vector _wavPosC;
        float _wavRadC;
        float _wavIntensityC;
        Vector _wavPosD;
        float _wavRadD;
        float _wavIntensityD;
        Vector _wavPosE;
        float _wavRadE;
        float _wavIntensityE;
        Vector _wavPosF;
        float _wavRadF;
        float _wavIntensityF;
        Vector _wavPosG;
        float _wavRadG;
        float _wavIntensityG;
        Vector _wavPosH;
        float _wavRadH;
        float _wavIntensityH;
        float _controllerHeldCDTimer;
        Vector _lControllerPos;
        float _controllerHeldCDTimerB;
        Vector _lControllerPosB;
        float _controllerHeldCDTimerC;
        Vector _lControllerPosC;
        float _controllerHeldCDTimerD;
        Vector _lControllerPosD;
        //int _controllerHeld;
        Vector _bodyPos0;
        Vector _bodyPos1;
        Vector _bodyPos2;
        Vector _bodyPos3;
        Vector _bodyPos4;
        Vector _bodyPos5;
        Vector _vertBounds;

        float rand(float2 co){
    		return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
		}

        VertexOutput vert(VertexInput v) {
         
            VertexOutput o;

            //v.v.z = v.v.z - (min(abs(_wavRadA),0.000001) * 1000000) * (v.uv.z * max(0.0,(((_wavIntensityA - 1.0)-v.v.z) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosA.x + 3.05)),2) + pow(abs(v.v.y - (_wavPosA.z + 18.0)),2)) - _wavRadA),2))));
            //v.v.z = v.v.z - (min(abs(_wavRadB),0.000001) * 1000000) * (v.uv.z * max(0.0,(((_wavIntensityB - 1.0)-v.v.z) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosB.x + 3.05)),2) + pow(abs(v.v.y - (_wavPosB.z + 18.0)),2)) - _wavRadB),2))));
            //v.v.z = v.v.z - (min(abs(_wavRadC),0.000001) * 1000000) * (v.uv.z * max(0.0,(((_wavIntensityC - 1.0)-v.v.z) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosC.x + 3.05)),2) + pow(abs(v.v.y - (_wavPosC.z + 18.0)),2)) - _wavRadC),2))));
            //v.v.z = v.v.z - (min(abs(_wavRadD),0.000001) * 1000000) * (v.uv.z * max(0.0,(((_wavIntensityD - 1.0)-v.v.z) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosD.x + 3.05)),2) + pow(abs(v.v.y - (_wavPosD.z + 18.0)),2)) - _wavRadD),2))));
            //v.v.z = v.v.z - (min(abs(_wavRadE),0.000001) * 1000000) * (max(0.0,(((_wavIntensityE - 1.0)-v.v.z) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosE.x + 3.05)),2) + pow(abs(v.v.y - (_wavPosE.z + 18.0)),2)) - _wavRadE),2))));
            //v.v.z = v.v.z - (min(abs(_wavRadF),0.000001) * 1000000) * (max(0.0,(((_wavIntensityF - 1.0)-v.v.z) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosF.x + 3.05)),2) + pow(abs(v.v.y - (_wavPosF.z + 18.0)),2)) - _wavRadF),2))));
            //v.v.z = v.v.z - (min(abs(_wavRadG),0.000001) * 1000000) * (max(0.0,(((_wavIntensityG - 1.0)-v.v.z) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosG.x + 3.05)),2) + pow(abs(v.v.y - (_wavPosG.z + 18.0)),2)) - _wavRadG),2))));
            //v.v.z = v.v.z - (min(abs(_wavRadH),0.000001) * 1000000) * (max(0.0,(((_wavIntensityH - 1.0)-v.v.z) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosH.x + 3.05)),2) + pow(abs(v.v.y - (_wavPosH.z + 18.0)),2)) - _wavRadH),2))));

            //v.v.y = max(v.v.y,v.v.y + (min(abs(_wavRadA),0.000001) * 1000000) * (v.uv.z * (((_wavIntensityA)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosA.x)),2) + pow(abs(v.v.z - (_wavPosA.z)),2)) - _wavRadA),2))));
            //v.v.y = max(v.v.y,v.v.y + (min(abs(_wavRadB),0.000001) * 1000000) * (v.uv.z * (((_wavIntensityB)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosB.x)),2) + pow(abs(v.v.z - (_wavPosB.z)),2)) - _wavRadB),2))));
            //v.v.y = max(v.v.y,v.v.y + (min(abs(_wavRadC),0.000001) * 1000000) * (v.uv.z * (((_wavIntensityC)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosC.x)),2) + pow(abs(v.v.z - (_wavPosC.z)),2)) - _wavRadC),2))));
            //v.v.y = max(v.v.y,v.v.y + (min(abs(_wavRadD),0.000001) * 1000000) * (v.uv.z * (((_wavIntensityD)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosD.x)),2) + pow(abs(v.v.z - (_wavPosD.z)),2)) - _wavRadD),2))));
            v.v.y = max(v.v.y,v.v.y + (min(abs(_wavRadA),0.000001) * 1000000) * (((_wavIntensityA)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosA.x)),2) + pow(abs(v.v.z - (_wavPosA.z)),2)) - _wavRadA),2)));
            v.v.y = max(v.v.y,v.v.y + (min(abs(_wavRadB),0.000001) * 1000000) * (((_wavIntensityB)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosB.x)),2) + pow(abs(v.v.z - (_wavPosB.z)),2)) - _wavRadB),2)));
            v.v.y = max(v.v.y,v.v.y + (min(abs(_wavRadC),0.000001) * 1000000) * (((_wavIntensityC)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosC.x)),2) + pow(abs(v.v.z - (_wavPosC.z)),2)) - _wavRadC),2)));
            v.v.y = max(v.v.y,v.v.y + (min(abs(_wavRadD),0.000001) * 1000000) * (((_wavIntensityD)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosD.x)),2) + pow(abs(v.v.z - (_wavPosD.z)),2)) - _wavRadD),2)));
            v.v.y = max(v.v.y,v.v.y + (min(abs(_wavRadE),0.000001) * 1000000) * (((_wavIntensityE)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosE.x)),2) + pow(abs(v.v.z - (_wavPosE.z)),2)) - _wavRadE),2)));
            v.v.y = max(v.v.y,v.v.y + (min(abs(_wavRadF),0.000001) * 1000000) * (((_wavIntensityF)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosF.x)),2) + pow(abs(v.v.z - (_wavPosF.z)),2)) - _wavRadF),2)));
            v.v.y = max(v.v.y,v.v.y + (min(abs(_wavRadG),0.000001) * 1000000) * (((_wavIntensityG)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosG.x)),2) + pow(abs(v.v.z - (_wavPosG.z)),2)) - _wavRadG),2)));
            v.v.y = max(v.v.y,v.v.y + (min(abs(_wavRadH),0.000001) * 1000000) * (((_wavIntensityH)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_wavPosH.x)),2) + pow(abs(v.v.z - (_wavPosH.z)),2)) - _wavRadH),2)));

            //if(_controllerHeld==1){
            //	o.pos = mul(UNITY_MATRIX_MVP, float4(v.v.x,v.v.y,v.v.z - max(0.0,((rnewPosZ-v.v.z) - pow(abs(rposDist),0.75))),v.v.w).xyzw);
			//	o.col = v.color;
            //}

            //newPosX = _pPositionX + 3.05;
            //newPosY = _pPositionY + 18.0;
            //posDist = sqrt(pow(abs(v.v.x - newPosX),2) + pow(abs(v.v.y - newPosY),2));
            //cnewPosX = _cPositionX + 3.05;
            //cnewPosY = _cPositionY + 18.0;
            //cposDist = sqrt(pow(abs(v.v.x - cnewPosX),2) + pow(abs(v.v.y - cnewPosY),2));
            //rnewPosX = _controllerPos.x + 3.05;
            //rnewPosY = (_lControllerPos.z + 18.0);
            //rnewPosZ = _controllerPos.y - 0.675;
            //rposDist = sqrt(pow(abs(v.v.x - rnewPosX),2) + pow(abs(v.v.y - rnewPosY),2));
            //rnewPosZ = _controllerPos.y - 0.675;
            //rposDist = ;
            //rnewPosX = (_controllerPos.x + 3.05);
            //rnewPosY = (_controllerPos.z + 18.0);
            //rnewPosZ = _controllerPos.y - 0.675;
            //rposDist = sqrt(pow(abs(v.v.x - (_controllerPos.x + 3.05)),2) + pow(abs(v.v.y - (_controllerPos.z + 18.0)),2));
            //rposDist = sqrt(pow(abs(v.v.x - rnewPosX),2) + pow(abs(v.v.y - rnewPosY),2));

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

            //if(_controllerHeldCDTimer > 0.0) {
            //REVERT
            //v.v.y = max(v.v.y,v.v.y + (_controllerHeldCDTimer * (((_lControllerPos.y)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPos.x)),2) + pow(abs(v.v.z - (_lControllerPos.z)),2))),0.75))));
            //v.v.y = max(v.v.y,v.v.y + (_controllerHeldCDTimerB * (((_lControllerPosB.y)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosB.x)),2) + pow(abs(v.v.z - (_lControllerPosB.z)),2))),0.75))));
            //v.v.y = max(v.v.y,v.v.y + (_controllerHeldCDTimerC * (((_lControllerPosC.y)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosC.x)),2) + pow(abs(v.v.z - (_lControllerPosC.z)),2))),0.75))));
            //v.v.y = max(v.v.y,v.v.y + (_controllerHeldCDTimerD * (((_lControllerPosD.y)-v.v.y) - pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosD.x)),2) + pow(abs(v.v.z - (_lControllerPosD.z)),2))),0.75))));
            //
            //}
            //if(sqrt(pow(abs(v.v.x - (_wavPosB.x)),2) + pow(abs(v.v.z - (_wavPosB.z)),2)) < _wavRadB && v.uv.z > (1.0 - (_wavIntensityB))){
            //	v.v.y = (_wavIntensityB);
            //}

            //else{
            float totBodyDist;
            if(_bodyPos0.z != 9999) {
	            totBodyDist = sqrt(pow((_bodyPos0.x - v.v.x),2) + pow((_bodyPos0.z - v.v.z),2));
	            v.v.x = v.v.x - (_bodyPos0.x - v.v.x) / (totBodyDist) * (((cos(max(0.000001,min(totBodyDist,1)) * (3.14159265359)) + 1) / 2) * 0.25);
	            v.v.z = v.v.z - (_bodyPos0.z - v.v.z) / (totBodyDist) * (((cos(max(0.000001,min(totBodyDist,1)) * (3.14159265359)) + 1) / 2) * 0.25);
            }
            if(_bodyPos1.z != 9999) {
	            totBodyDist = sqrt(pow((_bodyPos1.x - v.v.x),2) + pow((_bodyPos1.z - v.v.z),2));
	            v.v.x = v.v.x - (_bodyPos1.x - v.v.x) / (totBodyDist) * (((cos(max(0.000001,min(totBodyDist,1)) * (3.14159265359)) + 1) / 2) * 0.25);
	            v.v.z = v.v.z - (_bodyPos1.z - v.v.z) / (totBodyDist) * (((cos(max(0.000001,min(totBodyDist,1)) * (3.14159265359)) + 1) / 2) * 0.25);
            }
            if(_bodyPos2.z != 9999) {
	            totBodyDist = sqrt(pow((_bodyPos2.x - v.v.x),2) + pow((_bodyPos2.z - v.v.z),2));
	            v.v.x = v.v.x - (_bodyPos2.x - v.v.x) / (totBodyDist) * (((cos(max(0.000001,min(totBodyDist,1)) * (3.14159265359)) + 1) / 2) * 0.25);
	            v.v.z = v.v.z - (_bodyPos2.z - v.v.z) / (totBodyDist) * (((cos(max(0.000001,min(totBodyDist,1)) * (3.14159265359)) + 1) / 2) * 0.25);
            }
            if(_bodyPos3.z != 9999) {
	            totBodyDist = sqrt(pow((_bodyPos3.x - v.v.x),2) + pow((_bodyPos3.z - v.v.z),2));
	            v.v.x = v.v.x - (_bodyPos3.x - v.v.x) / (totBodyDist) * (((cos(max(0.000001,min(totBodyDist,1)) * (3.14159265359)) + 1) / 2) * 0.25);
	            v.v.z = v.v.z - (_bodyPos3.z - v.v.z) / (totBodyDist) * (((cos(max(0.000001,min(totBodyDist,1)) * (3.14159265359)) + 1) / 2) * 0.25);
            }
            if(_bodyPos4.z != 9999) {
	            totBodyDist = sqrt(pow((_bodyPos4.x - v.v.x),2) + pow((_bodyPos4.z - v.v.z),2));
	            v.v.x = v.v.x - (_bodyPos4.x - v.v.x) / (totBodyDist) * (((cos(max(0.000001,min(totBodyDist,1)) * (3.14159265359)) + 1) / 2) * 0.25);
	            v.v.z = v.v.z - (_bodyPos4.z - v.v.z) / (totBodyDist) * (((cos(max(0.000001,min(totBodyDist,1)) * (3.14159265359)) + 1) / 2) * 0.25);
            }
            if(_bodyPos5.z != 9999) {
	            totBodyDist = sqrt(pow((_bodyPos5.x - v.v.x),2) + pow((_bodyPos5.z - v.v.z),2));
	            v.v.x = v.v.x - (_bodyPos5.x - v.v.x) / (totBodyDist) * (((cos(max(0.000001,min(totBodyDist,1)) * (3.14159265359)) + 1) / 2) * 0.25);
	            v.v.z = v.v.z - (_bodyPos5.z - v.v.z) / (totBodyDist) * (((cos(max(0.000001,min(totBodyDist,1)) * (3.14159265359)) + 1) / 2) * 0.25);
            }

	        //o.pos = mul(UNITY_MATRIX_MVP, v.v);

	        if((v.v.x < _vertBounds.z && v.v.x > _vertBounds.x)&&(v.v.z < _vertBounds.w && v.v.z > _vertBounds.y)){
	        	float tX = (v.v.x - _vertBounds.x) / (_vertBounds.z - _vertBounds.x);
	        	float tZ = (v.v.z - _vertBounds.y) / (_vertBounds.w - _vertBounds.y);
	        	float4 tPos = float4(tex2Dlod(_RenderTex, float4(tX,tZ,0,0)).xyz, 1);
	        	//tPos.y = tPos.y;
	        	v.v = float4(tPos.x, v.v.y + tPos.y, tPos.z, 1);
	        }

	        o.pos = v.v;

	        o.col.r = 0.5;
	        o.col.g = 0.5;
	        o.col.b = 1.0;
            //}
            o.col.a = v.color.a;
            o.uv = v.uv;
            o.tDriver = v.uvB;
            o.tDriver.x = rand(v.uv.xz);
            return o;
        }

        float4 RotateAroundYInDegrees (float4 vertex, float degrees)
                 {
                     float alpha = degrees * UNITY_PI / 180.0;
                     float sina, cosa;
                     sincos(alpha, sina, cosa);
                     float2x2 m = float2x2(cosa, -sina, sina, cosa);
                     return float4(mul(m, vertex.xz), vertex.yw).xzyw;
                 }

		[maxvertexcount(60)]
		void geom (point VertexOutput input[1], inout TriangleStream<g2f> output)
		{
			float4 myIn = input[0].pos;
			float4 mIn = mul(UNITY_MATRIX_M, input[0].pos);
			//float4 myIn = input[0].vertex;


			//float myR = rand(myIn.xz);
			float myR = input[0].uv.z;


			float myLen = 4.0 * max(0,myR) * max(0,(sin(_Time[1] * rand(input[0].uv.xy)*0.5+0.5)));

			float upLen = sqrt(pow(myLen,2.0) - pow((myLen/2.0),2.0));
			float texLen = 1.8;
			float tupLen = sqrt(pow(texLen,2.0) - pow((texLen/2.0),2.0));
			float yy = tupLen + pow(texLen/2.0,2.0)/tupLen;

			float f = upLen/8.0;
			float4 colors[8] = { float4(rand(input[0].uv.xy),rand(input[0].tDriver.yz),rand(input[0].uv.xz),1.0f),
				float4(rand(input[0].uv.xy),rand(input[0].uv.xz),rand(input[0].uv.yz),1.0f),
				float4(rand(input[0].tDriver.yz),rand(input[0].uv.xy),rand(input[0].uv.xz),1.0f),
				float4(rand(input[0].uv.yz),rand(input[0].uv.xz),rand(input[0].uv.xy),1.0f),
				float4(rand(input[0].uv.xz),rand(input[0].uv.xy),rand(input[0].uv.yz),1.0f),
				float4(rand(input[0].uv.xz),rand(input[0].tDriver.yz),rand(input[0].uv.xy),1.0f),
				float4(rand(input[0].tDriver.yz),rand(input[0].uv.yz),rand(input[0].uv.xy),1.0f),
				float4(rand(input[0].tDriver.yz),rand(input[0].uv.xz),rand(input[0].uv.xy),1.0f) };

			if(rand(input[0].uv.xz)<0.0){
				const float4 VERTICES[56] = { float4( 0.000000, -0.600000, 0.000000, 0.0 ),
				float4( -0.208465, -0.208465, 0.000000, 0.0 ),
				float4( 0.000000, -0.208465, -0.208465, 0.0 ),
				float4( 0.000000, -0.600000, 0.000000, 0.0 ),
				float4( 0.000000, -0.208465, -0.208465, 0.0 ),
				float4( 0.208465, -0.208465, 0.000000, 0.0 ),
				float4( 0.000000, -0.600000, 0.000000, 0.0 ),
				float4( 0.208465, -0.208465, 0.000000, 0.0 ),
				float4( 0.000000, -0.208465, 0.208465, 0.0 ),
				float4( 0.000000, -0.600000, 0.000000, 0.0 ),
				float4( 0.000000, -0.208465, 0.208465, 0.0 ),
				float4( -0.208465, -0.208465, 0.000000, 0.0 ),
				float4( -0.208465, -0.208465, 0.000000, 0.0 ),
				float4( -0.600000, 0.000000, 0.000000, 0.0 ),
				float4( 0.000000, 0.000000, -0.600000, 0.0 ),
				float4( 0.000000, -0.208465, -0.208465, 0.0 ),
				float4( 0.000000, -0.208465, -0.208465, 0.0 ),
				float4( 0.000000, 0.000000, -0.600000, 0.0 ),
				float4( 0.600000, 0.000000, 0.000000, 0.0 ),
				float4( 0.208465, -0.208465, 0.000000, 0.0 ),
				float4( 0.208465, -0.208465, 0.000000, 0.0 ),
				float4( 0.600000, 0.000000, 0.000000, 0.0 ),
				float4( 0.000000, 0.000000, 0.600000, 0.0 ),
				float4( 0.000000, -0.208465, 0.208465, 0.0 ),
				float4( 0.000000, -0.208465, 0.208465, 0.0 ),
				float4( 0.000000, 0.000000, 0.600000, 0.0 ),
				float4( -0.600000, 0.000000, 0.000000, 0.0 ),
				float4( -0.208465, -0.208465, 0.000000, 0.0 ),
				float4( -0.600000, 0.000000, 0.000000, 0.0 ),
				float4( -0.208466, 0.208465, 0.000000, 0.0 ),
				float4( 0.000000, 0.208465, -0.208466, 0.0 ),
				float4( 0.000000, 0.000000, -0.600000, 0.0 ),
				float4( 0.000000, 0.000000, -0.600000, 0.0 ),
				float4( 0.000000, 0.208465, -0.208466, 0.0 ),
				float4( 0.208466, 0.208465, 0.000000, 0.0 ),
				float4( 0.600000, 0.000000, 0.000000, 0.0 ),
				float4( 0.600000, 0.000000, 0.000000, 0.0 ),
				float4( 0.208466, 0.208465, 0.000000, 0.0 ),
				float4( 0.000000, 0.208465, 0.208466, 0.0 ),
				float4( 0.000000, 0.000000, 0.600000, 0.0 ),
				float4( 0.000000, 0.000000, 0.600000, 0.0 ),
				float4( 0.000000, 0.208465, 0.208466, 0.0 ),
				float4( -0.208466, 0.208465, 0.000000, 0.0 ),
				float4( -0.600000, 0.000000, 0.000000, 0.0 ),
				float4( -0.208466, 0.208465, 0.000000, 0.0 ),
				float4( 0.000000, 0.600000, 0.000000, 0.0 ),
				float4( 0.000000, 0.208465, -0.208466, 0.0 ),
				float4( 0.000000, 0.208465, -0.208466, 0.0 ),
				float4( 0.000000, 0.600000, 0.000000, 0.0 ),
				float4( 0.208466, 0.208465, 0.000000, 0.0 ),
				float4( 0.208466, 0.208465, 0.000000, 0.0 ),
				float4( 0.000000, 0.600000, 0.000000, 0.0 ),
				float4( 0.000000, 0.208465, 0.208466, 0.0 ),
				float4( 0.000000, 0.208465, 0.208466, 0.0 ),
				float4( 0.000000, 0.600000, 0.000000, 0.0 ),
				float4( -0.208466, 0.208465, 0.000000, 0.0 ) };

				const int TRIANGLES[72] = { 0, 1, 2,
				3, 4, 5,
				6, 7, 8,
				9, 10, 11,
				12, 13, 14,
				12, 14, 15,
				16, 17, 18,
				16, 18, 19,
				20, 21, 22,
				20, 22, 23,
				24, 25, 26,
				24, 26, 27,
				28, 29, 30,
				28, 30, 31,
				32, 33, 34,
				32, 34, 35,
				36, 37, 38,
				36, 38, 39,
				40, 41, 42,
				40, 42, 43,
				44, 45, 46,
				47, 48, 49,
				50, 51, 52,
				53, 54, 55 };

				g2f o[72];

				float4 myPos;

				for(int i=0;i<72;i++){
				//o[i].uv = float2(0.5, 0.5);
				//o[i].col = colors[TRIANGLES[i]];
				myPos = mIn + (VERTICES[TRIANGLES[i]] * 0.005);
				myPos = mul(UNITY_MATRIX_VP,myPos);
				o[i].pos = myPos;
				UNITY_TRANSFER_FOG(o[i],myPos);
				//o[i].tDriver = input[0].tDriver;
				}

				for(int i=0;i<72; i = i + 3){
				output.Append(o[i]);
				output.Append(o[i+1]);
				output.Append(o[i+2]);
				output.RestartStrip();
				}

			}
			else{
			/*
				int defInd = (int)(rand(input[0].pos.xy) * 176);
				float defVal = rand(input[0].pos.xy);

				const float4 VERTICES[176] = { float4( 0.000000, -0.866025, -0.500000, 0.0 ),
				float4( 0.000000, -1.000000, 0.000000, 0.0 ),
				float4( -0.353553, -0.866025, -0.353553, 0.0 ),
				float4( 0.353553, -0.866025, -0.353553, 0.0 ),
				float4( 0.000000, -1.000000, 0.000000, 0.0 ),
				float4( 0.000000, -0.866025, -0.500000, 0.0 ),
				float4( 0.500000, -0.866025, 0.000000, 0.0 ),
				float4( 0.000000, -1.000000, 0.000000, 0.0 ),
				float4( 0.353553, -0.866025, -0.353553, 0.0 ),
				float4( 0.353553, -0.866025, 0.353553, 0.0 ),
				float4( 0.000000, -1.000000, 0.000000, 0.0 ),
				float4( 0.500000, -0.866025, 0.000000, 0.0 ),
				float4( 0.000000, -0.866025, 0.500000, 0.0 ),
				float4( 0.000000, -1.000000, 0.000000, 0.0 ),
				float4( 0.353553, -0.866025, 0.353553, 0.0 ),
				float4( -0.353553, -0.866025, 0.353553, 0.0 ),
				float4( 0.000000, -1.000000, 0.000000, 0.0 ),
				float4( 0.000000, -0.866025, 0.500000, 0.0 ),
				float4( -0.500000, -0.866025, 0.000000, 0.0 ),
				float4( 0.000000, -1.000000, 0.000000, 0.0 ),
				float4( -0.353553, -0.866025, 0.353553, 0.0 ),
				float4( -0.353553, -0.866025, -0.353553, 0.0 ),
				float4( 0.000000, -1.000000, 0.000000, 0.0 ),
				float4( -0.500000, -0.866025, 0.000000, 0.0 ),
				float4( -0.353553, 0.866025, -0.353553, 0.0 ),
				float4( 0.000000, 1.000000, 0.000000, 0.0 ),
				float4( 0.000000, 0.866025, -0.500000, 0.0 ),
				float4( 0.000000, 0.866025, -0.500000, 0.0 ),
				float4( 0.000000, 1.000000, 0.000000, 0.0 ),
				float4( 0.353553, 0.866025, -0.353553, 0.0 ),
				float4( 0.353553, 0.866025, -0.353553, 0.0 ),
				float4( 0.000000, 1.000000, 0.000000, 0.0 ),
				float4( 0.500000, 0.866025, 0.000000, 0.0 ),
				float4( 0.500000, 0.866025, 0.000000, 0.0 ),
				float4( 0.000000, 1.000000, 0.000000, 0.0 ),
				float4( 0.353553, 0.866025, 0.353553, 0.0 ),
				float4( 0.353553, 0.866025, 0.353553, 0.0 ),
				float4( 0.000000, 1.000000, 0.000000, 0.0 ),
				float4( 0.000000, 0.866025, 0.500000, 0.0 ),
				float4( 0.000000, 0.866025, 0.500000, 0.0 ),
				float4( 0.000000, 1.000000, 0.000000, 0.0 ),
				float4( -0.353553, 0.866025, 0.353553, 0.0 ),
				float4( -0.353553, 0.866025, 0.353553, 0.0 ),
				float4( 0.000000, 1.000000, 0.000000, 0.0 ),
				float4( -0.500000, 0.866025, 0.000000, 0.0 ),
				float4( -0.500000, 0.866025, 0.000000, 0.0 ),
				float4( 0.000000, 1.000000, 0.000000, 0.0 ),
				float4( -0.353553, 0.866025, -0.353553, 0.0 ),
				float4( -0.353553, -0.866025, -0.353553, 0.0 ),
				float4( -0.612372, -0.500000, -0.612372, 0.0 ),
				float4( 0.000000, -0.866025, -0.500000, 0.0 ),
				float4( 0.000000, -0.500000, -0.866025, 0.0 ),
				float4( 0.000000, -0.866025, -0.500000, 0.0 ),
				float4( 0.000000, -0.500000, -0.866025, 0.0 ),
				float4( 0.353553, -0.866025, -0.353553, 0.0 ),
				float4( 0.612372, -0.500000, -0.612372, 0.0 ),
				float4( 0.353553, -0.866025, -0.353553, 0.0 ),
				float4( 0.612372, -0.500000, -0.612372, 0.0 ),
				float4( 0.500000, -0.866025, 0.000000, 0.0 ),
				float4( 0.866025, -0.500000, 0.000000, 0.0 ),
				float4( 0.500000, -0.866025, 0.000000, 0.0 ),
				float4( 0.866025, -0.500000, 0.000000, 0.0 ),
				float4( 0.353553, -0.866025, 0.353553, 0.0 ),
				float4( 0.612372, -0.500000, 0.612372, 0.0 ),
				float4( 0.353553, -0.866025, 0.353553, 0.0 ),
				float4( 0.612372, -0.500000, 0.612372, 0.0 ),
				float4( 0.000000, -0.866025, 0.500000, 0.0 ),
				float4( 0.000000, -0.500000, 0.866025, 0.0 ),
				float4( 0.000000, -0.866025, 0.500000, 0.0 ),
				float4( 0.000000, -0.500000, 0.866025, 0.0 ),
				float4( -0.353553, -0.866025, 0.353553, 0.0 ),
				float4( -0.612372, -0.500000, 0.612372, 0.0 ),
				float4( -0.353553, -0.866025, 0.353553, 0.0 ),
				float4( -0.612372, -0.500000, 0.612372, 0.0 ),
				float4( -0.500000, -0.866025, 0.000000, 0.0 ),
				float4( -0.866025, -0.500000, 0.000000, 0.0 ),
				float4( -0.500000, -0.866025, 0.000000, 0.0 ),
				float4( -0.866025, -0.500000, 0.000000, 0.0 ),
				float4( -0.353553, -0.866025, -0.353553, 0.0 ),
				float4( -0.612372, -0.500000, -0.612372, 0.0 ),
				float4( -0.612372, -0.500000, -0.612372, 0.0 ),
				float4( -0.707107, 0.000000, -0.707107, 0.0 ),
				float4( 0.000000, -0.500000, -0.866025, 0.0 ),
				float4( 0.000000, 0.000000, -1.000000, 0.0 ),
				float4( 0.000000, -0.500000, -0.866025, 0.0 ),
				float4( 0.000000, 0.000000, -1.000000, 0.0 ),
				float4( 0.612372, -0.500000, -0.612372, 0.0 ),
				float4( 0.707107, 0.000000, -0.707107, 0.0 ),
				float4( 0.612372, -0.500000, -0.612372, 0.0 ),
				float4( 0.707107, 0.000000, -0.707107, 0.0 ),
				float4( 0.866025, -0.500000, 0.000000, 0.0 ),
				float4( 1.000000, 0.000000, 0.000000, 0.0 ),
				float4( 0.866025, -0.500000, 0.000000, 0.0 ),
				float4( 1.000000, 0.000000, 0.000000, 0.0 ),
				float4( 0.612372, -0.500000, 0.612372, 0.0 ),
				float4( 0.707107, 0.000000, 0.707107, 0.0 ),
				float4( 0.612372, -0.500000, 0.612372, 0.0 ),
				float4( 0.707107, 0.000000, 0.707107, 0.0 ),
				float4( 0.000000, -0.500000, 0.866025, 0.0 ),
				float4( 0.000000, 0.000000, 1.000000, 0.0 ),
				float4( 0.000000, -0.500000, 0.866025, 0.0 ),
				float4( 0.000000, 0.000000, 1.000000, 0.0 ),
				float4( -0.612372, -0.500000, 0.612372, 0.0 ),
				float4( -0.707107, 0.000000, 0.707107, 0.0 ),
				float4( -0.612372, -0.500000, 0.612372, 0.0 ),
				float4( -0.707107, 0.000000, 0.707107, 0.0 ),
				float4( -0.866025, -0.500000, 0.000000, 0.0 ),
				float4( -1.000000, 0.000000, 0.000000, 0.0 ),
				float4( -0.866025, -0.500000, 0.000000, 0.0 ),
				float4( -1.000000, 0.000000, 0.000000, 0.0 ),
				float4( -0.612372, -0.500000, -0.612372, 0.0 ),
				float4( -0.707107, 0.000000, -0.707107, 0.0 ),
				float4( -0.707107, 0.000000, -0.707107, 0.0 ),
				float4( -0.612372, 0.500000, -0.612372, 0.0 ),
				float4( 0.000000, 0.000000, -1.000000, 0.0 ),
				float4( 0.000000, 0.500000, -0.866025, 0.0 ),
				float4( 0.000000, 0.000000, -1.000000, 0.0 ),
				float4( 0.000000, 0.500000, -0.866025, 0.0 ),
				float4( 0.707107, 0.000000, -0.707107, 0.0 ),
				float4( 0.612372, 0.500000, -0.612372, 0.0 ),
				float4( 0.707107, 0.000000, -0.707107, 0.0 ),
				float4( 0.612372, 0.500000, -0.612372, 0.0 ),
				float4( 1.000000, 0.000000, 0.000000, 0.0 ),
				float4( 0.866025, 0.500000, 0.000000, 0.0 ),
				float4( 1.000000, 0.000000, 0.000000, 0.0 ),
				float4( 0.866025, 0.500000, 0.000000, 0.0 ),
				float4( 0.707107, 0.000000, 0.707107, 0.0 ),
				float4( 0.612372, 0.500000, 0.612372, 0.0 ),
				float4( 0.707107, 0.000000, 0.707107, 0.0 ),
				float4( 0.612372, 0.500000, 0.612372, 0.0 ),
				float4( 0.000000, 0.000000, 1.000000, 0.0 ),
				float4( 0.000000, 0.500000, 0.866025, 0.0 ),
				float4( 0.000000, 0.000000, 1.000000, 0.0 ),
				float4( 0.000000, 0.500000, 0.866025, 0.0 ),
				float4( -0.707107, 0.000000, 0.707107, 0.0 ),
				float4( -0.612372, 0.500000, 0.612372, 0.0 ),
				float4( -0.707107, 0.000000, 0.707107, 0.0 ),
				float4( -0.612372, 0.500000, 0.612372, 0.0 ),
				float4( -1.000000, 0.000000, 0.000000, 0.0 ),
				float4( -0.866025, 0.500000, 0.000000, 0.0 ),
				float4( -1.000000, 0.000000, 0.000000, 0.0 ),
				float4( -0.866025, 0.500000, 0.000000, 0.0 ),
				float4( -0.707107, 0.000000, -0.707107, 0.0 ),
				float4( -0.612372, 0.500000, -0.612372, 0.0 ),
				float4( -0.612372, 0.500000, -0.612372, 0.0 ),
				float4( -0.353553, 0.866025, -0.353553, 0.0 ),
				float4( 0.000000, 0.500000, -0.866025, 0.0 ),
				float4( 0.000000, 0.866025, -0.500000, 0.0 ),
				float4( 0.000000, 0.500000, -0.866025, 0.0 ),
				float4( 0.000000, 0.866025, -0.500000, 0.0 ),
				float4( 0.612372, 0.500000, -0.612372, 0.0 ),
				float4( 0.353553, 0.866025, -0.353553, 0.0 ),
				float4( 0.612372, 0.500000, -0.612372, 0.0 ),
				float4( 0.353553, 0.866025, -0.353553, 0.0 ),
				float4( 0.866025, 0.500000, 0.000000, 0.0 ),
				float4( 0.500000, 0.866025, 0.000000, 0.0 ),
				float4( 0.866025, 0.500000, 0.000000, 0.0 ),
				float4( 0.500000, 0.866025, 0.000000, 0.0 ),
				float4( 0.612372, 0.500000, 0.612372, 0.0 ),
				float4( 0.353553, 0.866025, 0.353553, 0.0 ),
				float4( 0.612372, 0.500000, 0.612372, 0.0 ),
				float4( 0.353553, 0.866025, 0.353553, 0.0 ),
				float4( 0.000000, 0.500000, 0.866025, 0.0 ),
				float4( 0.000000, 0.866025, 0.500000, 0.0 ),
				float4( 0.000000, 0.500000, 0.866025, 0.0 ),
				float4( 0.000000, 0.866025, 0.500000, 0.0 ),
				float4( -0.612372, 0.500000, 0.612372, 0.0 ),
				float4( -0.353553, 0.866025, 0.353553, 0.0 ),
				float4( -0.612372, 0.500000, 0.612372, 0.0 ),
				float4( -0.353553, 0.866025, 0.353553, 0.0 ),
				float4( -0.866025, 0.500000, 0.000000, 0.0 ),
				float4( -0.500000, 0.866025, 0.000000, 0.0 ),
				float4( -0.866025, 0.500000, 0.000000, 0.0 ),
				float4( -0.500000, 0.866025, 0.000000, 0.0 ),
				float4( -0.612372, 0.500000, -0.612372, 0.0 ),
				float4( -0.353553, 0.866025, -0.353553, 0.0 ) * defVal };

				const int TRIANGLES[240] = { 0, 1, 2,
				3, 4, 5,
				6, 7, 8,
				9, 10, 11,
				12, 13, 14,
				15, 16, 17,
				18, 19, 20,
				21, 22, 23,
				24, 25, 26,
				27, 28, 29,
				30, 31, 32,
				33, 34, 35,
				36, 37, 38,
				39, 40, 41,
				42, 43, 44,
				45, 46, 47,
				48, 49, 50,
				49, 51, 50,
				52, 53, 54,
				53, 55, 54,
				56, 57, 58,
				57, 59, 58,
				60, 61, 62,
				61, 63, 62,
				64, 65, 66,
				65, 67, 66,
				68, 69, 70,
				69, 71, 70,
				72, 73, 74,
				73, 75, 74,
				76, 77, 78,
				77, 79, 78,
				80, 81, 82,
				81, 83, 82,
				84, 85, 86,
				85, 87, 86,
				88, 89, 90,
				89, 91, 90,
				92, 93, 94,
				93, 95, 94,
				96, 97, 98,
				97, 99, 98,
				100, 101, 102,
				101, 103, 102,
				104, 105, 106,
				105, 107, 106,
				108, 109, 110,
				109, 111, 110,
				112, 113, 114,
				113, 115, 114,
				116, 117, 118,
				117, 119, 118,
				120, 121, 122,
				121, 123, 122,
				124, 125, 126,
				125, 127, 126,
				128, 129, 130,
				129, 131, 130,
				132, 133, 134,
				133, 135, 134,
				136, 137, 138,
				137, 139, 138,
				140, 141, 142,
				141, 143, 142,
				144, 145, 146,
				145, 147, 146,
				148, 149, 150,
				149, 151, 150,
				152, 153, 154,
				153, 155, 154,
				156, 157, 158,
				157, 159, 158,
				160, 161, 162,
				161, 163, 162,
				164, 165, 166,
				165, 167, 166,
				168, 169, 170,
				169, 171, 170,
				172, 173, 174,
				173, 175, 174 };

				static g2f o[240];


				for(int i=0;i<240;i++){
				//o[i].uv = float2(0.5, 0.5);
				//o[i].col = colors[TRIANGLES[i]];
				o[i].pos = mIn + VERTICES[TRIANGLES[i]] * 0.005;
				o[i].pos = o[i].pos = mul(UNITY_MATRIX_VP,o[i].pos);
				UNITY_TRANSFER_FOG(o[i],o[i].pos);
				//o[i].tDriver = input[0].tDriver;
				}

				for(int i=0;i<240; i = i + 3){
				output.Append(o[i]);
				output.Append(o[i+1]);
				output.Append(o[i+2]);
				output.RestartStrip();
				}
			*/
				//Small Triambic Icosahedron
				/*
				const float C0 = 0.276393202250021030359082633127;
				const float C1 = 0.381966011250105151795413165634;
				const float C2 = 0.447213595499957939281834733746;
				const float C3 = 0.618033988749894848204586834366;
				const float C4 = 0.723606797749978969640917366873;

				const float3 VERTICES[32] = { float3(0.0,  C0,  C4),
				float3(0.0,  C0, -C4),
				float3(0.0, -C0,  C4),
				float3(0.0, -C0, -C4),
				float3( C4, 0.0,  C0),
				float3( C4, 0.0, -C0),
				float3(-C4, 0.0,  C0),
				float3(-C4, 0.0, -C0),
				float3( C0,  C4, 0.0),
				float3( C0, -C4, 0.0),
				float3(-C0,  C4, 0.0),
				float3(-C0, -C4, 0.0),
				float3( C1, 0.0,  C3),
				float3( C1, 0.0, -C3),
				float3(-C1, 0.0,  C3),
				float3(-C1, 0.0, -C3),
				float3( C3,  C1, 0.0),
				float3( C3, -C1, 0.0),
				float3(-C3,  C1, 0.0),
				float3(-C3, -C1, 0.0),
				float3(0.0,  C3,  C1),
				float3(0.0,  C3, -C1),
				float3(0.0, -C3,  C1),
				float3(0.0, -C3, -C1),
				float3( C2,  C2,  C2),
				float3( C2,  C2, -C2),
				float3( C2, -C2,  C2),
				float3( C2, -C2, -C2),
				float3(-C2,  C2,  C2),
				float3(-C2,  C2, -C2),
				float3(-C2, -C2,  C2),
				float3(-C2, -C2, -C2) };

				const int TRIANGLES[120] = { 12, 0, 14, 30, 22, 26,
				12, 26, 17,  5, 16, 24,
				12, 24, 20, 28, 14,  2,
				12,  2, 22,  9, 17,  4,
				12,  4, 16,  8, 20,  0,
				15,  1, 13, 27, 23, 31,
				15, 31, 19,  6, 18, 29,
				15, 29, 21, 25, 13,  3,
				15,  3, 23, 11, 19,  7,
				15,  7, 18, 10, 21,  1,
				13,  1, 21,  8, 16,  5,
				13,  5, 17,  9, 23,  3,
				13, 25, 16,  4, 17, 27,
				14,  0, 20, 10, 18,  6,
				14,  6, 19, 11, 22,  2,
				14, 28, 18,  7, 19, 30,
				20,  8, 21, 29, 18, 28,
				20, 24, 16, 25, 21, 10,
				22, 11, 23, 27, 17, 26,
				22, 30, 19, 31, 23,  9 };

				static g2f o[120];


				for(int i=0;i<120;i++){
				//o[i].uv = float2(0.5, 0.5);
				//o[i].col = colors[TRIANGLES[i]];
				o[i].pos = float4(0,0,0,0);
				o[i].pos.xyz = mIn + VERTICES[TRIANGLES[119 - i]] * 10000.05;
				o[i].pos = o[i].pos = mul(UNITY_MATRIX_VP,o[i].pos);
				UNITY_TRANSFER_FOG(o[i],o[i].pos);
				//o[i].tDriver = input[0].tDriver;
				}

				for(int i=0;i<120; i = i + 3){
				output.Append(o[i]);
				output.Append(o[i+1]);
				output.Append(o[i+2]);
				output.RestartStrip();
				}
				*/

				int defInd = (int)(rand(input[0].pos.xz) * 176);
				float defVal = (rand(input[0].pos.xz) - 0.5) * 8.0;
				float randScale = 0.5;
				float baseScale = myLen;

				float3 VERTICES[12] = {float3(0, -0.525731, 0.850651) * (baseScale - rand(input[0].pos.xz) * randScale),
				float3(0.850651,  0,  0.525731) * (baseScale - rand(input[0].pos.xy) * randScale),
				float3(0.850651,  0,  -0.525731) * (baseScale - rand(input[0].pos.yz) * randScale),
				float3(-0.850651,  0,  -0.525731) * (baseScale - rand(input[0].uv.xz) * randScale),
				float3(-0.850651,  0,  0.525731) * (baseScale - rand(input[0].uv.xy) * randScale),
				float3(-0.525731,  0.850651,  0) * (baseScale - rand(input[0].uv.yz) * randScale),
				float3(0.525731,  0.850651,  0) * (baseScale - -rand(input[0].pos.xz) * randScale),
				float3(0.525731,  -0.850651,  0) * (baseScale - -rand(input[0].pos.xy) * randScale),
				float3(-0.525731,  -0.850651,  0) * (baseScale - -rand(input[0].pos.yz) * randScale),
				float3(0,  -0.525731,  -0.850651) * (baseScale - -rand(input[0].uv.xz) * randScale),
				float3(0,  0.525731,  -0.850651) * (baseScale - -rand(input[0].uv.xy) * randScale),
				float3(0,  0.525731,  0.850651) * (baseScale - -rand(input[0].uv.yz * randScale))};

				const int TRIANGLES[60] = { 2,  3,  7,
				2,  8,  3,
				4,  5,  6,
				5,  4,  9,
				7,  6,  12,
				6,  7,  11,
				10,  11,  3,
				11,  10,  4,
				8,  9,  10,
				9,  8,  1,
				12,  1,  2,
				1,  12,  5,
				7,  3,  11,
				2,  7,  12,
				4,  6,  11,
				6,  5,  12,
				3,  8,  10,
				8,  2,  1,
				4,  10,  9,
				5,  9,  1 };

				static g2f o[60];

				const float4 COLS[12] = {float4(1,0,0,1),
				float4(.8,.5,0,1),
				float4(0,1,0,1),
				float4(0,.8,.5,1),
				float4(0,0,1,1),
				float4(.5,0,.8,1),
				float4(.5,.8,0,1),
				float4(0,.5,.8,1),
				float4(.8,0,.5,1),
				float4(.25,.25,.25,1),
				float4(.5,.5,.5,1),
				float4(1,1,1,1)};


				for(int i=0;i<12;i++){
					VERTICES[i] = RotateAroundYInDegrees (float4(VERTICES[i],0),rand(input[0].uv.xy)*360.0).xyz;
				}

				for(int i=0;i<60;i++){
					//o[i].uv = float2(0.5, 0.5);
					//o[i].uv = COLS[TRIANGLES[i]-1].rg;
					o[i].col = COLS[TRIANGLES[i]-1];
					//o[i].pos = mIn + RotateAroundYInDegrees (float4(VERTICES[TRIANGLES[i]-1],1.0),fmod(rand(input[0].uv.xy)*_Time.y*10,360.0)) * 0.15;
					//o[i].pos = mIn + RotateAroundYInDegrees (float4(VERTICES[TRIANGLES[i]-1],1.0),rand(input[0].uv.xy)*360.0) * 0.00375;
					o[i].pos = mIn + float4(VERTICES[TRIANGLES[i]-1],0.0) * 0.00375;
					//o[i].pos = o[i].pos = mul(UNITY_MATRIX_VP,o[i].pos);
					UNITY_TRANSFER_FOG(o[i],o[i].pos);
					//o[i].tDriver = input[0].tDriver;
				}

				for(int i=0;i<60; i = i + 3){
					float3 ab = o[i+1].pos - o[i].pos;
					float3 bc = o[i+2].pos - o[i].pos;
					float3 normals = -normalize(cross(bc,ab));

					//normals = ( mul(UNITY_MATRIX_MV, float4(normals,0)) ).xyz;

					o[i].nrm = normals;
					o[i+1].nrm = normals;
					o[i+2].nrm = normals;
					o[i].pos = mul(UNITY_MATRIX_VP,o[i].pos);
					o[i+1].pos = mul(UNITY_MATRIX_VP,o[i+1].pos);
					o[i+2].pos = mul(UNITY_MATRIX_VP,o[i+2].pos);
					output.Append(o[i+0]);
					output.Append(o[i+1]);
					output.Append(o[i+2]);
					output.RestartStrip();
				}
			}
			/*
			else{
				const float4 VERTICES[48] = { float4( 0.000000, -0.204448, 0.000000, 0.0 ),
				float4( -0.139848, -0.139848, 0.000000, 0.0 ),
				float4( 0.000000, 0.069632, 0.069632, 0.0 ),
				float4( 0.000000, -0.204448, 0.000000, 0.0 ),
				float4( 0.000000, 0.069632, 0.069632, 0.0 ),
				float4( 0.303588, -0.303588, 0.000000, 0.0 ),
				float4( 0.000000, -0.204448, 0.000000, 0.0 ),
				float4( 0.303588, -0.303588, 0.000000, 0.0 ),
				float4( 0.000000, 0.413689, -0.413689, 0.0 ),
				float4( 0.000000, -0.204448, 0.000000, 0.0 ),
				float4( 0.000000, 0.413689, -0.413689, 0.0 ),
				float4( -0.139848, -0.139848, 0.000000, 0.0 ),
				float4( -0.139848, -0.139848, 0.000000, 0.0 ),
				float4( -0.600000, 0.000000, 0.000000, 0.0 ),
				float4( 0.000000, 0.000000, -0.303752, 0.0 ),
				float4( 0.000000, 0.069632, 0.069632, 0.0 ),
				float4( 0.831564, 0.000000, 0.000000, 0.0 ),
				float4( 0.303588, -0.303588, 0.000000, 0.0 ),
				float4( 0.303588, -0.303588, 0.000000, 0.0 ),
				float4( 0.831564, 0.000000, 0.000000, 0.0 ),
				float4( 0.000000, 0.000000, -0.182820, 0.0 ),
				float4( 0.000000, 0.413689, -0.413689, 0.0 ),
				float4( 0.000000, 0.413689, -0.413689, 0.0 ),
				float4( 0.000000, 0.000000, -0.182820, 0.0 ),
				float4( -0.600000, 0.000000, 0.000000, 0.0 ),
				float4( -0.139848, -0.139848, 0.000000, 0.0 ),
				float4( -0.600000, 0.000000, 0.000000, 0.0 ),
				float4( -0.536939, 0.536939, 0.000000, 0.0 ),
				float4( 0.000000, 0.327460, -0.327460, 0.0 ),
				float4( 0.000000, 0.000000, -0.303752, 0.0 ),
				float4( 0.000000, 0.000000, -0.303752, 0.0 ),
				float4( 0.000000, 0.327460, -0.327460, 0.0 ),
				float4( 0.700680, 0.700679, 0.000000, 0.0 ),
				float4( 0.831564, 0.000000, 0.000000, 0.0 ),
				float4( 0.831564, 0.000000, 0.000000, 0.0 ),
				float4( 0.700680, 0.700679, 0.000000, 0.0 ),
				float4( 0.000000, -0.016598, -0.016598, 0.0 ),
				float4( 0.000000, 0.000000, -0.182820, 0.0 ),
				float4( -0.536939, 0.536939, 0.000000, 0.0 ),
				float4( -0.600000, 0.000000, 0.000000, 0.0 ),
				float4( 0.000000, 0.165225, 0.000000, 0.0 ),
				float4( 0.000000, 0.165225, 0.000000, 0.0 ),
				float4( -0.536939, 0.536939, 0.000000, 0.0 ),
				float4( 0.000000, 0.165225, 0.000000, 0.0 ),
				float4( 0.000000, 0.327460, -0.327460, 0.0 ),
				float4( 0.000000, 0.327460, -0.327460, 0.0 ),
				float4( 0.000000, 0.165225, 0.000000, 0.0 ),
				float4( 0.700680, 0.700679, 0.000000, 0.0 ) };

				const int TRIANGLES[72] = { 0, 1, 2,
				3, 4, 5,
				6, 7, 8,
				9, 10, 11,
				12, 13, 14,
				12, 14, 15,
				15, 14, 16,
				15, 16, 17,
				18, 19, 20,
				18, 20, 21,
				22, 23, 24,
				22, 24, 25,
				26, 27, 28,
				26, 28, 29,
				30, 31, 32,
				30, 32, 33,
				34, 35, 36,
				34, 36, 37,
				37, 36, 38,
				37, 38, 39,
				35, 40, 36,
				36, 41, 38,
				42, 43, 44,
				45, 46, 47 };

				g2f o[72];

				for(int i=0;i<72;i++){
				//o[i].uv = float2(0.5, 0.5);
				//o[i].col = colors[TRIANGLES[i]];
				o[i].pos = mIn + VERTICES[TRIANGLES[i]] * 0.005;
				o[i].pos = o[i].pos = mul(UNITY_MATRIX_VP,o[i].pos);
				UNITY_TRANSFER_FOG(o[i],o[i].pos);
				//o[i].tDriver = input[0].tDriver;
				}

				for(int i=0;i<72; i = i + 3){
				output.Append(o[i]);
				output.Append(o[i+1]);
				output.Append(o[i+2]);
				output.RestartStrip();
				}

			}
			*/
		}
         
        float4 frag(g2f i) : SV_Target {
        	float4 col;

        	const float4 colors[8] = { float4(0.0,0.75,0.1,1.0f),
				float4(0.5,1.0,0.0,1.0f),
				float4(0.75,0.5,0.0,1.0f),
				float4(0.0,1.0,0.5,1.0f),
				float4(1.0,0.0,0.75,1.0f),
				float4(1.0,0.75,0.0,1.0f),
				float4(0.65,0.0,1.0,1.0f),
				float4(1.0,0.65,0.0,1.0f) };
        	//if(i.tDriver.x<.3333){
            //	col = tex2D(_MainTex, i.uv);
            //}
            //else if(i.tDriver.x<.6667){
            //	col = tex2D(_AltTexA, i.uv);
            //}
            //else{
            //	col = tex2D(_AltTexB, i.uv);
            //}

            //col = i.col;
            int rIndex = int(floor(rand(i.pos.xy) + 0.5) * 1 + floor(rand(i.pos.yz) + 0.5) * 2 + floor(rand(i.pos.xz) + 0.5) * 4);
            //col = colors[rIndex];
            float outCol = (dot(mul(UNITY_MATRIX_VP,float4(i.nrm,0)).xyz,float3(1,-1,1))*0.5 + 0.5)*1.0;
            //col = float4(outCol,outCol,outCol,1);
            //outCol = 1;
            col = float4(i.col.rgb * outCol,1);

            UNITY_APPLY_FOG(i.fogCoord, col);
            //if(col.a < 0.1) discard;
            return col;
        }
 
        ENDCG
        } 
    }
 
}