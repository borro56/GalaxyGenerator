float random2(float i, float j)
{
	const uint hash[256] = { 151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
	140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
	247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
	57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
	74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
	60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
	65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
	200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
	52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
	207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
	119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
	129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
	218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
	81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
	184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
	222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180 };

	uint h = abs(i);
	h = hash[h % 256] + abs(j);
	h = hash[h % 256];
	return (int)h / 256.0;
}

float2 grad2(float i)
{
	const float2 grads[8] =
	{
		normalize(float2(1,1)),
		normalize(float2(-1,1)),
		normalize(float2(-1,-1)),
		normalize(float2(1,-1)),
		float2(0,1),
		float2(1,0),
		float2(-1,0),
		float2(0,-1)
	};

	return grads[(uint)i % 8];
}

//TODO: Make overload that accepts two floats
float noise2(float2 uv)
{
	float offsetX = frac(uv.x);
	float offsetY = frac(uv.y);

	float i00 = random2(uv.x, uv.y);
	float i01 = random2(uv.x + 1, uv.y);
	float i10 = random2(uv.x, uv.y + 1);
	float i11 = random2(uv.x + 1, uv.y + 1);

	float2 g00 = grad2(i00 * 8);
	float2 g01 = grad2(i01 * 8);
	float2 g10 = grad2(i10 * 8);
	float2 g11 = grad2(i11 * 8);

	float2 c00 = float2(offsetX, offsetY);
	float2 c01 = float2(offsetX - 1, offsetY);
	float2 c10 = float2(offsetX, offsetY - 1);
	float2 c11 = float2(offsetX - 1, offsetY - 1);

	float v00 = dot(g00, c00) * 0.5 + 0.5;
	float v01 = dot(g01, c01) * 0.5 + 0.5;
	float v10 = dot(g10, c10) * 0.5 + 0.5;
	float v11 = dot(g11, c11) * 0.5 + 0.5;

	offsetX = smooth(offsetX);
	offsetY = smooth(offsetY);

	return lerp(lerp(v00, v01, offsetX),
				lerp(v10, v11, offsetX),
				offsetY);
}