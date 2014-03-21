float4x4 ViewMatrix					: VIEW;
float4x4 ProjectionMatrix			: PROJECTION;
float4x4 ViewProjection				: VIEW_PROJECTION;
float4x4 WorldMatrix				: WORLD;
float4x4 WorldViewProjection		: WORLD_VIEW_PROJECTION;
float4x4 ReflectionView				: REFLECTION_VIEW;
float4x4 WindDirection				: WIND_DIRECTION;
float TotalTime						: TIME;
float Reflectivity					: REFLECTIVITY;
float4 SpecularColor				: SPECULAR_COLOR;
float SpecularPower					: SPECULAR_POWER;
float4 AmbientColor					: AMBIENT_COLOR;
float4 DiffuseColor					: DIFFUSE_COLOR;
float4 LightDir						: LIGHT_DIRECTION;
float4 CameraPos					: VIEW_POS;
float4 CameraForward				: VIEW_FORWARD;
float FogNear						: FOG_NEAR;
float FogFar						: FOG_FAR;
float FogDistanceRange				: FOG_DISTANCE_RANGE;
float4 FogColor						: FOG_COLOR;
float FogAltitude					: FOG_ALTITUDE;
float FogThinning					: FOG_THINNING;
float AmbientPower = 0.5f;

float MinTerrainHeight				: MIN_TERRAIN_HEIGHT;
float MaxTerrainHeight				: MAX_TERRAIN_HEIGHT;
float TerrainHeightRange			: TERRAIN_HEIGHT_RANGE;
float TerrainElevationModifier		: TERRAIN_ELEVATION_MODIFIER;
float TerrainScale					: TERRAIN_SCALE;
float TerrainWidth					: TERRAIN_WIDTH;

float WaveHeight					: WAVE_HEIGHT;
float WindForce						: WIND_FORCE;
float WaveLength					: WAVE_LENGTH;

float4 WaterColor					: WATER_COLOR;

//===========================================================================

texture CubeMap		: CUBE_MAP;
samplerCUBE CubeMapSampler = sampler_state
{  
    Texture = (CubeMap); 
    MipFilter = LINEAR; 
    MinFilter = LINEAR; 
    MagFilter = LINEAR; 
    AddressU  = Wrap;
    AddressV  = Wrap; 
    AddressW  = Wrap;
};

Texture BumpTexture			: NORMAL_MAP;
sampler2D BumpMapSampler : TEXUNIT1 = sampler_state
{ Texture   = (BumpTexture); magfilter = LINEAR; minfilter = LINEAR; 
                             mipfilter = LINEAR; AddressU  = Wrap;
                             AddressV  = Wrap; AddressW  = Wrap; };
                             
