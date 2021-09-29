Shader "Custom/CelestialShader" 
{
    Properties
    {
        _Blend("Blend", Range(-2, 10.0)) = 1
        _NoiseScale("NoiseScale", Range(0.1, 2.5)) = 1
        _NoiseWeight("NoiseWeight", range(0,10)) = 1
        _Drip("Drip", Range(-4, 4)) = 1
        _Speed("speed", range(0,1)) = 0.5
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _RimColor ("RimColor", Color) = (1,1,1,1)
        _RimExponent ("RimExponent", Range(0.5, 8.0)) = 2
        _GradColor0 ("GradColor0", Color) = (1,1,1,1)
        _GradColor1 ("GradColor1", Color) = (0,0,0,1)
        _EdgeColor ("EdgeColor", Color) = (0,0,0,1)
        _EdgeWidth("EdgeWidth", range(0.0,1.1)) = 0.975
        _Poke("Poke", range(-1,1)) = 0
		_Alpha("Alpha", Range(0.0, 1.0)) = 1.0
    }

    SubShader
    {
		Tags 
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // include file that contains UnityObjectToWorldNormal helper function
            #include "UnityCG.cginc"

			//IO Struct
            struct appdata 
			{
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD;
            };

            struct v2f 
			{
                float3 normal : NORMAL;
                float2 vUv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 localNorm : TEXCOORD2;
                float4 view : TEXCOORD3;
            };

            float _Blend; // col.r -> magnitude
            float _Drip; // pos.y -> parallax
            float _NoiseScale; // uvb.x -> rms
            float _NoiseWeight; // uv.w -> snr
            float _Speed; // uv.z -> period
            float _EdgeWidth; // uvb.z -> proper motion
            float _Poke; // col.b -> num obs
            float _RimExponent; // col.r -> magnitude

            float4 _Color; // ???
            float4 _RimColor; // uv.y -> psuedocolor
            float4 _GradColor0; // col.g -> std dev
            float4 _GradColor1; // col.a -> variability
            float4 _EdgeColor; // ???

			float _Alpha;

            sampler2D _MainTex;

			//User Functions
			//quatFromAxisAngle: return Quaternion
			// http://www.euclideanspace.com/maths/geometry/rotations/conversions/angleToQuaternion/index.htm
			// assumes axis is normalized
            float4 quatFromAxisAngle(float3 axis, float angle) //Useless
			{
				float halfAngle = angle / 2.0;
				float s = sin( halfAngle );
				return float4(axis * s, cos(halfAngle));
            }

			//qv3(float4(sin(0.0005), 0, 0, cos(0.0005)), normal);
            float3 qv3(float4 q, float3 v ) //Useless
			{
				float x = v.x;
				float y = v.y;
				float z = v.z;
				float qx = q.x; //0.0005
				float qy = q.y; //0.0
				float qz = q.z; //0.0
				float qw = q.w; //1.0

				// calculate quat * floattor
				float ix =  qw * x + qy * z - qz * y; //x
				float iy =  qw * y + qz * x - qx * z; //y - 0.0005z
				float iz =  qw * z + qx * y - qy * x; //z + 0.0005y
				float iw = -qx * x - qy * y - qz * z; //-0.0005x

				//return float3(x, y - 0.001 * z, z + 0.001 * y);

				// calculate result * inverse quat
				return float3( ix * qw + iw * -qx + iy * -qz - iz * -qy,  //x
							   iy * qw + iw * -qy + iz * -qx - ix * -qz,  //y - 0.001z
							   iz * qw + iw * -qz + ix * -qy - iy * -qx); //z + 0.001y
            }

            float hash(float n)
            {
                return frac(sin(n) * 43758.5453);
            }

			// The noise function returns a value in the range -1.0f -> 1.0f
            float noise( float3 x )
            {
                float3 p = floor(x);
                float3 f = frac(x);

                f = f * f * (3.0 - 2.0 * f);

                float n = p.x + p.y*57.0 + 113.0*p.z;

                return lerp(
							lerp(
								 lerp(hash(n + 0.0), hash(n + 1.0), f.x),
                                 lerp(hash(n + 57.0), hash(n + 58.0), f.x), 
								 f.y),
                            lerp(
								 lerp(hash(n + 113.0), hash(n + 114.0), f.x),
                                 lerp(hash(n + 170.0), hash(n + 171.0), f.x), 
							     f.y),
					        f.z);
            }
     
            float mapLinear(float x, float a1, float a2, float b1, float b2) //Useless
			{
                return a2 == a1 ? b1 : b1 + ( x - a1 ) * ( b2 - b1 ) / ( a2 - a1 );
            }

            float3 nf3p(float3 p0, float3 p1, float3 p2)
			{
              return normalize(cross(normalize(p2 - p1), normalize(p0 - p1)));
            }

            float3 getWorldPos(float3 pos, float3 offset, float offsetDistance, float posScale, float dripScale, float3 nrm, float2 uv)
			{
				//Get initial values
                float orgLength = length(pos);
                float3 dir = normalize(pos);
				float3 p = float3(pos.x, pos.y * 2, pos.z);

                //Get noise of pos * scale and the offset + noise of half p * scale and double the offset, then the squared half
                float n = noise(pos * posScale + offset);
                n += noise(p * posScale * 0.5 + offset * 2.0);
                n = pow(n / 2, 2);

				//Output vector in direction of pos scaled by pos length, offset, and noise + pos
                float3 outpos = (offsetDistance * n * orgLength) * dir + pos;

				//Calculate the drip based on the y position
				float drip = smoothstep(0, 1, 0.5 * (1 - pos.y));

				//Add to the output a vector scaled with the drip, noise, and drip scale
                outpos += normalize(outpos) * drip * n * n * dripScale;

				//Interpolate based on the poke if the uv coordinate is above the width
                if(uv.y < _EdgeWidth) outpos = lerp(outpos, float3(0, 0, 0), _Poke );

                return outpos;
            }

            //Vertex Function
            v2f vert (appdata v)
            {
                float sampleOffset = 0.001;
                float offsetDistance = _NoiseWeight;
                float posScale = _NoiseScale;
                float dripScale = _Drip;

				//A clamped blend value
                float _blendAmount = clamp(_Blend,0,1);

				//Output
                v2f o;

				//Keep UVs the same
                o.vUv = v.uv;

				//Get unclamped interpolation of amount _Blend between point local position and its dir vector
                float3 pos = v.vertex.xyz;
                pos = lerp(pos, normalize(pos), _Blend);

				//Get clamped interpolation between pos dir vector and the point local normal
                float3 normal = normalize(pos);
                normal = lerp(v.normal.xyz, normal, _blendAmount);

				//Get clamped interpolation between actual tangent and this new vector
                float3 tangent = normalize(float3(0, 0.001 * normal.z, -0.001 * normal.y));
                tangent = lerp(v.tangent.xyz, tangent, _blendAmount);

                float3 bitangent = cross(normal, tangent);
                float3 timeOffset = float3(0, _Time.y * clamp(_Speed, -3.5f, 3.5f), 0);
                float offsetUVScale = 1.1;
                
				//Get the world positions for pos, pos + tangent, and pos + bitangent
                float3 p0 = getWorldPos(pos, timeOffset, offsetDistance, posScale, dripScale, normal, o.vUv);
                float3 p1 = getWorldPos(pos + tangent * sampleOffset, timeOffset , offsetDistance, posScale, dripScale, normal, o.vUv * 0.9);
                float3 p2 = getWorldPos(pos + bitangent * sampleOffset, timeOffset , offsetDistance, posScale, dripScale, normal, o.vUv * 1.1);

				//Get normal among all three
                float3 norm = nf3p(p0, p2, p1);

				//Set Output Values
                o.worldPos = p0;
                o.view = mul(UNITY_MATRIX_MV, float4(p0, 1));
                o.pos = UnityObjectToClipPos(float4(p0, 1));
                o.localNorm = norm;
                o.normal = normalize(mul((float3x3)UNITY_MATRIX_MV, -norm ));
                o.normal.z = abs(o.normal.z);

                return o;
            }

			//Fragmentation Function
            fixed4 frag (v2f i) : SV_Target
            {
				//Get local uv over object
				float2 uv = (i.localNorm.xy + 1.0) / 2.0;

				//Sample Texture and apply color
                float4 c = tex2D(_MainTex, uv.yx) * _Color;

				//Set output color based on point uv comparison to _EdgeWidth
                c.rgb = i.vUv.x < _EdgeWidth ? c.rgb : lerp(c.rgb, _EdgeColor.rgb, _EdgeColor.a * 2);

				//How much the localNorm is in the +y direction
				float vertGrad = (dot(normalize(i.localNorm), float3(0, 1, 0)) + 1.0) / 2.0;

				float mfr = 1 - max(0, dot(normalize(i.view.xyz), normalize(-i.normal)));
                c *= lerp(_GradColor0, _GradColor1, vertGrad);
                c.rgb = lerp(c.rgb, _RimColor, pow(mfr, _RimExponent));

				c.a = _Alpha;

                return c;
            }

            ENDCG
        }
    }
}
