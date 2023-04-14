
#pragma multi_compile _ _ADDITIONAL_LIGHTS
#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
#pragma multi_compile _ _ADDITIONAL_LIGHT_CALCULATE_SHADOWS
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS_CASCADE
#pragma multi_compile _ _SHADOWS_SOFT


float3 calcSimpleLambertShading(float3 direction, float3 color, float distanceAtten, float shadowAtten, float3 normalWS) 
{
	return saturate(dot(normalize(normalWS), direction)) * color * (distanceAtten * shadowAtten);
}

float3 calcShading(float3 direction, float3 color, float distanceAtten, float shadowAtten, float3 normalWS) 
{
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

void GetAllLights_float(float3 positionWS, float3 normalWS, float3 viewDirectionWS, float smoothness, bool useFalloff, float distanceattenOffset, out float3 lambert, out float3 specular)
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
	float distanceAtten = mainLight.distanceAttenuation;
	if (distanceAtten > 0.001) {
		distanceAtten += distanceattenOffset;
		if (!useFalloff) {distanceAtten += 0.1;}
	}
	
	float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
	half shadowAtten = MainLightRealtimeShadow(shadowCoord);

	if (useFalloff) {
		lambert = calcSimpleLambertShading(direction, color, distanceAtten, shadowAtten, normalWS);
    }
	else {
		lambert = calcShading(direction, color, distanceAtten, shadowAtten, normalWS);
    }
	
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
		float distanceAtten = light.distanceAttenuation;
		if (distanceAtten > 0.001) {
			distanceAtten += distanceattenOffset;
			if (!useFalloff) {distanceAtten += 0.1;}
		}

		half shadowAtten = AdditionalLightRealtimeShadow(perObjectLightIndex, positionWS);
		//half shadowAtten = AdditionalLightRealtimeShadow(lightI, positionWS, direction);
		//half shadowAtten = light.shadowAttenuation;
		float3 lambertTmp = 0;
		if (useFalloff) {
			lambertTmp = calcSimpleLambertShading(direction, color, distanceAtten, shadowAtten, normalWS);
        }
		else {
			lambertTmp = calcShading(direction, color, distanceAtten, shadowAtten, normalWS);
        }
		lambert += lambertTmp;
		specular += calcSpecular(normalWS, direction, viewDirectionWS, smoothness, distanceAtten, shadowAtten, color);
	}
	//lambert = lambert / usedLights;
#endif
#endif
}