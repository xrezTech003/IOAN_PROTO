Shader "Custom/MQStarShader" 
{
    Properties
	{
    	_baseColorValMinRange("baseColorValMinRange",Vector) = (1,0,0)
		_rotSpeedMinRange("rotSpeedMinRange",Vector) = (1,0,0)
		_baseSizeMinRange("baseSizeMinRange",Vector) = (1,0,0)
		_extensionOscFreqMinRange("extensionOscFreqMinRange",Vector) = (1,0,0)
		_extensionOscRangeMinRange("extensionOscRangeMinRange",Vector) = (1,0,0)
		_baseColorSatMinRange("baseColorSatMinRange",Vector) = (1,0,0)
		_extensionScalarMinRange("extensionScalarMinRange",Vector) = (1,0,0)
		_oscSizeMinRange("oscSizeMinRange",Vector) = (1,0,0)
		_oscSizeFreqMinRange("oscSizeFreqMinRange",Vector) = (1,0,0)
		_wavStickTimeMod("wavStickTimeMod",float) = 0
		_wavStickTimeRange("wavStickTimeRange",float) = 0
		_wavStickTimePower("wavStickTimePower",float) = 0
		_paramCutoff("paramCutoff",float) = 0

    	_RenderTex ("Color (RGB) Alpha (A)", 2D) = "white" {}
    	_LitTex ("Albedo", 2D) = "white" {}

		_paramColorMod0("paramColorMod0", Color) = (1, 1, 1, 1)
		_paramColorMod1("paramColorMod1", Color) = (1, 1, 1, 1)
		_paramColorMod2("paramColorMod2", Color) = (1, 1, 1, 1)
		_paramColorMod3("paramColorMod3", Color) = (1, 1, 1, 1)

		_controllerHeldCDTimer("controllerHeldCDTimer",float) = 0
		_lControllerPos("lControllerPos",Vector) = (0,0,0)
		_controllerHeldCDTimerB("controllerHeldCDTimerB",float) = 0
		_lControllerPosB("lControllerPosB",Vector) = (0,0,0)
		_controllerHeldCDTimerC("controllerHeldCDTimerC",float) = 0
		_lControllerPosC("lControllerPosC",Vector) = (0,0,0)
		_controllerHeldCDTimerD("controllerHeldCDTimerD",float) = 0
		_lControllerPosD("lControllerPosD",Vector) = (0,0,0)

		_vertBounds("vertBounds",Vector) = (0,0,0,0)
	}

    SubShader 
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"DisableBatching" = "False"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		LOD 200
        
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma multi_compile_fog
			#pragma target 5.0

			#include "UnityCG.cginc"

			uniform StructuredBuffer<float3> wavPosBuff : register(t5);
			uniform StructuredBuffer<float> wavRadBuff : register(t6);
			uniform StructuredBuffer<float> wavIntBuff : register(t7);
			uniform StructuredBuffer<int> wavParamBuff : register(t8);
			
			//IO Structs

			struct VertexInput 
			{
				float4 v : POSITION;
				float4 color: COLOR;
				float4 uv : TEXCOORD0;
				float4 uvB : TEXCOORD1;
			};
         
			struct VertexOutput 
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR;
				float4 uv : TEXCOORD0;
				float4 uvB : TEXCOORD1;
				float4 oldPos : TEXCOORD2;

				float4 colMod : TEXCOORD3;
			};

			struct g2f 
			{
        		float4 pos : SV_POSITION;
				float4 col : COLOR;
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(2)

				float4 colMod : TEXCOORD3;
			};

			///User Functions
			//Creates a random float value given a float2
			//Source: https://answers.unity.com/questions/399751/randomity-in-cg-shaders-beginner.html
			float rand(float2 co)
			{
				return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
			}

			float denormalizeToRange(float val, float min, float max)
			{
				return (val * (max - min)) + min;
			}

			//Converts HSV values to RGB values
			//Hue, Saturation, Value
			//Red, Green, Blue
			//Formula Source: https://www.rapidtables.com/convert/color/hsv-to-rgb.html
			float3 HSVtoRGB(float3 HSV)
			{
				float3 RGB;
				RGB.x = abs(HSV.x * 6 - 3) - 1;
				RGB.y = 2 - abs(HSV.x * 6 - 2);
				RGB.z = 2 - abs(HSV.x * 6 - 4);
				RGB = saturate(RGB);

				return ((RGB - 1) * HSV.y + 1) * HSV.z;
			}

			//Source: https://www.rapidtables.com/convert/color/rgb-to-hsv.html
			float3 RGB2HSV(float3 rgb)
			{
				float cMax = max(max(rgb.r, rgb.g), rgb.b);
				float cMin = min(min(rgb.r, rgb.g), rgb.b);
				float delta = cMax - cMin;

				float hue;

				if (delta == 0.0) hue = 0.0;
				else if (cMax == rgb.r) hue = (((rgb.g - rgb.b) / delta) + 6.0) % 6.0;
				else if (cMax == rgb.g) hue = (rgb.b - rgb.r) / delta + 2.0;
				else if (cMax == rgb.b) hue = (rgb.r - rgb.g) / delta + 4.0;
				hue *= 60.0;

				float sat = (cMax == 0.0) ? 0.0 : delta / cMax;

				return float3(hue, sat, cMax);
			}

			//Source: https://www.rapidtables.com/convert/color/hsv-to-rgb.html
			float3 HSV2RGB(float3 hsv)
			{
				float c = hsv.z * hsv.y;
				float x = c * (1.0 - abs((hsv.x / 60.0) % 2.0 - 1.0));
				float m = hsv.z - c;

				float3 rgb;
				if (0.0 <= hsv.x && hsv.x < 60.0) rgb = float3(c, x, 0.0);
				else if (hsv.x < 120.0) rgb = float3(x, c, 0.0);
				else if (hsv.x < 180.0) rgb = float3(0.0, c, x);
				else if (hsv.x < 240.0) rgb = float3(0.0, x, c);
				else if (hsv.x < 300.0) rgb = float3(x, 0.0, c);
				else if (hsv.x < 360.0) rgb = float3(c, 0.0, x);

				rgb.r += m;
				rgb.g += m;
				rgb.b += m;

				return rgb;
			}

			//Vertex Variables
			sampler2D _RenderTex;
			float4 _RenderTex_ST;

			float4 _paramColorMod0;
			float4 _paramColorMod1;
			float4 _paramColorMod2;
			float4 _paramColorMod3;

			float _controllerHeldCDTimer;
			Vector _lControllerPos;
			float _controllerHeldCDTimerB;
			Vector _lControllerPosB;
			float _controllerHeldCDTimerC;
			Vector _lControllerPosC;
			float _controllerHeldCDTimerD;
			Vector _lControllerPosD;

			Vector _vertBounds;

			float _wavStickTimeMod;
			float _wavStickTimeRange;
			float _wavStickTimePower;
			float _paramCutoff;

			//Vertex Function
			VertexOutput vert(VertexInput v) 
			{
				//Var Init
				VertexOutput o;
				o.oldPos = v.v;

				//Bounds Checking
				float dThresh = 10;
				if((v.v.x < _vertBounds.z && v.v.x > _vertBounds.x)&&(v.v.z < _vertBounds.w && v.v.z > _vertBounds.y))
				{
	        		float tX = (v.v.x - _vertBounds.x) / (_vertBounds.z - _vertBounds.x);
	        		float tZ = (v.v.z - _vertBounds.y) / (_vertBounds.w - _vertBounds.y);
	        		float4 tPos = float4(tex2Dlod(_RenderTex, float4(tX, tZ, 0, 0)).xyz, 1);

	        		if(abs(tPos.x - v.v.x) < dThresh && abs(tPos.z - v.v.z) < dThresh) 
						v.v = float4(tPos.x, v.v.y + tPos.y, tPos.z, 1);
	        		else 
						v.v = float4(v.v.x, v.v.y + tPos.y, v.v.z, 1);
				}

				//Wave Control
				float clampedRand = clamp(rand(float2(o.oldPos.x, o.oldPos.z)), 0.2, 1.0);
				//float wavStickTimeModifier = _wavStickTimeMod + rand(float2(o.oldPos.x, o.oldPos.z)) * (_wavStickTimeRange);
				float wavStickTimeModifier = _wavStickTimeMod + clampedRand * (_wavStickTimeRange);
				float wavStickTimePower = _wavStickTimePower;
				float paramCheckCutoff = _paramCutoff;
				float wavMod = 2.0;
				float wavCutOff = 30.0;
				float oldY = v.v.y;
				float wavIntMod = 0.25;

				int paramBits = 0;

				for (int i = 0; i < 8; i++)
				{
					float3 wavPos = wavPosBuff[i];
					float wavRad = wavRadBuff[i];
					float wavInt = wavIntBuff[i];
					float wavParam = wavParamBuff[i];

					if (wavRad > 0)
					{
						float dist = sqrt(pow(v.v.x - wavPos.x, 2) + pow(v.v.z - wavPos.z, 2));

						float temp = max(abs(dist - wavRad), 0.0000001);
						float posi = max(abs(dist), 0.0000001);

						if (posi > wavRad)
						{
							if (temp < wavCutOff)
							{
								v.v.y += (wavInt * wavIntMod) * sin(wavMod * temp) / (wavMod * temp);
							}
						}
						else
						{
							int paramBool = 0;
							int tempBits = paramBits;

							switch (wavParam)
							{
							case 0: //RMS < 0.15
								if (denormalizeToRange(v.uvB.x, 0.0, 0.6715103569670999) < 0.15)
									paramBool = 1;
								tempBits |= 0x8;
								break;
							case 1: //SNR > 12
								if (denormalizeToRange(v.uv.w, 0.0, 26.61842977741538) > 12.0)
									paramBool = 1;
								tempBits |= 0x4;
								break;
							case 2: //Highest Teff in the Grid //SUB IN AstroColor
								if (denormalizeToRange(v.uv.y, 1.231199705120561, 1.8991459147993115) < 1.5)
									paramBool = 1;
								tempBits |= 0x2;
								break;
							case 3: //Variable Class 3 tier scores
								if (denormalizeToRange(v.uvB.y, 0.0, 1.0) == 1.0) //Possibly implemented poorly in Mesh
									paramBool = 1;
								tempBits |= 0x1;
								break;
							}

							if (paramBool == 1 /*&& (v.v.y - oldY < (wavInt * wavIntMod) || i == 0)*/)
							{
								float yMod = (wavInt * wavIntMod) * min(max((1.0 - pow(wavRad - posi, wavStickTimePower) * wavStickTimeModifier), 0.0), 1.0);
								v.v.y += yMod;

								if (v.v.y > 0.5f && yMod > 0.0) paramBits = tempBits;
							}
							else if (temp < wavCutOff)
								v.v.y += (wavInt * wavIntMod) * sin(wavMod * temp) / (wavMod * temp);
						}
					}
				}

				//???
				float aDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPos.x)),  2) + pow(abs(v.v.z - (_lControllerPos.z)),  2))), 0.5) * 1.0;
				float bDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosB.x)), 2) + pow(abs(v.v.z - (_lControllerPosB.z)), 2))), 0.5) * 1.0;
				float cDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosC.x)), 2) + pow(abs(v.v.z - (_lControllerPosC.z)), 2))), 0.5) * 1.0;
				float dDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosD.x)), 2) + pow(abs(v.v.z - (_lControllerPosD.z)), 2))), 0.5) * 1.0;

				if (_controllerHeldCDTimer  < 0) _controllerHeldCDTimer  = 0.0;
				if (_controllerHeldCDTimerB < 0) _controllerHeldCDTimerB = 0.0;
				if (_controllerHeldCDTimerC < 0) _controllerHeldCDTimerC = 0.0;
				if (_controllerHeldCDTimerD < 0) _controllerHeldCDTimerD = 0.0;

           		aDist = (_lControllerPos.y  - v.v.y) - aDist;
           		bDist = (_lControllerPosB.y - v.v.y) - bDist;
           		cDist = (_lControllerPosC.y - v.v.y) - cDist;
           		dDist = (_lControllerPosD.y - v.v.y) - dDist;

           		aDist = aDist * _controllerHeldCDTimer;
           		bDist = bDist * _controllerHeldCDTimerB;
           		cDist = cDist * _controllerHeldCDTimerC;
           		dDist = dDist * _controllerHeldCDTimerD;

           		if (aDist < 0) aDist = 0.0;
           		if (bDist < 0) bDist = 0.0;
           		if (cDist < 0) cDist = 0.0;
           		if (dDist < 0) dDist = 0.0;

				float aRatio = 0.0;
				float bRatio = 0.0;
				float cRatio = 0.0;
				float dRatio = 0.0;

				if((aDist + bDist + cDist + dDist) != 0)
				{
            		aRatio = aDist / (aDist + bDist + cDist + dDist);
            		bRatio = bDist / (aDist + bDist + cDist + dDist);
            		cRatio = cDist / (aDist + bDist + cDist + dDist);
            		dRatio = dDist / (aDist + bDist + cDist + dDist);
				}
				else
				{
            		aRatio = 0.0;
            		bRatio = 0.0;
            		cRatio = 0.0;
            		dRatio = 0.0;
				}

				v.v.y = max(v.v.y, v.v.y + (pow(abs(aRatio), 0.75) * aDist) + (pow(abs(bRatio), 0.75) * bDist) + (pow(abs(cRatio), 0.75) * cDist) + (pow(abs(dRatio), 0.75) * dDist));

				//Format Output
				o.pos = v.v;
				o.col.rgb = v.color.rgb;
				o.col.a = v.color.a;
				o.uv = v.uv;
				o.uvB = v.uvB;

				//Wave Param Coloring
				float colorNum = 0;
				float3 colorMod[4];

				if (paramBits & 0x8) colorMod[colorNum++] = RGB2HSV(_paramColorMod0);
				if (paramBits & 0x4) colorMod[colorNum++] = RGB2HSV(_paramColorMod1);
				if (paramBits & 0x2) colorMod[colorNum++] = RGB2HSV(_paramColorMod2);
				if (paramBits & 0x1) colorMod[colorNum++] = RGB2HSV(_paramColorMod3);

				float3 totalVals = float3(0, 0, 0);
				for (int i = 0; i < colorNum; i++) totalVals += colorMod[i];

				float3 newColorMod;
				newColorMod.x = (colorNum == 0) ? 0.0 : (totalVals.x / colorNum) % 360.0;
				newColorMod.y = (colorNum == 0) ? 0.0 : min((totalVals.y / colorNum) * 0.5 * max(1.0, colorNum * 0.75), 1.0);
				newColorMod.z = (colorNum == 0) ? 0.0 : min((totalVals.z / colorNum) * 0.5, 1.0);

				o.colMod.rgb = HSV2RGB(newColorMod);
				o.colMod.a = (v.uvB.w == 0.5) ? 0.5 : 0.0;

				return o;
			}

			//Geometry Variables
			Vector _baseColorValMinRange;
			Vector _rotSpeedMinRange;
			Vector _baseSizeMinRange;
			Vector _extensionOscFreqMinRange;
			Vector _extensionOscRangeMinRange;
			Vector _baseColorSatMinRange;
			Vector _extensionScalarMinRange;
			Vector _oscSizeMinRange;
			Vector _oscSizeFreqMinRange;

			//Geometry Function
       		[maxvertexcount(4)]
			void geom (point VertexOutput input[1], inout LineStream<g2f> output)
			{
				float baseColorVal      = (input[0].col.r * (_baseColorValMinRange.y)) + _baseColorValMinRange.x - _baseColorValMinRange.y * 0.5f;
				float vertexColorPal1   = input[0].col.g;
				float rotSpeed          = (input[0].col.b * (_rotSpeedMinRange.y)) + _rotSpeedMinRange.x - _rotSpeedMinRange.y * 0.5f;
				float baseSize          = (input[0].col.a * (_baseSizeMinRange.y)) + _baseSizeMinRange.x - _baseSizeMinRange.y * 0.5f;
				float vertexColorPal2   = input[0].uv.x;
				float baseColorHue      = input[0].uv.y;
				float extensionOscFreq  =  (input[0].uv.z * (_extensionOscFreqMinRange.y)) + _extensionOscFreqMinRange.x - _extensionOscFreqMinRange.y * 0.5f;
				float extensionOscRange = ((input[0].uv.w * (_extensionOscRangeMinRange.y)) + _extensionOscRangeMinRange.x - _extensionOscRangeMinRange.y * 0.5f)* baseSize;
				float baseColorSat      = (input[0].uvB.x * (_baseColorSatMinRange.y)) + _baseColorSatMinRange.x - _baseColorSatMinRange.y * 0.5f;
				float extensionScalar   = (input[0].uvB.y * (_extensionScalarMinRange.y)) + _extensionScalarMinRange.x - _extensionScalarMinRange.y * 0.5f;
				float oscSize           = ((input[0].uvB.z * (_oscSizeMinRange.y)) + _oscSizeMinRange.x - _oscSizeMinRange.y * 0.5f) * baseSize ;
				float oscSizeFreq		=  (rand(float2(baseColorSat, baseColorHue)) * (_oscSizeFreqMinRange.y)) + _oscSizeFreqMinRange.x - _oscSizeFreqMinRange.y * 0.5f;
				float4 mIn              = mul(UNITY_MATRIX_MV, input[0].pos);

				float f = 4.0 + baseSize * 4.0 + (oscSize * (sin(_Time[1] * oscSizeFreq + rand(float2(extensionScalar, baseColorHue))* 1.5)/2.+.5)) * rand(float2(extensionOscFreq, vertexColorPal1));
				f /= 200.0;

				float4 vc[3] = { float4( -f / 0.866, -f, 0.0f, 0.0f ),
								 float4( 0.0f,       +f, 0.0f, 0.0f ),
								 float4( +f / 0.866, -f, 0.0f, 0.0f ) };

				for (int i = 0; i < 3; i++)
					vc[i] = clamp(vc[i], float4(1, 1, 1, 1) * -0.05f, float4(1, 1, 1, 1) * 0.05f);

				const int TRI_STRIP_SQUARE[4] = {0, 1, 2, 0};

				float texOffset = 1.0;

				const float UVX[4] = {0.5 - 0.5 * texOffset,   0.5, 0.5 + 0.5 * texOffset, 0.5 - 0.5 * texOffset};
				const float UVY[4] = {0.5 - 0.433 * texOffset, 0.5 + 0.433 * texOffset, 0.5 - 0.433 * texOffset, 0.5 - 0.433 * texOffset};

				g2f o[4];

				float tDriver = ((float)((int)((rand(input[0].oldPos.xz)) / 0.0625)) + 0.1) * 0.0625;
				float4 outColor  = float4(HSVtoRGB(float3(baseColorHue,    baseColorSat, baseColorVal)), 1.0);
				float4 colorPal1 = float4(HSVtoRGB(float3(vertexColorPal1, baseColorSat, baseColorVal)), 1.0);
				float4 colorPal2 = float4(HSVtoRGB(float3(vertexColorPal2, baseColorSat, baseColorVal)), 1.0);

				outColor = 0.5 * outColor + 0.5 * (colorPal1*((float)6)/12.0 + colorPal2*((float)(12-6))/12.0);

				for(int i = 0; i < 4; i++)
				{
					o[i].uv = float2(0.5, 0.5);
					o[i].col = outColor;
					o[i].pos = mIn + vc[TRI_STRIP_SQUARE[i]];
					o[i].colMod = input[0].colMod;
					float2 co = (0, 1);
					float r = rand(co);
					o[i].pos = mul(mul(UNITY_MATRIX_P, r), o[i].pos);

					UNITY_TRANSFER_FOG(o[i], o[i].pos);
					o[i].uv = float2(UVX[i], UVY[i]);
					o[i].col.a = tDriver;

					output.Append(o[i]);
				}

				output.RestartStrip();
			}
         
			//Fragmentation Function
			float4 frag(g2f i) : SV_Target 
			{
        		float4 col;
				col.rgb = pow(i.col.rgb, 0.3);
				col.rgb += i.colMod.rgb;

				float4 fogCol = float4(1, 1, 1, 1);
				UNITY_APPLY_FOG(i.fogCoord, fogCol);

				col.a = fogCol.a * 1.0;

				return col;
			}

			ENDCG
        }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma multi_compile_fog
			#pragma target 5.0

			#include "UnityCG.cginc"

			uniform StructuredBuffer<float3> wavPosBuff : register(t5);
			uniform StructuredBuffer<float> wavRadBuff : register(t6);
			uniform StructuredBuffer<float> wavIntBuff : register(t7);
			uniform StructuredBuffer<int> wavParamBuff : register(t8);

			//IO Structs

			struct VertexInput
			{
				float4 v : POSITION;
				float4 color: COLOR;
				float4 uv : TEXCOORD0;
				float4 uvB : TEXCOORD1;
			};

			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR;
				float4 uv : TEXCOORD0;
				float4 uvB : TEXCOORD1;
				float4 oldPos : TEXCOORD2;

				float4 colMod : TEXCOORD3;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR;
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(2)

				float4 colMod : TEXCOORD3;
			};

			///User Functions
			//Creates a random float value given a float2
			//Source: https://answers.unity.com/questions/399751/randomity-in-cg-shaders-beginner.html
			float rand(float2 co)
			{
				return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
			}

			float denormalizeToRange(float val, float min, float max)
			{
				return (val * (max - min)) + min;
			}

			//Converts HSV values to RGB values
			//Hue, Saturation, Value
			//Red, Green, Blue
			//Formula Source: https://www.rapidtables.com/convert/color/hsv-to-rgb.html
			float3 HSVtoRGB(float3 HSV)
			{
				float3 RGB;
				RGB.x = abs(HSV.x * 6 - 3) - 1;
				RGB.y = 2 - abs(HSV.x * 6 - 2);
				RGB.z = 2 - abs(HSV.x * 6 - 4);
				RGB = saturate(RGB);

				return ((RGB - 1) * HSV.y + 1) * HSV.z;
			}

			//Source: https://www.rapidtables.com/convert/color/rgb-to-hsv.html
			float3 RGB2HSV(float3 rgb)
			{
				float cMax = max(max(rgb.r, rgb.g), rgb.b);
				float cMin = min(min(rgb.r, rgb.g), rgb.b);
				float delta = cMax - cMin;

				float hue;

				if (delta == 0.0) hue = 0.0;
				else if (cMax == rgb.r) hue = (((rgb.g - rgb.b) / delta) + 6.0) % 6.0;
				else if (cMax == rgb.g) hue = (rgb.b - rgb.r) / delta + 2.0;
				else if (cMax == rgb.b) hue = (rgb.r - rgb.g) / delta + 4.0;
				hue *= 60.0;

				float sat = (cMax == 0.0) ? 0.0 : delta / cMax;

				return float3(hue, sat, cMax);
			}

			//Source: https://www.rapidtables.com/convert/color/hsv-to-rgb.html
			float3 HSV2RGB(float3 hsv)
			{
				float c = hsv.z * hsv.y;
				float x = c * (1.0 - abs((hsv.x / 60.0) % 2.0 - 1.0));
				float m = hsv.z - c;

				float3 rgb;
				if (0.0 <= hsv.x && hsv.x < 60.0) rgb = float3(c, x, 0.0);
				else if (hsv.x < 120.0) rgb = float3(x, c, 0.0);
				else if (hsv.x < 180.0) rgb = float3(0.0, c, x);
				else if (hsv.x < 240.0) rgb = float3(0.0, x, c);
				else if (hsv.x < 300.0) rgb = float3(x, 0.0, c);
				else if (hsv.x < 360.0) rgb = float3(c, 0.0, x);

				rgb.r += m;
				rgb.g += m;
				rgb.b += m;

				return rgb;
			}

			//Vertex Variables
			sampler2D _RenderTex;
			float4 _RenderTex_ST;

			float4 _paramColorMod0;
			float4 _paramColorMod1;
			float4 _paramColorMod2;
			float4 _paramColorMod3;

			float _controllerHeldCDTimer;
			Vector _lControllerPos;
			float _controllerHeldCDTimerB;
			Vector _lControllerPosB;
			float _controllerHeldCDTimerC;
			Vector _lControllerPosC;
			float _controllerHeldCDTimerD;
			Vector _lControllerPosD;

			Vector _vertBounds;

			float _wavStickTimeMod;
			float _wavStickTimeRange;
			float _wavStickTimePower;
			float _paramCutoff;

			//Vertex Function
			VertexOutput vert(VertexInput v)
			{
				//Var Init
				VertexOutput o;
				o.oldPos = v.v;
				
				//Bounds Checking
				float dThresh = 10;
				if ((v.v.x < _vertBounds.z && v.v.x > _vertBounds.x) && (v.v.z < _vertBounds.w && v.v.z > _vertBounds.y))
				{
					float tX = (v.v.x - _vertBounds.x) / (_vertBounds.z - _vertBounds.x);
					float tZ = (v.v.z - _vertBounds.y) / (_vertBounds.w - _vertBounds.y);
					float4 tPos = float4(tex2Dlod(_RenderTex, float4(tX, tZ, 0, 0)).xyz, 1);

					if (abs(tPos.x - v.v.x) < dThresh && abs(tPos.z - v.v.z) < dThresh)
						v.v = float4(tPos.x, v.v.y + tPos.y, tPos.z, 1);
					else
						v.v = float4(v.v.x, v.v.y + tPos.y, v.v.z, 1);
				}

				//Wave Control
				float clampedRand = clamp(rand(float2(o.oldPos.x, o.oldPos.z)), 0.2, 1.0);
				//float wavStickTimeModifier = _wavStickTimeMod + rand(float2(o.oldPos.x, o.oldPos.z)) * (_wavStickTimeRange);
				float wavStickTimeModifier = _wavStickTimeMod + clampedRand * (_wavStickTimeRange);
				float wavStickTimePower = _wavStickTimePower;
				float paramCheckCutoff = _paramCutoff;
				float wavMod = 2.0;
				float wavCutOff = 30.0;
				float oldY = v.v.y;
				float wavIntMod = 0.25;

				int paramBits = 0;

				for (int i = 0; i < 8; i++)
				{
					float3 wavPos = wavPosBuff[i];
					float wavRad = wavRadBuff[i];
					float wavInt = wavIntBuff[i];
					float wavParam = wavParamBuff[i];

					if (wavRad > 0)
					{
						float dist = sqrt(pow(v.v.x - wavPos.x, 2) + pow(v.v.z - wavPos.z, 2));

						float temp = max(abs(dist - wavRad), 0.0000001);
						float posi = max(abs(dist), 0.0000001);

						if (posi > wavRad)
						{
							if (temp < wavCutOff)
							{
								v.v.y += (wavInt * wavIntMod) * sin(wavMod * temp) / (wavMod * temp);
							}
						}
						else
						{
							int paramBool = 0;
							int tempBits = paramBits;

							switch (wavParam)
							{
							case 0: //RMS < 0.15
								if (denormalizeToRange(v.uvB.x, 0.0, 0.6715103569670999) < 0.15)
									paramBool = 1;
								tempBits |= 0x8;
								break;
							case 1: //SNR > 12
								if (denormalizeToRange(v.uv.w, 0.0, 26.61842977741538) > 12.0)
									paramBool = 1;
								tempBits |= 0x4;
								break;
							case 2: //Highest Teff in the Grid //SUB IN AstroColor
								if (denormalizeToRange(v.uv.y, 1.231199705120561, 1.8991459147993115) < 1.5)
									paramBool = 1;
								tempBits |= 0x2;
								break;
							case 3: //Variable Class 3 tier scores
								if (denormalizeToRange(v.uvB.y, 0.0, 1.0) == 1.0) //Possibly implemented poorly in Mesh
									paramBool = 1;
								tempBits |= 0x1;
								break;
							}

							if (paramBool == 1 /*&& (v.v.y - oldY < (wavInt * wavIntMod) || i == 0)*/)
							{
								float yMod = (wavInt * wavIntMod) * min(max((1.0 - pow(wavRad - posi, wavStickTimePower) * wavStickTimeModifier), 0.0), 1.0);
								v.v.y += yMod;

								if (v.v.y > 0.5f && yMod > 0.0) paramBits = tempBits;
							}
							else if (temp < wavCutOff)
								v.v.y += (wavInt * wavIntMod) * sin(wavMod * temp) / (wavMod * temp);
						}
					}
				}

				//???
				float aDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPos.x)),  2) + pow(abs(v.v.z - (_lControllerPos.z)),  2))), 0.5) * 1.0;
				float bDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosB.x)), 2) + pow(abs(v.v.z - (_lControllerPosB.z)), 2))), 0.5) * 1.0;
				float cDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosC.x)), 2) + pow(abs(v.v.z - (_lControllerPosC.z)), 2))), 0.5) * 1.0;
				float dDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosD.x)), 2) + pow(abs(v.v.z - (_lControllerPosD.z)), 2))), 0.5) * 1.0;

				if (_controllerHeldCDTimer < 0) _controllerHeldCDTimer = 0.0;
				if (_controllerHeldCDTimerB < 0) _controllerHeldCDTimerB = 0.0;
				if (_controllerHeldCDTimerC < 0) _controllerHeldCDTimerC = 0.0;
				if (_controllerHeldCDTimerD < 0) _controllerHeldCDTimerD = 0.0;

				aDist = (_lControllerPos.y - v.v.y) - aDist;
				bDist = (_lControllerPosB.y - v.v.y) - bDist;
				cDist = (_lControllerPosC.y - v.v.y) - cDist;
				dDist = (_lControllerPosD.y - v.v.y) - dDist;

				aDist = aDist * _controllerHeldCDTimer;
				bDist = bDist * _controllerHeldCDTimerB;
				cDist = cDist * _controllerHeldCDTimerC;
				dDist = dDist * _controllerHeldCDTimerD;

				if (aDist < 0) aDist = 0.0;
				if (bDist < 0) bDist = 0.0;
				if (cDist < 0) cDist = 0.0;
				if (dDist < 0) dDist = 0.0;

				float aRatio = 0.0;
				float bRatio = 0.0;
				float cRatio = 0.0;
				float dRatio = 0.0;

				if ((aDist + bDist + cDist + dDist) != 0)
				{
					aRatio = aDist / (aDist + bDist + cDist + dDist);
					bRatio = bDist / (aDist + bDist + cDist + dDist);
					cRatio = cDist / (aDist + bDist + cDist + dDist);
					dRatio = dDist / (aDist + bDist + cDist + dDist);
				}
				else
				{
					aRatio = 0.0;
					bRatio = 0.0;
					cRatio = 0.0;
					dRatio = 0.0;
				}

				v.v.y = max(v.v.y, v.v.y + (pow(abs(aRatio), 0.75) * aDist) + (pow(abs(bRatio), 0.75) * bDist) + (pow(abs(cRatio), 0.75) * cDist) + (pow(abs(dRatio), 0.75) * dDist));

				//Format Output
				o.pos = v.v;
				o.col.rgb = v.color.rgb;
				o.col.a = v.color.a;
				o.uv = v.uv;
				o.uvB = v.uvB;

				//Wave Param Coloring
				float colorNum = 0;
				float3 colorMod[4];

				if (paramBits & 0x8) colorMod[colorNum++] = RGB2HSV(_paramColorMod0);
				if (paramBits & 0x4) colorMod[colorNum++] = RGB2HSV(_paramColorMod1);
				if (paramBits & 0x2) colorMod[colorNum++] = RGB2HSV(_paramColorMod2);
				if (paramBits & 0x1) colorMod[colorNum++] = RGB2HSV(_paramColorMod3);

				float3 totalVals = float3(0, 0, 0);
				for (int i = 0; i < colorNum; i++) totalVals += colorMod[i];

				float3 newColorMod;
				newColorMod.x = (colorNum == 0) ? 0.0 : (totalVals.x / colorNum) % 360.0;
				newColorMod.y = (colorNum == 0) ? 0.0 : min((totalVals.y / colorNum) * 0.5 * max(1.0, colorNum * 0.75), 1.0);
				newColorMod.z = (colorNum == 0) ? 0.0 : min((totalVals.z / colorNum) * 0.5, 1.0);

				o.colMod.rgb = HSV2RGB(newColorMod);
				o.colMod.a = (v.uvB.w == 0.5) ? 0.5 : 0.0;

				return o;
			}

			//Geometry Variables
			Vector _baseColorValMinRange;
			Vector _rotSpeedMinRange;
			Vector _baseSizeMinRange;
			Vector _extensionOscFreqMinRange;
			Vector _extensionOscRangeMinRange;
			Vector _baseColorSatMinRange;
			Vector _extensionScalarMinRange;
			Vector _oscSizeMinRange;
			Vector _oscSizeFreqMinRange;

			//Geometry Function
			[maxvertexcount(4)]
			void geom(point VertexOutput input[1], inout TriangleStream<g2f> output)
			{
				float baseColorVal = (input[0].col.r * (_baseColorValMinRange.y)) + _baseColorValMinRange.x - _baseColorValMinRange.y * 0.5f;
				//float baseColorVal = 0;
				float vertexColorPal1 = input[0].col.g;
				float rotSpeed = (input[0].col.b * (_rotSpeedMinRange.y)) + _rotSpeedMinRange.x - _rotSpeedMinRange.y * 0.5f;
				float baseSize = ((input[0].col.a * (_baseSizeMinRange.y)) + _baseSizeMinRange.x - _baseSizeMinRange.y * 0.5f) * .974f;
				float vertexColorPal2 = input[0].uv.x;
				float baseColorHue = input[0].uv.y;
				float extensionOscFreq =  (input[0].uv.z * (_extensionOscFreqMinRange.y)) + _extensionOscFreqMinRange.x - _extensionOscFreqMinRange.y * 0.5f;
				float extensionOscRange = ((input[0].uv.w * (_extensionOscRangeMinRange.y)) + _extensionOscRangeMinRange.x - _extensionOscRangeMinRange.y * 0.5f) * baseSize;
				float baseColorSat = (input[0].uvB.x * (_baseColorSatMinRange.y)) + _baseColorSatMinRange.x - _baseColorSatMinRange.y * 0.5f;
				float extensionScalar = (input[0].uvB.y * (_extensionScalarMinRange.y)) + _extensionScalarMinRange.x - _extensionScalarMinRange.y * 0.5f;
				float oscSize = ((input[0].uvB.z * (_oscSizeMinRange.y)) + _oscSizeMinRange.x - _oscSizeMinRange.y * 0.5f) * baseSize;
				float oscSizeFreq =  (rand(float2(baseColorSat, baseColorHue)) * (_oscSizeFreqMinRange.y)) + _oscSizeFreqMinRange.x - _oscSizeFreqMinRange.y *0.5f;
				float4 mIn = mul(UNITY_MATRIX_MV, input[0].pos);

				float f = 4.0 + baseSize * 4.0 + (oscSize * (sin(_Time[1] * oscSizeFreq + rand(float2(extensionScalar, baseColorHue)) * 1.5) / 2. + .5)) * rand(float2(extensionOscFreq, vertexColorPal1));
				f /= 200.0;

				float4 vc[3] = { float4(-f / 0.866, -f, 0.0f, 0.0f) ,
								 float4(0.0f,       +f, 0.0f, 0.0f) ,
								 float4(+f / 0.866, -f, 0.0f, 0.0f)  };

				for (int i = 0; i < 3; i++)
					vc[i] = clamp(vc[i], float4(1, 1, 1, 1) * -0.05f, float4(1, 1, 1, 1) * 0.05f);

				const int TRI_STRIP_SQUARE[4] = {0, 1, 2, 0} ;

				float texOffset = 1.0;

				const float UVX[4] = {0.5 - 0.5 * texOffset,   0.5, 0.5 + 0.5 * texOffset, 0.5 - 0.5 * texOffset};
				const float UVY[4] = {0.5 - 0.433 * texOffset, 0.5 + 0.433 * texOffset, 0.5 - 0.433 * texOffset, 0.5 - 0.433 * texOffset};

				g2f o[4];

				float tDriver = ((float)((int)((rand(input[0].oldPos.xz)) / 0.0625)) + 0.1) * 0.0625;
				float4 outColor = float4(HSVtoRGB(float3(baseColorHue,    baseColorSat, baseColorVal)), 1.0);
				float4 colorPal1 = float4(HSVtoRGB(float3(vertexColorPal1, baseColorSat, baseColorVal)), 1.0);
				float4 colorPal2 = float4(HSVtoRGB(float3(vertexColorPal2, baseColorSat, baseColorVal)), 1.0);

				outColor = 0.5 * outColor + 0.5 * (colorPal1 * ((float)6) / 12.0 + colorPal2 * ((float)(12 - 6)) / 12.0);

				for (int i = 0; i < 4; i++)
				{
					o[i].uv = float2(0.5, 0.5);
					o[i].col = outColor;
					o[i].pos = mIn + vc[TRI_STRIP_SQUARE[i]];
					o[i].colMod = input[0].colMod;
					float2 co = (0, 1);
					float r = rand(co);
					o[i].pos = mul(mul(UNITY_MATRIX_P, r), o[i].pos);

					UNITY_TRANSFER_FOG(o[i], o[i].pos);
					o[i].uv = float2(UVX[i], UVY[i]);
					o[i].col.a = .7f;// tDriver;

					output.Append(o[i]);
				}

				output.RestartStrip();
			}

			//Fragmentation Function
			float4 frag(g2f i) : SV_Target
			{
				float4 fogCol = float4(1, 1, 1, 1);
				UNITY_APPLY_FOG(i.fogCoord, fogCol);

				float4 col;
				col = float4(0, 0, 0, .985f);

				return col;
			}

			ENDCG
		}
    }
 
}