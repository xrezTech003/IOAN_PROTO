Shader "Custom/StarIDShader"
{
	// this shader is used by the cameras attached to the controllers
	// to detect when the controller is touching a "star"

	// this shader should be the same as the shader rendering the interactive space
	// (currently called vertexcolorprocnew)

	// they should be kept in sync using includes (instead of the mess that is currently being used)

	// the only differnce between the rendered doing the interactive space should be the fragment shader
	// and the tags.  The fragment shader sould render star ids (as RGB) instead of colors

	//		FOR REFERENCE
	//		not sure if this is current EGM
	//		in asd2.newStart it looks like newUV2.w is the id
	//		and v.y is parsecs (scaled into an offset)
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
		_vertBounds("vertBounds",Vector) = (0,0,0,0)
		_RenderTex("Color (RGB) Alpha (A)", 2D) = "white" {}

		_wavStickTimeMod("wavStickTimeMod",float) = 0
		_wavStickTimeRange("wavStickTimeRange",float) = 0
		_wavStickTimePower("wavStickTimePower",float) = 0
		_paramCutoff("paramCutoff",float) = 0

		//Geometry Properties
		_baseColorValMinRange("baseColorValMinRange",Vector) = (1,0,0)
		_rotSpeedMinRange("rotSpeedMinRange",Vector) = (1,0,0)
		_baseSizeMinRange("baseSizeMinRange",Vector) = (1,0,0)
		_extensionOscFreqMinRange("extensionOscFreqMinRange",Vector) = (1,0,0)
		_extensionOscRangeMinRange("extensionOscRangeMinRange",Vector) = (1,0,0)
		_baseColorSatMinRange("baseColorSatMinRange",Vector) = (1,0,0)
		_extensionScalarMinRange("extensionScalarMinRange",Vector) = (1,0,0)
		_oscSizeMinRange("oscSizeMinRange",Vector) = (1,0,0)
		_oscSizeFreqMinRange("oscSizeFreqMinRange",Vector) = (1,0,0)
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

			LOD 200

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom
				#pragma multi_compile_fog
				#pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap
				#pragma target 5.0

				#include "UnityCG.cginc"

				uniform StructuredBuffer<float2> highlightPosBuff : register(t1);
				uniform StructuredBuffer<float2> obscurePosBuff : register(t3);

				uniform StructuredBuffer<float3> wavPosBuff : register(t5);
				uniform StructuredBuffer<float> wavRadBuff : register(t6);
				uniform StructuredBuffer<float> wavIntBuff : register(t7);
				uniform StructuredBuffer<int> wavParamBuff : register(t8);

				struct VertexInput
				{
					float4 v : POSITION;
					float4 color: COLOR;
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
					float3 starID : TEXCOORD3;
				};

				struct g2f
				{
					float4 pos : SV_POSITION;
					float4 col : COLOR;
					float3 nrm : NORMAL;
					float3 starID : TEXCOORD3;
					UNITY_FOG_COORDS(2)
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
					float r = (1 - (w / m));
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

				//Vertex Function
				VertexOutput vert(VertexInput v)
				{
					VertexOutput o;

					o.nrm = v.nrm;
					o.oldPos = v.v;

					//Star Highlighting/Obscuring
					v.uvB.w = 0.0;
					o.starID = v.nrm;

					for (int i = 0; i < 60; i++)
						if (v.nrm.x == highlightPosBuff[i].x && v.nrm.y == highlightPosBuff[i].y) v.uvB.w = 0.5;

					for (int i = 0; i < 60; i++)
						if (v.nrm.x == obscurePosBuff[i].x && v.nrm.y == obscurePosBuff[i].y) v.uvB.w = 1.0;

					//Bounds Checking
					if ((v.v.x < _vertBounds.z && v.v.x > _vertBounds.x) && (v.v.z < _vertBounds.w && v.v.z > _vertBounds.y))
					{
						float tX = (v.v.x - _vertBounds.x) / (_vertBounds.z - _vertBounds.x);
						float tZ = (v.v.z - _vertBounds.y) / (_vertBounds.w - _vertBounds.y);
						float4 tPos = float4(tex2Dlod(_RenderTex, float4(tX, tZ, 0, 0)).xyz, 1);

						v.v = float4(tPos.x, v.v.y + tPos.y, tPos.z, 1);
					}

					//Wave Control
					float wavStickTimeModifier = _wavStickTimeMod + rand(float2(o.oldPos.x, o.oldPos.z)) * (_wavStickTimeRange);
					float wavStickTimePower = _wavStickTimePower;
					float paramCheckCutoff = _paramCutoff;
					float wavMod = 2.0;
					float wavCutOff = 30.0;
					float oldY = v.v.y;
					float wavIntMod = 0.25;

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

								switch (wavParam)
								{
								case 0: //RMS < 0.15
									if (denormalizeToRange(v.uvB.x, 0.0, 0.6715103569670999) < 0.15)
										paramBool = 1;
									break;
								case 1: //SNR > 12
									if (denormalizeToRange(v.uv.w, 0.0, 26.61842977741538) > 12.0)
										paramBool = 1;
									break;
								case 2: //Highest Teff in the Grid //SUB IN AstroColor
									if (denormalizeToRange(v.uv.y, 1.231199705120561, 1.8991459147993115) < 1.5)
										paramBool = 1;
									break;
								case 3: //Variable Class 3 tier scores
									if (denormalizeToRange(v.uvB.y, 0.0, 1.0) == 1.0) //Possibly implemented poorly in Mesh
										paramBool = 1;
									break;
								}

								if (paramBool == 1 /*&& (v.v.y - oldY < (wavInt * wavIntMod) || i == 0)*/)
								{
									float yMod = (wavInt * wavIntMod) * min(max((1.0 - pow(wavRad - posi, wavStickTimePower) * wavStickTimeModifier), 0.0), 1.0);
									v.v.y += yMod;
								}
								else if (temp < wavCutOff)
									v.v.y += (wavInt * wavIntMod) * sin(wavMod * temp) / (wavMod * temp);
							}
						}
					}

					o.nrm.x = 0.0;

					//Load Output
					o.pos = v.v;
					o.col.rgb = v.color.rgb;
					o.col.a = v.color.a;
					o.uv = v.uv;
					o.uvB = v.uvB;

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
						2,  3,  7,
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
						5,  9,  1
					};

					//Load new Vertices
					static g2f o[NUMTRI];

					//StarID Encoding
					uint starID = (input[0].uvB.w == 1.0) ? 0 : ((uint)(input[0].starID.x * 10000)) + (uint)input[0].starID.y;

					float a = (float)((starID & 0xFF000000) >> 24);
					float r = (float)((starID & 0x00FF0000) >> 16);
					float g = (float)((starID & 0x0000FF00) >> 8);
					float b = (float)((starID & 0x000000FF) >> 0);

					float4 starIDColor = float4(r, g, b, a) / 255.0f;
					float4 modelInPos = mul(UNITY_MATRIX_M, input[0].pos);

					for (int i = 0; i < 60; i++)
					{
						o[i].col = starIDColor;
						o[i].starID = input[0].starID;
						o[i].pos = modelInPos + float4(VERTICES[TRIANGLES[i] - 1], 0.0) * 0.00375;
						UNITY_TRANSFER_FOG(o[i], mul(UNITY_MATRIX_VP, o[i].pos));
					}

					//Generate all new vertices
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

						output.Append(o[i + 0]);
						output.Append(o[i + 1]);
						output.Append(o[i + 2]);

						output.RestartStrip();
					}
				}

				//Fragment Function
				float4 frag(g2f i) : SV_Target
				{
					if (i.col.a == 0.0) discard;
					return i.col;
				}

				ENDCG
			}
		}
}
