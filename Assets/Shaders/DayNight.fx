#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_3_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;

float4x4 lightX;
float4x4 lightY;
float4x4 lightI;
float4x4 lightR;
float4x4 lightG;
float4x4 lightB;

float4 ambient;
float bloom;

float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	float4 pixel = tex2D(s0, texCoord) * color1;

	float dist;
	float distance1 = 0;
	float distance2 = 0;
	float distance3 = 0;
	float distance4 = 0;
	float distance5 = 0;
	float distance6 = 0;
	float distance7 = 0;
	float distance8 = 0;
	float distance9 = 0;
	float distance10 = 0;
	float distance11 = 0;
	float distance12 = 0;

	if (lightI._11 > 0)
	{
		dist = distance(position, float2(lightX._11, lightY._11));
		distance1 = round((1.0f - smoothstep(0, lightI._11, dist)) * 12.0f) / lightI._11 / 12.0f;
	}

	if (lightI._12 > 0)
	{
		dist = distance(position, float2(lightX._12, lightY._12));
		distance2 = round((1.0f - smoothstep(0, lightI._12, dist)) * 12.0f) / lightI._12 / 12.0f;
	}

	if (lightI._13 > 0)
	{
		dist = distance(position, float2(lightX._13, lightY._13));
		distance3 = round((1.0f - smoothstep(0, lightI._13, dist)) * 12.0f) / lightI._13 / 12.0f;
	}

	if (lightI._14 > 0)
	{
		dist = distance(position, float2(lightX._14, lightY._14));
		distance4 = round((1.0f - smoothstep(0, lightI._14, dist)) * 12.0f) / lightI._14 / 12.0f;
	}

	if (lightI._21 > 0)
	{
		dist = distance(position, float2(lightX._21, lightY._21));
		distance5 = round((1.0f - smoothstep(0, lightI._21, dist)) * 12.0f) / lightI._21 / 12.0f;
	}

	if (lightI._22 > 0)
	{
		dist = distance(position, float2(lightX._22, lightY._22));
		distance6 = round((1.0f - smoothstep(0, lightI._22, dist)) * 12.0f) / lightI._22 / 12.0f;
	}

	if (lightI._23 > 0)
	{
		dist = distance(position, float2(lightX._23, lightY._23));
		distance7 = round((1.0f - smoothstep(0, lightI._23, dist)) * 12.0f) / lightI._23 / 12.0f;
	}

	if (lightI._24 > 0)
	{
		dist = distance(position, float2(lightX._24, lightY._24));
		distance8 = round((1.0f - smoothstep(0, lightI._24, dist)) * 12.0f) / lightI._24 / 12.0f;
	}

	if (lightI._31 > 0)
	{
		dist = distance(position, float2(lightX._31, lightY._31));
		distance9 = round((1.0f - smoothstep(0, lightI._31, dist)) * 12.0f) / lightI._31 / 12.0f;
	}

	if (lightI._32 > 0)
	{
		dist = distance(position, float2(lightX._32, lightY._32));
		distance10 = round((1.0f - smoothstep(0, lightI._32, dist)) * 12.0f) / lightI._32 / 12.0f;
	}

	if (lightI._33 > 0)
	{
		dist = distance(position, float2(lightX._33, lightY._33));
		distance11 = round((1.0f - smoothstep(0, lightI._33, dist)) * 12.0f) / lightI._33 / 12.0f;
	}

	if (lightI._34 > 0)
	{
		dist = distance(position, float2(lightX._34, lightY._34));
		distance12 = round((1.0f - smoothstep(0, lightI._34, dist)) * 12.0f) / lightI._34 / 12.0f;
	}

	float redlight = distance1 * lightR._11 + distance2 * lightR._12 + distance3 * lightR._13 + distance4 * lightR._14 + distance5 * lightR._21 + distance6 * lightR._22 + distance7 * lightR._23 + distance8 * lightR._24 + distance9 * lightR._31 + distance10 * lightR._32 + distance11 * lightR._33 + distance11 * lightR._34;
	if (redlight > bloom) redlight = bloom;
	if (redlight < 0.0) redlight = 0.0;
	float greenlight = distance1 * lightG._11 + distance2 * lightG._12 + distance3 * lightG._13 + distance4 * lightG._14 + distance5 * lightG._21 + distance6 * lightG._22 + distance7 * lightG._23 + distance8 * lightG._24 + distance9 * lightG._31 + distance10 * lightG._32 + distance11 * lightG._33 + distance11 * lightG._34;
	if (greenlight > bloom) greenlight = bloom;
	if (greenlight < 0.0) greenlight = 0.0;
	float bluelight = distance1 * lightB._11 + distance2 * lightB._12 + distance3 * lightB._13 + distance4 * lightB._14 + distance5 * lightB._21 + distance6 * lightB._22 + distance7 * lightB._23 + distance8 * lightB._24 + distance9 * lightB._31 + distance10 * lightB._32 + distance11 * lightB._33 + distance11 * lightB._34;
	if (bluelight > bloom) bluelight = bloom;
	if (bluelight < 0.0) bluelight = 0.0;
	

	return float4(lerp(ambient.x * pixel.x, pixel.x, redlight), lerp(ambient.y * pixel.y, pixel.y, greenlight), lerp(ambient.z * pixel.z, pixel.z, bluelight), pixel.w);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}