struct FieldCell
{
	float value;
	float3 position;
};

struct FieldVertex
{
	float3 position;
};

float3 VertexInterp(float isolevel, float3 p1, float3 p2, float val1, float val2)
{
	float mu = clamp((isolevel - val1) / (val2 - val1), 0, 1);
	return lerp(p1, p2, mu);
}