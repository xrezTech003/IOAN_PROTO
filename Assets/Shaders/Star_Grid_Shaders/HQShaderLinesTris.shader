Shader "Custom/HQStarShaderFilledTriangles"
{
	//      FOR REFERENCE
	//      pos [i].x = ra
	//      pos [i].y = parallax
	//      pos [i].z = dec
	//      colors [i].r = meanMag;
	//		colors [i].g = magSTD;
	//		colors [i].b = numObs;
	//		colors [i].a = variability;
	//		newUV [i].x = catalog;
	//		newUV [i].y = astroColor;
	//		newUV [i].z = period;
	//		newUV [i].w = periodSNR;
	//		newUV2 [i].x = lightcurveRMS;
	//		newUV2 [i].y = varClass;
	//		newUV2 [i].z = properMotion;
	//		newUV2 [i].w = parsecs;
	//		newVertices [i].y = (parsecs * 2.0f) - 1.0f;
	//		indices [i] = i;

	Properties
	{
		//Vertex Properties
		_bodyCloseScalarPower("bodyCloseScalarPower",Vector) = (1,0,0)

		_wavStickTimeMod("wavStickTimeMod",float) = 0
		_wavStickTimeRange("wavStickTimeRange",float) = 0
		_wavStickTimePower("wavStickTimePower",float) = 0
		_paramCutoff("paramCutoff",float) = 0.8
		_RenderTex("Color (RGB) Alpha (A)", 2D) = "white" {}

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
		_wavIntMod("wavIntMid",float) = 0.25

		//Geometry Properties
		_baseColorValMinRange("baseColorValMinRange", Vector) = (1, 0, 0)
		_rotSpeedMinRange("rotSpeedMinRange", Vector) = (1, 0, 0)
		_baseSizeMinRange("baseSizeMinRange", Vector) = (1, 0, 0)
		_extensionOscFreqMinRange("extensionOscFreqMinRange", Vector) = (1, 0, 0)
		_extensionOscRangeMinRange("extensionOscRangeMinRange", Vector) = (1, 0, 0)
		_baseColorSatMinRange("baseColorSatMinRange", Vector) = (1, 0, 0)
		_extensionScalarMinRange("extensionScalarMinRange", Vector) = (1, 0, 0)
		_oscSizeMinRange("oscSizeMinRange", Vector) = (1, 0, 0)
		_oscSizeFreqMinRange("oscSizeFreqMinRange", Vector) = (1, 0, 0)
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"DisableBatching" = "True"
			"StarField" = "True"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Zwrite On

		Pass //lines
		{
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

			uniform StructuredBuffer<float2> highlightPosBuff : register(t1);
			uniform StructuredBuffer<float> highlightValBuff : register(t2);
			uniform StructuredBuffer<float2> obscurePosBuff : register(t3);
			uniform StructuredBuffer<float3> bodyPosBuff : register(t4); //Probably Deprecated

			uniform StructuredBuffer<float3> wavPosBuff : register(t5);
			uniform StructuredBuffer<float> wavRadBuff : register(t6);
			uniform StructuredBuffer<float> wavIntBuff : register(t7);
			uniform StructuredBuffer<int> wavParamBuff : register(t8);

			//User I/O Structs
			struct VertexInput
			{
				float4 v : POSITION;
				float4 color: COLOR0;
				float4 uv : TEXCOORD0;
				float4 uvB : TEXCOORD1;
				float3 nrm : NORMAL;
			};

			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR;
				float4 uv : TEXCOORD0;
				float4 uvB : TEXCOORD1;
				float4 oldPos : TEXCOORD2;
				float3 nrm : NORMAL;

				float4 colMod : TEXCOORD3;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR;
				float3 nrm : NORMAL;
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

			//Converts HSV values to RGB values
			//Hue, Saturation, Value
			//Red, Green, Blue
			float3 HSVtoRGB(float3 HSV)
			{
				float3 RGB;
				RGB.x = abs(HSV.x * 6 - 3) - 1;
				RGB.y = 2 - abs(HSV.x * 6 - 2);
				RGB.z = 2 - abs(HSV.x * 6 - 4);
				RGB = saturate(RGB);

				return ((RGB - 1) * HSV.y + 1) * HSV.z;
			}

			float denormalizeToRange(float val, float min, float max)
			{
				return (val * (max - min)) + min;
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
			//Source: https://shadowmint.gitbooks.io/unity-material-shaders/content/support/syntax/lerp.html
			float lerpH(float a, float b, float w, float m) {
				float r =(1 - (w / m));
				return b - (r * (b - a));
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

			//Clamp all values in the input float4 between [0, 1]
			float4 clamper(float4 inData)
			{
				inData.x = clamp(inData.x, 0.0, 1.0);
				inData.y = clamp(inData.y, 0.0, 1.0);
				inData.z = clamp(inData.z, 0.0, 1.0);
				inData.w = clamp(inData.w, 0.0, 1.0);

				return inData;
			}
			
			//Vertex Variables
			Vector _vertBounds;

			sampler2D _RenderTex;
			float4 _RenderTex_ST;

			float _wavStickTimeMod;
			float _wavStickTimeRange;
			float _wavStickTimePower;
			float _paramCutoff;
			float _wavIntMod;

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

			Vector _bodyCloseScalarPower;

			//Vertex Function
			VertexOutput vert(VertexInput v)
			{
				//Var Init
				VertexOutput o;
				
				o.nrm = v.nrm;
				o.oldPos = v.v;
				
				v.uvB.w = 0.0;

				for (int i = 0; i < 60; i++)
					if (v.nrm.x == highlightPosBuff[i].x && v.nrm.y == highlightPosBuff[i].y) v.uvB.w = highlightValBuff[i];

				for (int i = 0; i < 60; i++)
					if (v.nrm.x == obscurePosBuff[i].x && v.nrm.y == obscurePosBuff[i].y) v.uvB.w = 1.0;

				//Bounds Checking
				if ((v.v.x < _vertBounds.z) && (v.v.x > _vertBounds.x) && (v.v.z < _vertBounds.w) && (v.v.z > _vertBounds.y))
				{
					float tX = (v.v.x - _vertBounds.x) / (_vertBounds.z - _vertBounds.x);
					float tZ = (v.v.z - _vertBounds.y) / (_vertBounds.w - _vertBounds.y);
					float4 tPos = float4(tex2Dlod(_RenderTex, float4(tX, tZ, 0, 0)).xyz, 1);

					v.v = float4(tPos.x, v.v.y + tPos.y, tPos.z, 1);
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

				//Attempt for modifying star color based on player location
				/*
				o.nrm.x = 0.0;

				float totBodyDist;
				for (int i = 0; i < 6; i++)
				{
					if (bodyPosBuff[i].z != 9999)
					{
						totBodyDist = sqrt(pow((bodyPosBuff[i].x - v.v.x), 2) + pow((bodyPosBuff[i].z - v.v.z), 2));
						o.nrm.x += max(0.0, 1.0 - pow(totBodyDist, _bodyCloseScalarPower.y) * _bodyCloseScalarPower.x);
					}
				}
				*/

				//Load Output
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

			//Geometry Functions
			[maxvertexcount(60)]
			void geom(point VertexOutput input[1], inout LineStream<g2f> output)
			{
				//Set Mod Values
				input[0].col = clamper(input[0].col);
				input[0].uv = clamper(input[0].uv);
				input[0].uvB = clamper(input[0].uvB);

				float baseColorVal = (input[0].col.r * _baseColorValMinRange.y) + _baseColorValMinRange.x - (_baseColorValMinRange.y * 0.5f);
				float vertexColorPal1 = (input[0].col.g);
				float baseSize = (input[0].col.a * _baseSizeMinRange.y) + _baseSizeMinRange.x - (_baseSizeMinRange.y * 0.5f);

				float vertexColorPal2 = (input[0].uv.x);
				float baseColorHue = (input[0].uv.y);
				float extensionOscFreq = (input[0].uv.z * _extensionOscFreqMinRange.y) + _extensionOscFreqMinRange.x - (_extensionOscFreqMinRange.y * 0.5f);
				float extensionOscRange = ((input[0].uv.w * _extensionOscRangeMinRange.y) + _extensionOscRangeMinRange.x - _extensionOscRangeMinRange.y * 0.5f) * baseSize;

				float baseColorSat = (input[0].uvB.x * _baseColorSatMinRange.y) + _baseColorSatMinRange.x - _baseColorSatMinRange.y * 0.5f;
				float extensionScalar = (input[0].uvB.y * _extensionScalarMinRange.y) + _extensionScalarMinRange.x - _extensionScalarMinRange.y * 0.5f;
				float oscSize = ((input[0].uvB.z * _oscSizeMinRange.y) + _oscSizeMinRange.x - _oscSizeMinRange.y * 0.5f) * baseSize;

				float oscSizeFreq = (rand(float2(baseColorSat, baseColorHue)) * _oscSizeFreqMinRange.y) + _oscSizeFreqMinRange.x - _oscSizeFreqMinRange.y * 0.5f;
				float bodyCloseScalar = 1.0; // -(input[0].nrm.x);

				const int NUMVERT = 12;

				float randScales[NUMVERT] = {
					rand(float2(baseColorVal, oscSize)),
					rand(float2(baseColorHue, oscSize)),
					rand(float2(baseColorSat, extensionScalar)),
					rand(float2(baseColorVal, extensionOscRange)),
					rand(float2(baseColorHue, baseSize)),
					rand(float2(baseColorSat, oscSize)),
					rand(float2(baseColorVal, extensionScalar)),
					rand(float2(baseColorHue, extensionOscRange)),
					rand(float2(baseColorSat, baseSize)),
					rand(float2(baseColorVal, oscSize)),
					rand(float2(baseColorHue, baseColorVal)),
					rand(float2(baseColorSat, baseColorVal))
				};

				for (int i = 0; i < NUMVERT; i++)
					randScales[i] = extensionScalar + (sin(_Time[1] * extensionOscFreq * randScales[i]) / 2.0 + 0.5) * extensionOscRange;

				float baseScale = baseSize + (oscSize * (sin(_Time[1] * oscSizeFreq + rand(float2(extensionScalar, baseColorHue))) / 2.0 + 0.5)) * rand(float2(extensionOscFreq, vertexColorPal1));

				float scales[NUMVERT] = {
					rand(input[0].oldPos.xz),
					rand(input[0].oldPos.xy),
					rand(input[0].oldPos.yz),
					rand(input[0].uv.xy),
					rand(input[0].uv.xz),
					rand(input[0].uv.yz),
					rand(input[0].uv.xw),
					rand(input[0].uv.yw),
					rand(input[0].uvB.xy),
					rand(input[0].uvB.xz),
					rand(input[0].uvB.yz),
					rand(float2(input[0].uvB.x, input[0].uv.x))
				};

				for (int i = 0; i < NUMVERT; i++)
					scales[i] = (2.0 + (scales[i] * randScales[i])) * baseScale * .85f;

				//Set Mesh Values
				float3 VERTICES[NUMVERT] = {
					float3(0, -0.525731,  0.850651),
					float3(0.850651,         0,  0.525731),
					float3(0.850651,         0, -0.525731),
					float3(-0.850651,         0, -0.525731),
					float3(-0.850651,         0,  0.525731),
					float3(-0.525731,  0.850651,         0),
					float3(0.525731,  0.850651,         0),
					float3(0.525731, -0.850651,         0),
					float3(-0.525731, -0.850651,         0),
					float3(0, -0.525731, -0.850651),
					float3(0,  0.525731, -0.850651),
					float3(0,  0.525731,  0.850651)
				};

				for (int i = 0; i < NUMVERT; i++)
					VERTICES[i] *= scales[i];

				const int NUMTRI = 60;
				const int TRIANGLES[NUMTRI] = {
					1, 2, 1, 5, 1, 8, 1, 12, 1, 9,
					2, 3, 2, 7, 2, 8, 2, 12,
					3, 7, 3, 8, 3, 10, 3, 11,
					4, 5, 4, 6, 4, 9, 4, 10, 4, 11,
					5, 6, 5, 9, 5, 12,
					6, 7, 6, 11, 6, 12,
					7, 11, 7, 12,
					8, 9, 8, 10,
					9, 10,
					10, 11
				};

				/* FOR OLD TRIANGLE POLYGONS
				const int TRIANGLES[NUMTRI] = {
					2,  3,  7,
					2,  8,  3,
					4,  5,  6,
					5,  4,  9,
					7,  6,  12,
					6,  7,  11,
					10, 11, 3,
					11, 10, 4,
					8,  9,  10,
					9,  8,  1,
					12, 1,  2,
					1,  12, 5,
					7,  3,  11,
					2,  7,  12,
					4,  6,  11,
					6,  5,  12,
					3,  8,  10,
					8,  2,  1,
					4,  10, 9,
					5,  9,  1
				};
				*/

				float alphaMod = 1.0;

				float bSatMod = baseColorSat; // *(2 - bodyCloseScalar);
				float bValMod = baseColorVal; // *(((1 - bodyCloseScalar) * 12) + 1);

				if (input[0].uvB.w == 1.0) alphaMod = 0.0;
				else if (input[0].uvB.w > 0.0)
				{
					bSatMod *= (1 + (3 * input[0].uvB.w));

					bValMod *= ( 1 + input[0].uvB.w);
					bValMod = clamp(bValMod, 0, 1);
				}

				bValMod = clamp(bValMod, 0, 4);
				bSatMod = clamp(bSatMod, 0, 1);

				float4 COLS[NUMVERT];
				float4 baseColor = float4(HSV2RGB(float3(baseColorHue    * 360.0, bSatMod, bValMod)), 1.0);
				float4 colorPal1 = float4(HSV2RGB(float3(vertexColorPal1 * 360.0, bSatMod, bValMod)), 1.0);
				float4 colorPal2 = float4(HSV2RGB(float3(vertexColorPal2 * 360.0, bSatMod, bValMod)), 1.0);

				for (float i = 0.0; i < (float)NUMVERT; i += 1.0)
					COLS[i].rgba = colorPal1 * (i / (float)NUMVERT) + colorPal2 * (((float)NUMVERT - i) / NUMVERT);

				float colorTOffset = rand(input[0].oldPos.xz) + 0.4;
				float magSin =  sin(_Time[1] * colorTOffset) * 0.5 + 0.5;
				float phsSin = -sin(_Time[1] * colorTOffset) * 0.5 + 0.5;
				float4 modelInPos = mul(UNITY_MATRIX_M, input[0].pos);

				static g2f o[NUMTRI];

				for (int i = 0; i < NUMTRI; i++)
				{
					o[i].col = (0.125 + 0.75 * magSin) * COLS[TRIANGLES[i] - 1] + (0.125 + 0.75 * phsSin) * baseColor * 2.5f;
					o[i].col.a = alphaMod;
					o[i].pos = modelInPos + float4(VERTICES[TRIANGLES[i] - 1], 0.0) * 0.00375;

					UNITY_TRANSFER_FOG(o[i], mul(UNITY_MATRIX_VP, o[i].pos));
				}

				for (int i = 0; i < NUMTRI; i += 2)
				{
					float3 normals = -normalize(cross(o[i + 0].pos, o[i + 1].pos));

					o[i + 0].nrm = float3(0, 0, 0);
					o[i + 1].nrm = float3(0, 0, 0);

					o[i + 0].pos = mul(UNITY_MATRIX_VP, o[i + 0].pos);
					o[i + 1].pos = mul(UNITY_MATRIX_VP, o[i + 1].pos);

					o[i].colMod = input[0].colMod;
					o[i + 1].colMod = input[0].colMod;

					output.Append(o[i]);
					output.Append(o[i + 1]);

					output.RestartStrip();
				}

				/* FOR OLD TRIANGLE POLYGONS
				for (int i = 0; i < NUMTRI; i = i + 3)
				{
					float3 ab = o[i + 1].pos - o[i].pos;
					float3 bc = o[i + 2].pos - o[i].pos;
					float3 normals = -normalize(cross(bc, ab));

					o[i].nrm = normals;
					o[i + 1].nrm = normals;
					o[i + 2].nrm = normals;

					o[i].pos = mul(UNITY_MATRIX_VP, o[i].pos);
					o[i + 1].pos = mul(UNITY_MATRIX_VP, o[i + 1].pos);
					o[i + 2].pos = mul(UNITY_MATRIX_VP, o[i + 2].pos);

					o[i].colMod = input[0].colMod;
					o[i + 1].colMod = input[0].colMod;
					o[i + 2].colMod = input[0].colMod;

					output.Append(o[i]);
					output.Append(o[i + 1]);
					output.Append(o[i + 2]);

					output.RestartStrip();
				}
				*/
			}

			float4 frag(g2f i) : SV_Target
			{
				if (i.col.a == 0.0) discard; //Discard Vertices with null alphas

				float outCol = (dot(mul(UNITY_MATRIX_VP, float4(i.nrm, 0)).xyz, float3(1, -1, 1)) * 0.5 + 0.5) * 1.0;
				float4 col = float4(i.col.rgb * outCol, i.col.a);

				float4 fogCol = float4(1, 1, 1, 1);
				UNITY_APPLY_FOG(i.fogCoord, fogCol);

				col.rgb += i.colMod.rgb;
				col.a *= fogCol.r;

				return col;
			}

			ENDCG
		}

		Pass //fill
		{
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

			uniform StructuredBuffer<float2> highlightPosBuff : register(t1);
			uniform StructuredBuffer<float> highlightValBuff : register(t2);
			uniform StructuredBuffer<float2> obscurePosBuff : register(t3);
			uniform StructuredBuffer<float3> bodyPosBuff : register(t4);

			uniform StructuredBuffer<float3> wavPosBuff : register(t5);
			uniform StructuredBuffer<float> wavRadBuff : register(t6);
			uniform StructuredBuffer<float> wavIntBuff : register(t7);
			uniform StructuredBuffer<int> wavParamBuff : register(t8);

			//User I/O Structs
			struct VertexInput
			{
				float4 v : POSITION;
				float4 color: COLOR0;
				float4 uv : TEXCOORD0;
				float4 uvB : TEXCOORD1;
				float3 nrm : NORMAL;
			};

			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR;
				float4 uv : TEXCOORD0;
				float4 uvB : TEXCOORD1;
				float4 oldPos : TEXCOORD2;
				float3 nrm : NORMAL;

				float4 colMod : TEXCOORD3;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR;
				float3 nrm : NORMAL;
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

			//Converts HSV values to RGB values
			//Hue, Saturation, Value
			//Red, Green, Blue
			float3 HSVtoRGB(float3 HSV)
			{
				float3 RGB;
				RGB.x = abs(HSV.x * 6 - 3) - 1;
				RGB.y = 2 - abs(HSV.x * 6 - 2);
				RGB.z = 2 - abs(HSV.x * 6 - 4);
				RGB = saturate(RGB);

				return ((RGB - 1) * HSV.y + 1) * HSV.z;
			}

			float denormalizeToRange(float val, float min, float max)
			{
				return (val * (max - min)) + min;
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
			//Source: https://shadowmint.gitbooks.io/unity-material-shaders/content/support/syntax/lerp.html
			float lerpN(float a, float b, float w) {
				return b - (w * (b - a));
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

			//Clamp all values in the input float4 between [0, 1]
			float4 clamper(float4 inData)
			{
				inData.x = clamp(inData.x, 0.0, 1.0);
				inData.y = clamp(inData.y, 0.0, 1.0);
				inData.z = clamp(inData.z, 0.0, 1.0);
				inData.w = clamp(inData.w, 0.0, 1.0);

				return inData;
			}

			//Vertex Variables
			Vector _vertBounds;

			sampler2D _RenderTex;
			float4 _RenderTex_ST;

			float _wavStickTimeMod;
			float _wavStickTimeRange;
			float _wavStickTimePower;
			float _paramCutoff;
			float _wavIntMod;

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

			Vector _bodyCloseScalarPower;

			//Vertex Function
			VertexOutput vert(VertexInput v)
			{
				//Var Init
				VertexOutput o;

				o.nrm = v.nrm;
				o.oldPos = v.v;

				v.uvB.w = 0.0;

				for (int i = 0; i < 60; i++)
					if (v.nrm.x == highlightPosBuff[i].x && v.nrm.y == highlightPosBuff[i].y) v.uvB.w = 3.0f * highlightValBuff[i] - 2.0f;

				for (int i = 0; i < 60; i++)
					if (v.nrm.x == obscurePosBuff[i].x && v.nrm.y == obscurePosBuff[i].y) v.uvB.w = 1.0;

				/*
				v.uvB.w =
					((clamp( (float)( (_highlightPos0.x - v.nrm.x) + (_highlightPos0.y - v.nrm.y) ), -1.0, 0.0) + 1) * _highlightValue0) +
					((clamp((float)((_highlightPos1.x - v.nrm.x) + (_highlightPos1.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue1) +
					((clamp((float)((_highlightPos2.x - v.nrm.x) + (_highlightPos2.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue2) +
					((clamp((float)((_highlightPos3.x - v.nrm.x) + (_highlightPos3.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue3) +
					((clamp((float)((_highlightPos4.x - v.nrm.x) + (_highlightPos4.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue4) +
					((clamp((float)((_highlightPos5.x - v.nrm.x) + (_highlightPos5.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue5) +
					((clamp((float)((_highlightPos6.x - v.nrm.x) + (_highlightPos6.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue6) +
					((clamp((float)((_highlightPos7.x - v.nrm.x) + (_highlightPos7.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue7) +
					((clamp((float)((_highlightPos8.x - v.nrm.x) + (_highlightPos8.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue8) +
					((clamp((float)((_highlightPos9.x - v.nrm.x) + (_highlightPos9.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue9) +
					((clamp((float)((_highlightPos10.x - v.nrm.x) + (_highlightPos10.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue10) +
					((clamp((float)((_highlightPos11.x - v.nrm.x) + (_highlightPos11.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue11) +
					((clamp((float)((_highlightPos12.x - v.nrm.x) + (_highlightPos12.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue12) +
					((clamp((float)((_highlightPos13.x - v.nrm.x) + (_highlightPos13.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue13) +
					((clamp((float)((_highlightPos14.x - v.nrm.x) + (_highlightPos14.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue14) +
					((clamp((float)((_highlightPos15.x - v.nrm.x) + (_highlightPos15.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue15) +
					((clamp((float)((_highlightPos16.x - v.nrm.x) + (_highlightPos16.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue16) +
					((clamp((float)((_highlightPos17.x - v.nrm.x) + (_highlightPos17.y - v.nrm.y)), -1.0, 0.0) + 1) * _highlightValue17) +
					((clamp((float)((_obscurePos0.x - v.nrm.x) + (_obscurePos0.y - v.nrm.y)), -1.0, 0.0) + 1) * 1) +
					((clamp((float)((_obscurePos1.x - v.nrm.x) + (_obscurePos1.y - v.nrm.y)), -1.0, 0.0) + 1) * 1) +
					((clamp((float)((_obscurePos2.x - v.nrm.x) + (_obscurePos2.y - v.nrm.y)), -1.0, 0.0) + 1) * 1) +
					((clamp((float)((_obscurePos3.x - v.nrm.x) + (_obscurePos3.y - v.nrm.y)), -1.0, 0.0) + 1) * 1) +
					((clamp((float)((_obscurePos4.x - v.nrm.x) + (_obscurePos4.y - v.nrm.y)), -1.0, 0.0) + 1) * 1) +
					((clamp((float)((_obscurePos5.x - v.nrm.x) + (_obscurePos5.y - v.nrm.y)), -1.0, 0.0) + 1) * 1) +
					((clamp((float)((_obscurePos6.x - v.nrm.x) + (_obscurePos6.y - v.nrm.y)), -1.0, 0.0) + 1) * 1) +
					((clamp((float)((_obscurePos7.x - v.nrm.x) + (_obscurePos7.y - v.nrm.y)), -1.0, 0.0) + 1) * 1);
				*/

				//Bounds Checking
				if ((v.v.x < _vertBounds.z) && (v.v.x > _vertBounds.x) && (v.v.z < _vertBounds.w) && (v.v.z > _vertBounds.y))
				{
					float tX = (v.v.x - _vertBounds.x) / (_vertBounds.z - _vertBounds.x);
					float tZ = (v.v.z - _vertBounds.y) / (_vertBounds.w - _vertBounds.y);
					float4 tPos = float4(tex2Dlod(_RenderTex, float4(tX, tZ, 0, 0)).xyz, 1);

					v.v = float4(tPos.x, v.v.y + tPos.y, tPos.z, 1);
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
				/*
				float aDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPos.x)), 2) + pow(abs(v.v.z - (_lControllerPos.z)), 2))), 0.5) * 1.0;
				float bDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosB.x)), 2) + pow(abs(v.v.z - (_lControllerPosB.z)), 2))), 0.5) * 1.0;
				float cDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosC.x)), 2) + pow(abs(v.v.z - (_lControllerPosC.z)), 2))), 0.5) * 1.0;
				float dDist = pow(abs(sqrt(pow(abs(v.v.x - (_lControllerPosD.x)), 2) + pow(abs(v.v.z - (_lControllerPosD.z)), 2))), 0.5) * 1.0;

				if (_controllerHeldCDTimer < 0)  _controllerHeldCDTimer = 0.0;
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
				*/

				//DISPLACEMENT, REVERT, ???
				o.nrm.x = 0.0;

				float totBodyDist;
				for (int i = 0; i < 6; i++)
				{
					if (bodyPosBuff[i].z != 9999)
					{
						totBodyDist = sqrt(pow((bodyPosBuff[i].x - v.v.x), 2) + pow((bodyPosBuff[i].z - v.v.z), 2));
						o.nrm.x += max(0.0, 1.0 - pow(totBodyDist, _bodyCloseScalarPower.y) * _bodyCloseScalarPower.x);
					}
				}

				//Load Output
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
			[maxvertexcount(60)]
			void geom(point VertexOutput input[1], inout TriangleStream<g2f> output)
			{
				//Set Mod Values
				input[0].col = clamper(input[0].col);
				input[0].uv = clamper(input[0].uv);
				input[0].uvB = clamper(input[0].uvB);

				float baseColorVal = (input[0].col.r * _baseColorValMinRange.y) + _baseColorValMinRange.x - (_baseColorValMinRange.y * 0.5f);
				float vertexColorPal1 = (input[0].col.g);
				float baseSize = (input[0].col.a * _baseSizeMinRange.y) + _baseSizeMinRange.x - (_baseSizeMinRange.y * 0.5f);

				float vertexColorPal2 = (input[0].uv.x);
				float baseColorHue = (input[0].uv.y);
				float extensionOscFreq = (input[0].uv.z * _extensionOscFreqMinRange.y) + _extensionOscFreqMinRange.x - (_extensionOscFreqMinRange.y * 0.5f);
				float extensionOscRange = ((input[0].uv.w * _extensionOscRangeMinRange.y) + _extensionOscRangeMinRange.x - _extensionOscRangeMinRange.y * 0.5f) * baseSize;

				float baseColorSat = (input[0].uvB.x * _baseColorSatMinRange.y) + _baseColorSatMinRange.x - _baseColorSatMinRange.y * 0.5f;
				float extensionScalar = (input[0].uvB.y * _extensionScalarMinRange.y) + _extensionScalarMinRange.x - _extensionScalarMinRange.y * 0.5f;
				float oscSize = ((input[0].uvB.z * _oscSizeMinRange.y) + _oscSizeMinRange.x - _oscSizeMinRange.y * 0.5f) * baseSize;

				float oscSizeFreq = (rand(float2(baseColorSat, baseColorHue)) * _oscSizeFreqMinRange.y) + _oscSizeFreqMinRange.x - _oscSizeFreqMinRange.y * 0.5f;
				float bodyCloseScalar = 1.0 - (input[0].nrm.x);

				const int NUMVERT = 12;

				float randScales[NUMVERT] = {
					rand(float2(baseColorVal, oscSize)),
					rand(float2(baseColorHue, oscSize)),
					rand(float2(baseColorSat, extensionScalar)),
					rand(float2(baseColorVal, extensionOscRange)),
					rand(float2(baseColorHue, baseSize)),
					rand(float2(baseColorSat, oscSize)),
					rand(float2(baseColorVal, extensionScalar)),
					rand(float2(baseColorHue, extensionOscRange)),
					rand(float2(baseColorSat, baseSize)),
					rand(float2(baseColorVal, oscSize)),
					rand(float2(baseColorHue, baseColorVal)),
					rand(float2(baseColorSat, baseColorVal))
				};

				for (int i = 0; i < NUMVERT; i++)
					randScales[i] = extensionScalar + (sin(_Time[1] * extensionOscFreq * randScales[i]) / 2.0 + 0.5) * extensionOscRange;

				float baseScale = baseSize + (oscSize * (sin(_Time[1] * oscSizeFreq + rand(float2(extensionScalar, baseColorHue))) / 2.0 + 0.5)) * rand(float2(extensionOscFreq, vertexColorPal1));

				float scales[NUMVERT] = {
					rand(input[0].oldPos.xz),
					rand(input[0].oldPos.xy),
					rand(input[0].oldPos.yz),
					rand(input[0].uv.xy),
					rand(input[0].uv.xz),
					rand(input[0].uv.yz),
					rand(input[0].uv.xw),
					rand(input[0].uv.yw),
					rand(input[0].uvB.xy),
					rand(input[0].uvB.xz),
					rand(input[0].uvB.yz),
					rand(float2(input[0].uvB.x, input[0].uv.x))
				};

				for (int i = 0; i < NUMVERT; i++)
					scales[i] = (2.0 + (scales[i] * randScales[i])) * baseScale * .84;

				//Set Mesh Values
				float3 VERTICES[NUMVERT] = {
					float3(0, -0.525731,  0.850651),
					float3(0.850651,         0,  0.525731),
					float3(0.850651,         0, -0.525731),
					float3(-0.850651,         0, -0.525731),
					float3(-0.850651,         0,  0.525731),
					float3(-0.525731,  0.850651,         0),
					float3(0.525731,  0.850651,         0),
					float3(0.525731, -0.850651,         0),
					float3(-0.525731, -0.850651,         0),
					float3(0, -0.525731, -0.850651),
					float3(0,  0.525731, -0.850651),
					float3(0,  0.525731,  0.850651)
				};

				for (int i = 0; i < NUMVERT; i++)
					VERTICES[i] *= scales[i];

				const int TRIANGLES[60] = { 2,  3,  7,
											2,  8,  3,
											4,  5,  6,
											5,  4,  9,
											7,  6,  12,
											6,  7,  11,
											10, 11, 3,
											11, 10, 4,
											8,  9,  10,
											9,  8,  1,
											12, 1,  2,
											1,  12, 5,
											7,  3,  11,
											2,  7,  12,
											4,  6,  11,
											6,  5,  12,
											3,  8,  10,
											8,  2,  1,
											4,  10, 9,
											5,  9,  1 };

				static g2f o[60];

				float alphaMod = 1.0;

				float bSatMod = baseColorSat * (((1 - bodyCloseScalar) * 1.0) + 1);
				float bValMod = baseColorVal * (((1 - bodyCloseScalar) * 12) + 1);
				float darknessMod = 0.98f; //Hannah highlight color edits before
					

				/*if (input[0].uvB.w == 0.5)
				{
					bSatMod *= 4;

					bValMod *= 2;
					bValMod = clamp(bValMod, 0, 2);
				}*/
				if (input[0].uvB.w == 1.0)
				{
					alphaMod = 0.0;
				}
				if (input[0].uvB.w > 0.0)
				{
					bSatMod *= (1 + (3 * input[0].uvB.w)) ;

					bValMod *= (1 + input[0].uvB.w);
					bValMod = clamp(bValMod, 0, 1);
					darknessMod = .98 + (.02 * (input[0].uvB.w) ); //Hannah highlight color edits before
					darknessMod = clamp(darknessMod, 0.979, 1); //Hannah highlight color edits before
					//darknessMod = input[0].uvB.w; //Hannah highlight color edits after

				}

					

				bValMod = clamp(bValMod, 0, 4);
				bSatMod = clamp(bSatMod, 0, 1);

				float4 COLS[12];
				float4 baseColor = float4(HSVtoRGB(float3(baseColorHue, bSatMod, bValMod)), 1.0);
				float4 colorPal1 = float4(HSVtoRGB(float3(vertexColorPal1, 0 + 1 * bSatMod, 0 + 1 * bValMod)), 1.0);
				float4 colorPal2 = float4(HSVtoRGB(float3(vertexColorPal2, 0 + 1 * bSatMod, 0 + 1 * bValMod)), 1.0);

				for (int i = 0; i < 12; i++)
				{
					COLS[i].rgba = (colorPal1 * ((float)i) / 12.0 + colorPal2 * ((float)(12 - i)) / 12.0);
				}

				float colorTOffset = rand(input[0].oldPos.xz) + 0.4;
				float magSin = sin(_Time[1] * colorTOffset) * 0.5 + 0.5;
				float phsSin = sin(_Time[1] * colorTOffset + 3.14159) * 0.5 + 0.5;
				float4 modelInPos = mul(UNITY_MATRIX_M, input[0].pos);

				for (int i = 0; i < 60; i++)
				{
					o[i].col = (0.25 + 0.5 * magSin) * COLS[TRIANGLES[i] - 1] + (0.25 + 0.5 * phsSin) * baseColor;
					o[i].col.a = darknessMod * alphaMod;
					o[i].pos = modelInPos + float4(VERTICES[TRIANGLES[i] - 1], 0.0) * 0.00375;

					UNITY_TRANSFER_FOG(o[i], mul(UNITY_MATRIX_VP, o[i].pos));
				}

				for (int i = 0; i < 60; i = i + 3)
				{
					float3 ab = o[i + 1].pos - o[i].pos;
					float3 bc = o[i + 2].pos - o[i].pos;
					float3 normals = -normalize(cross(bc, ab));

					o[i].nrm = normals;
					o[i + 1].nrm = normals;
					o[i + 2].nrm = normals;

					o[i].pos = mul(UNITY_MATRIX_VP, o[i].pos);
					o[i + 1].pos = mul(UNITY_MATRIX_VP, o[i + 1].pos);
					o[i + 2].pos = mul(UNITY_MATRIX_VP, o[i + 2].pos);

					o[i].colMod = input[0].colMod;
					o[i + 1].colMod = input[0].colMod;
					o[i + 2].colMod = input[0].colMod;

					output.Append(o[i]);
					output.Append(o[i + 1]);
					output.Append(o[i + 2]);

					output.RestartStrip();
				}
			}

			float4 frag(g2f i) : SV_Target
			{
				if (i.col.a == 0.0) discard; //Discard Vertices with null alphas

				float outCol = (dot(mul(UNITY_MATRIX_VP, float4(i.nrm, 0)).xyz, float3(1, -1, 1)) * 0.5 + 0.5) * 1.0;
				float4 col = float4(i.col.rgb * outCol, i.col.a);

				float4 fogCol = float4(1, 1, 1, 1);
				UNITY_APPLY_FOG(i.fogCoord, fogCol);

				col.rgb += i.colMod; //Hannah highlight color edits after (but also the old way of filling stars)
				//col.rgb = float3(0, 0, 0); //Hannah highlight color edits before
				float darknessMod = (i.col.a - .98) * (50); //Hannah highlight color edits after
				float3 minCol = col.rgb * (.015f);
				col.rgb = lerp(minCol, col.rgb, darknessMod);//Hannah Color Highlight fix line after
				col.a *= fogCol.r * i.col.a;//Hannah Color Highlight fix line

				return col;
			}

			ENDCG
		}
	}
}
