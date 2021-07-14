float random3(float i, float j, float k)
{
	const uint hash3[256] = { 151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
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
	h = hash3[h % 256] + abs(j);
	h = hash3[h % 256] + abs(k);
	h = hash3[h % 256];
	return (int)h / 256.0;
}

float random3(float3 pos)
{
    return random3(pos.x, pos.y, pos.z);
}

float3 grad3(float i)
{
	const float3 grads[12] =
	{
		normalize(float3(1,1,0)),
		normalize(float3(-1,1,0)),
		normalize(float3(1,-1,0)),
		normalize(float3(-1,-1,0)),
		normalize(float3(1,0,1)),
		normalize(float3(-1,0,1)),
		normalize(float3(1,0,-1)),
		normalize(float3(-1,0,-1)),
		normalize(float3(0,1,1)),
		normalize(float3(0,-1,1)),
		normalize(float3(0,1,-1)),
		normalize(float3(0,-1,-1))
	};

	return grads[i * 12];
}

float noise3(float3 pos)
{
	float offsetX = frac(pos.x);
	float offsetY = frac(pos.y);
	float offsetZ = frac(pos.z);

	float i000 = random3(pos.x, pos.y, pos.z);
	float i010 = random3(pos.x + 1, pos.y, pos.z);
	float i100 = random3(pos.x, pos.y + 1, pos.z);
	float i110 = random3(pos.x + 1, pos.y + 1, pos.z);
	float i001 = random3(pos.x, pos.y, pos.z + 1);
	float i011 = random3(pos.x + 1, pos.y, pos.z + 1);
	float i101 = random3(pos.x, pos.y + 1, pos.z + 1);
	float i111 = random3(pos.x + 1, pos.y + 1, pos.z + 1);

	float3 g000 = grad3(i000);
	float3 g010 = grad3(i010);
	float3 g100 = grad3(i100);
	float3 g110 = grad3(i110);
	float3 g001 = grad3(i001);
	float3 g011 = grad3(i011);
	float3 g101 = grad3(i101);
	float3 g111 = grad3(i111);

	float3 c000 = float3(offsetX, offsetY, offsetZ);
	float3 c010 = float3(offsetX - 1, offsetY, offsetZ);
	float3 c100 = float3(offsetX, offsetY - 1, offsetZ);
	float3 c110 = float3(offsetX - 1, offsetY - 1, offsetZ);
	float3 c001 = float3(offsetX, offsetY, offsetZ - 1);
	float3 c011 = float3(offsetX - 1, offsetY, offsetZ - 1);
	float3 c101 = float3(offsetX, offsetY - 1, offsetZ - 1);
	float3 c111 = float3(offsetX - 1, offsetY - 1, offsetZ - 1);

	float v000 = dot(g000, c000) * 0.5 + 0.5;
	float v010 = dot(g010, c010) * 0.5 + 0.5;
	float v100 = dot(g100, c100) * 0.5 + 0.5;
	float v110 = dot(g110, c110) * 0.5 + 0.5;
	float v001 = dot(g001, c001) * 0.5 + 0.5;
	float v011 = dot(g011, c011) * 0.5 + 0.5;
	float v101 = dot(g101, c101) * 0.5 + 0.5;
	float v111 = dot(g111, c111) * 0.5 + 0.5;

	float tx = smooth(offsetX);
	float ty = smooth(offsetY);
	float tz = smooth(offsetZ);

	return lerp(
		lerp(lerp(v000, v010, tx), lerp(v100, v110, tx), ty),
		lerp(lerp(v001, v011, tx), lerp(v101, v111, tx), ty),
		tz);
}