Texture ReflectionMap		: REFLECTION_MAP;
sampler ReflectionSampler = sampler_state { texture = <ReflectionMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture RefractionMap		: REFRACTION_MAP;
sampler RefractionSampler = sampler_state { texture = <RefractionMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture HeightMap			: HEIGHT_MAP;
sampler HeightMapSampler = sampler_state { texture = <HeightMap> ; magfilter = LINEAR; minfilter = LINEAR; 
                                                                         mipfilter = LINEAR; AddressU  = Wrap;
                                                                         AddressV  = Wrap; AddressW  = Wrap;};

//==================================================================================                        
// TECHNIQUE: Water
// This technique uses refraction and reflection
// CREDIT: Most of this shader was written by Riemer Grootjans, http://www.riemers.net                 

struct VS_INPUT
 {
     float4 Position            : POSITION0;
     float2 TextureCoords       : TEXCOORD0;
 };

struct VS_OUTPUT
{
    float4 Position             : POSITION;
    float2 TextureCoords        : TEXCOORD0;
    float3 Normal				: TEXCOORD1;
    float2 TextureCoordsAbs		: TEXCOORD2;    
	float3 CameraPos			: TEXCOORD3;   
    
    float4 RefractionMap        : TEXCOORD5;     
    float4 ReflectionMap		: TEXCOORD6; 
    float3 WorldPos				: TEXCOORD7;
	
};

VS_OUTPUT VertexShaderFunction( VS_INPUT input )
{
	VS_OUTPUT Output;
  
    float4 worldSpacePos = mul(input.Position, WorldMatrix);

	// When we use WorldViewProjection passed into this shader the refractions get messed up...
    float4x4 WorldViewProjection = mul(WorldMatrix, ViewProjection);
    
    Output.RefractionMap = mul(input.Position, WorldViewProjection);
    
    float4x4 WorldReflectionViewProjection = mul (WorldMatrix, mul (ReflectionView, ProjectionMatrix));
    Output.ReflectionMap = mul(input.Position, WorldReflectionViewProjection);

	float3 Binormal;
	float3 Tangent;	
    
    Output.Position = mul(worldSpacePos, ViewProjection);    // transform Position    
    
    float4 absoluteTexCoords = float4(input.TextureCoords, 0, 1);	
    float4 rotatedTexCoords = mul(absoluteTexCoords, WindDirection);
    
    float2 moveVector = float2(0, 1);
    Output.TextureCoords = rotatedTexCoords.xy / WaveLength + TotalTime * WindForce * moveVector.xy;    
    
    Output.TextureCoordsAbs.x = input.Position.x / (TerrainWidth * TerrainScale);
    Output.TextureCoordsAbs.y = input.Position.z / (TerrainWidth * TerrainScale);
    
    Output.Normal = float3(0, 1, 0);

	Output.CameraPos = CameraPos;
	
	float3 eyePosition = mul(-ViewMatrix._m30_m31_m32, transpose(ViewMatrix));
    if ( eyePosition.y < worldSpacePos.y )
	{		
		Output.CameraPos.y = worldSpacePos.y - (eyePosition.y - worldSpacePos.y);
	}

    Output.WorldPos = worldSpacePos;

    return Output;
} 
 
float4 PixelShaderFunction( VS_OUTPUT Input ) : COLOR
{	 
	float3 bumpColor = tex2D(BumpMapSampler, Input.TextureCoords);  
        
    float2 perturbation = WaveHeight * (bumpColor.rg - 0.5f);
 
	float2 ProjectedRefrTexCoords;
    ProjectedRefrTexCoords.x = (Input.RefractionMap.x / Input.RefractionMap.w) / 2.0f + 0.5f;
    ProjectedRefrTexCoords.y = (-Input.RefractionMap.y / Input.RefractionMap.w) / 2.0f + 0.5f;
    float2 perturbatedRefrTexCoords = ProjectedRefrTexCoords + perturbation;
    float4 refractiveColor = tex2D(RefractionSampler, perturbatedRefrTexCoords);
    
    float2 ProjectedTexCoords;
    ProjectedTexCoords.x = (Input.ReflectionMap.x / Input.ReflectionMap.w) / 2.0f + 0.5f;
    ProjectedTexCoords.y = (-Input.ReflectionMap.y / Input.ReflectionMap.w) / 2.0f + 0.5f;
    float2 perturbatedTexCoords = ProjectedTexCoords + perturbation;     
    float4 reflectiveColor = tex2D(ReflectionSampler, perturbatedTexCoords); 

    float3 CamToWorldPos = float3(Input.CameraPos - Input.WorldPos);
    float d = length(CamToWorldPos);
	float inverseLength = 1 / d;
	CamToWorldPos *= inverseLength;	// Normalize

	float3 Binormal = Input.Normal;
	float3 Tangent = Input.Normal;

	float3 TSBumpColor = (bumpColor.rgb - 1.0f);
    TSBumpColor = mul(TSBumpColor, float3x3(Binormal, Input.Normal, Tangent)) * 0.5f; 

    float3 BumpWithNormal = (TSBumpColor + Input.Normal);
    
    float fresnelTerm = clamp(dot(CamToWorldPos, BumpWithNormal), 0, 0.9);    

    float3 Reflect = reflect(LightDir, BumpWithNormal);
    float4 specular = pow(saturate(dot(Reflect, CamToWorldPos)), SpecularPower);
    float4 SpecColor = (reflectiveColor + (SpecularColor * specular));

	float3 TerrainHeight = tex2D(HeightMapSampler, Input.TextureCoordsAbs);
	
	// We do the same calculation on the heightmap that we did to figure out the heights in the code,
	// except we don't do any of the terrain smoothing here, but this should be close enough to give
	// us a usable shoreline.
	float HeightAfterCalc = (TerrainHeight.r * 255 + TerrainHeight.g * 255 + TerrainHeight.b * 255);
	float heightAtZX = ( ((HeightAfterCalc - MinTerrainHeight) / TerrainHeightRange) * TerrainElevationModifier) * TerrainScale;
	
	// Only brighten shallow water
	if ( (Input.WorldPos.y - heightAtZX) < 25.0f )
	{
		float falloff = ((Input.WorldPos.y - heightAtZX) * 0.03f);
		refractiveColor.xyz += 0.75f - falloff;
		refractiveColor.xyz = saturate(refractiveColor.xyz);
	}	

    float4 combinedColor = ( refractiveColor * fresnelTerm ) + ( SpecColor * (1 - fresnelTerm) * Reflectivity );	

	float l = saturate((d - FogNear) / FogDistanceRange / clamp( (Input.WorldPos.y / FogAltitude) + 1, 1, FogThinning));

	return lerp(combinedColor, FogColor, l);
}

technique Water
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

float4 PixelShaderFunctionNoShoreline( VS_OUTPUT Input ) : COLOR
{	 
	float3 bumpColor = tex2D(BumpMapSampler, Input.TextureCoords);  
        
    float2 perturbation = WaveHeight * (bumpColor.rg - 0.5f);
 
	float2 ProjectedRefrTexCoords;
    ProjectedRefrTexCoords.x = (Input.RefractionMap.x / Input.RefractionMap.w) / 2.0f + 0.5f;
    ProjectedRefrTexCoords.y = (-Input.RefractionMap.y / Input.RefractionMap.w) / 2.0f + 0.5f;
    float2 perturbatedRefrTexCoords = ProjectedRefrTexCoords + perturbation;
    float4 refractiveColor = tex2D(RefractionSampler, perturbatedRefrTexCoords);
    
    float2 ProjectedTexCoords;
    ProjectedTexCoords.x = (Input.ReflectionMap.x / Input.ReflectionMap.w) / 2.0f + 0.5f;
    ProjectedTexCoords.y = (-Input.ReflectionMap.y / Input.ReflectionMap.w) / 2.0f + 0.5f;
    float2 perturbatedTexCoords = ProjectedTexCoords + perturbation;     
    float4 reflectiveColor = tex2D(ReflectionSampler, perturbatedTexCoords); 

    float3 CamToWorldPos = float3(Input.CameraPos - Input.WorldPos);
    float d = length(CamToWorldPos);
	float inverseLength = 1 / d;
	CamToWorldPos *= inverseLength;	// Normalize

	float3 Binormal = Input.Normal;
	//Binormal.xy = 0;
	float3 Tangent = Input.Normal;
	//Tangent.yz = 0;

	float3 TSBumpColor = (bumpColor.rgb - 1.0f);
    TSBumpColor = mul(TSBumpColor, float3x3(Binormal, Input.Normal, Tangent)) * 0.5f; 

    float3 BumpWithNormal = (TSBumpColor + Input.Normal);
    
    float fresnelTerm = clamp(dot(CamToWorldPos, BumpWithNormal), 0, 0.9);    

    float3 Reflect = reflect(LightDir, BumpWithNormal);
    float4 specular = pow(saturate(dot(Reflect, CamToWorldPos)), SpecularPower);
    float4 SpecColor = (reflectiveColor + (SpecularColor * specular));

	float3 TerrainHeight = tex2D(HeightMapSampler, Input.TextureCoordsAbs);
	
	// We do the same calculation on the heightmap that we did to figure out the heights in the code,
	// except we don't do any of the terrain smoothing here, but this should be close enough to give
	// us a usable shoreline.
	float HeightAfterCalc = (TerrainHeight.r * 255 + TerrainHeight.g * 255 + TerrainHeight.b * 255);
	float heightAtZX = ( ((HeightAfterCalc - MinTerrainHeight) / TerrainHeightRange) * TerrainElevationModifier) * TerrainScale;

	float waterDepth = clamp(Input.WorldPos.y - heightAtZX, 0, 200);
	float depthPerc = ((200 - waterDepth) / 200);
	depthPerc = clamp(depthPerc, 0.0f, 0.1f);
	
	// Only brighten shallow water
	if ( (Input.WorldPos.y - heightAtZX) < 25.0f )
	{
		float falloff = ((Input.WorldPos.y - heightAtZX) * 0.03f);
		refractiveColor.xyz += 0.75f - falloff;
		refractiveColor.xyz = saturate(refractiveColor.xyz);
	}	

    float4 combinedColor = ( refractiveColor * fresnelTerm ) + ( SpecColor * (1 - fresnelTerm) * 0.5f );	

	float l = saturate((d - FogNear) / FogDistanceRange / clamp( (Input.WorldPos.y / FogAltitude) + 1, 1, FogThinning));

	return lerp(combinedColor, FogColor, l);
}

technique WaterNoShoreline
{
    pass P0
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunctionNoShoreline();
    }
}

//==================================================================================                        
// TECHNIQUE: WaterReflectOnly
// This technique uses refraction and doesn't calculate terrain shorelines
// CREDIT: Most of this shader was written by Riemer Grootjans, http://www.riemers.net                 

struct NO_REFRACT_VS_OUTPUT
{
    float4 Position             : POSITION;
    float2 TextureCoords        : TEXCOORD0;
    float3 Normal		        : TEXCOORD1;  
    float3 Binormal			    : TEXCOORD2;
    float3 Tangent			    : TEXCOORD3;     
    float4 ReflectionMap		: TEXCOORD4; 
    float3 WorldPos				: TEXCOORD5;    
};

NO_REFRACT_VS_OUTPUT No_Refract_VertexShader( VS_INPUT input )
{
	NO_REFRACT_VS_OUTPUT Output;
  
    float4 worldSpacePos = mul(input.Position, WorldMatrix);

    float4x4 WorldReflectionViewProjection = mul(WorldMatrix, mul(ReflectionView, ProjectionMatrix));
    Output.ReflectionMap = mul(input.Position, WorldReflectionViewProjection);

	float3 Binormal;
	float3 Tangent;
	
	float4 eyePosition = mul(-ViewMatrix._m30_m31_m32, transpose(ViewMatrix));
	
	Output.Normal = float3(0, 1, 0);
    Binormal = (0.0f, 0.0f, 1.0f);
	Tangent = (1.0f, 0.0f, 0.0f);
    
    if ( eyePosition.y < worldSpacePos.y )
	{
		Binormal = (0.0f, 0.0f, -1.0f);
		Tangent = (-1.0f, 0.0f, 0.0f);
		Output.Normal = float3(0, -1, 0);		
	}
	
    Output.Tangent = mul(Tangent, WorldMatrix);
    Output.Binormal = mul(Binormal, WorldMatrix);
    
    Output.Position = mul(worldSpacePos, ViewProjection);    // transform Position    
    
    float4 absoluteTexCoords = float4(input.TextureCoords, 0, 1);	
    float4 rotatedTexCoords = mul(absoluteTexCoords, WindDirection);
    
    float2 moveVector = float2(0, 1);
    Output.TextureCoords = rotatedTexCoords.xy / WaveLength + TotalTime * WindForce * moveVector.xy;
    
    Output.WorldPos = worldSpacePos;

    return Output;
} 
 
float4 No_Refract_PixelShader( NO_REFRACT_VS_OUTPUT Input ) : COLOR
{	
    float3 bumpColor = tex2D(BumpMapSampler, Input.TextureCoords);  
        
    float2 perturbation = WaveHeight * (bumpColor.rg - 0.5f);
    
    float3 TSBumpColor = (2.0f * bumpColor.rgb - 1.0f);
    TSBumpColor = mul(TSBumpColor, float3x3(Input.Binormal, Input.Normal, Input.Tangent)) * 0.5f; 

    float2 ProjectedTexCoords;
    ProjectedTexCoords.x = (Input.ReflectionMap.x / Input.ReflectionMap.w) / 2.0f + 0.5f;
    ProjectedTexCoords.y = (-Input.ReflectionMap.y / Input.ReflectionMap.w) / 2.0f + 0.5f;
    float2 perturbatedTexCoords = ProjectedTexCoords + perturbation;     
    float4 reflectiveColor = tex2D(ReflectionSampler, perturbatedTexCoords); 
        
	float4 eyePosition = mul(-ViewMatrix._m30_m31_m32, transpose(ViewMatrix));
	if ( eyePosition.y < Input.WorldPos.y )
	{
		eyePosition.y = Input.WorldPos.y - (eyePosition.y - Input.WorldPos.y);
	}

    float3 CamToWorldPos = float3(eyePosition - Input.WorldPos);
    float d = length(CamToWorldPos);
    CamToWorldPos = normalize(CamToWorldPos);

	float3 BumpWithNormal = TSBumpColor + Input.Normal;

    float fresnelTerm = clamp(dot(CamToWorldPos, BumpWithNormal), 0, 0.9); 

    float3 Reflect = reflect(LightDir, BumpWithNormal);
    float4 specular = pow(saturate(dot(Reflect, CamToWorldPos)), SpecularPower);
    float4 SpecColor = (reflectiveColor + (SpecularColor * specular));

    float4 combinedColor = (WaterColor * fresnelTerm) + (SpecColor * (1 - fresnelTerm));
    combinedColor.a = 0.85f;
    
    float l = saturate((d - FogNear) / (FogFar - FogNear) / clamp(Input.WorldPos.y / FogAltitude + 1, 1, FogThinning));
	 
	return lerp(combinedColor, FogColor, l);
}

technique WaterReflectOnlyNoShoreline
{
    pass P0
    {
        VertexShader = compile vs_1_1 No_Refract_VertexShader();
        PixelShader = compile ps_2_0 No_Refract_PixelShader();
    }
}