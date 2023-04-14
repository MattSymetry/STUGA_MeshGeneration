
#pragma multi_compile _ _ADDITIONAL_LIGHTS
#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
#pragma multi_compile _ _ADDITIONAL_LIGHT_CALCULATE_SHADOWS
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS_CASCADE
#pragma multi_compile _ _SHADOWS_SOFT


float3 calcSimpleLambertShading(float3 direction, float3 color, float distanceAtten, float shadowAtten, float3 normalWS) 
{
	//return saturate(dot(normalize(normalWS), direction)) * color * (distanceAtten * shadowAtten);
	return saturate(1) * color * (distanceAtten * shadowAtten);
}

float3 calcSpecular(float3 normalWS, float3 direction, float3 viewDirectionWS, float smoothness, float distanceAtten, float shadowAtten, float3 col)
{
	float3 radiance = col * (distanceAtten * shadowAtten);
	float diffuse = saturate(dot(normalWS, direction));
	float specularDot = saturate(dot(normalWS, normalize(direction + viewDirectionWS)));
	float specular = pow(specularDot, exp2(10 * smoothness)) * diffuse;

	float3 color = radiance * (diffuse + specular) * smoothness;

	return color;
}

float calcDistanceAtten(float dA, bool useFalloff, float baseBrightness, float maxBrightness) {
	float distanceAtten = dA;
	if (distanceAtten > 0.001) {
		distanceAtten += baseBrightness;
	}

	if (distanceAtten < baseBrightness) {
		distanceAtten = baseBrightness;
    }
	else if (distanceAtten > maxBrightness) {
		distanceAtten = maxBrightness;
    }

	return distanceAtten;
}

void GetAllLights_float(float3 positionWS, float3 normalWS, float3 viewDirectionWS, float smoothness, bool useFalloff, float BaseBrightness, float MaxBrightness, out float3 lambert, out float3 specular)
{
#if SHADERGRAPH_PREVIEW
	float3 direction = float3(0.5, 0.5, 0);
	float3 color = 1;
	float distanceAtten = 1;
	float shadowAtten = 1;
	lambert = saturate(dot(normalize(normalWS), direction)) * color * (distanceAtten * shadowAtten);
	specular = 0;
#else
	Light mainLight = GetMainLight();
	float3 direction = mainLight.direction;
	float3 color = mainLight.color;
	float distanceAtten = calcDistanceAtten(mainLight.distanceAttenuation, useFalloff, BaseBrightness, MaxBrightness);
	
	float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
	half shadowAtten = MainLightRealtimeShadow(shadowCoord);

	lambert = calcSimpleLambertShading(direction, color, distanceAtten, shadowAtten, normalWS);
	specular = calcSpecular(normalWS, direction, viewDirectionWS, smoothness, distanceAtten, shadowAtten, color);

#ifdef _ADDITIONAL_LIGHTS
	uint numAdditionalLights = GetAdditionalLightsCount();
	uint usedLights = 1;
	for (uint lightI = 0; lightI < numAdditionalLights; lightI++) {
		int perObjectLightIndex = GetPerObjectLightIndex(lightI);
        Light light = GetAdditionalPerObjectLight(perObjectLightIndex, positionWS);
		//Light light = GetAdditionalLight(lightI, positionWS);
		float3 direction = light.direction;
		float3 color = light.color;
		float distanceAtten = calcDistanceAtten(light.distanceAttenuation, useFalloff, BaseBrightness, MaxBrightness);

		half shadowAtten = AdditionalLightRealtimeShadow(perObjectLightIndex, positionWS);
		//half shadowAtten = AdditionalLightRealtimeShadow(lightI, positionWS, direction);
		//half shadowAtten = light.shadowAttenuation;
		float3 lambertTmp = calcSimpleLambertShading(direction, color, distanceAtten, shadowAtten, normalWS);
		lambert += lambertTmp;
		specular += calcSpecular(normalWS, direction, viewDirectionWS, smoothness, distanceAtten, shadowAtten, color);
	}
	//lambert = lambert / usedLights;
#endif
#endif
}