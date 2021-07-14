#include "noiseCommon.cginc"
#include "noise3.cginc"

float planetNoise(	float3 pos, 
					float3 areaSize, 
					float surfaceFrequency, 
					float surfaceHeight,
					float athmosphereHeight,
					float3 noiseOffset,
					float smoothRange,
					float cavesFrequency)
{
	//Calculate basic properties
	float3 center = areaSize / 2;
	float highRadius = areaSize.x / 2;
	float lowRadius = highRadius - surfaceHeight;

	//Calculate current position height
	float posHeight = length(pos - center);

	//Calculate surface height with noise
	float3 surfacePos = center + normalize(pos - center) * lowRadius * surfaceFrequency;
	float3 noisePos = surfacePos; //+ noiseOffset; //TODO: Do another kind of displacement, noise offset is too high
	float surfaceNoise = pow(noise3(noisePos) * 0.9 + noise3(noisePos * 4) * 0.1, 3);
	float groundHeight = lowRadius + surfaceNoise * surfaceHeight;

	//Flatten
	float plainHeight = surfaceHeight * 0.05;
	groundHeight = max(lowRadius, groundHeight - plainHeight);

	//Interpolate the heights
	float diff = posHeight - groundHeight;
	float noise = 
		diff >= smoothRange ? 0 :
		diff <= -smoothRange ? 1 :
		lerp(1, 0, (diff + smoothRange) / (smoothRange * 2));

	return noise;
}