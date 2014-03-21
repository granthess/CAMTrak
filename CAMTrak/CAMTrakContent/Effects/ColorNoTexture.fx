float4x4 World					: WORLD;
float4x4 View					: VIEW;
float4x4 Projection				: PROJECTION;
float4x4 WorldViewProjection	: WORLDVIEWPROJECTION;
float4x4 LightViewProjection	: LIGHTVIEWPROJECTION;
float4 LightDir					: LIGHT_DIRECTION;
float4 SpecularColor			: SPECULAR_COLOR;
float4 SpecularPower			: SPECULAR_POWER;
float4 AmbientColor				: AMBIENT_COLOR;
float4 DiffuseColor				: DIFFUSE_COLOR;
float4 CameraForward			: VIEW_FORWARD;
float4 CameraPos				: VIEW_POS;
float FogNear					: FOG_NEAR;
float FogFar					: FOG_FAR;
float4 FogColor					: FOG_COLOR;
float FogAltitude				: FOG_ALTITUDE;
float FogThinning				: FOG_THINNING;
float4 ModelColor				: MODEL_COLOR;

float WaterElevation			: WATER_ELEVATION;
float4 WaterColorLight			: WATER_COLOR_LIGHT;

bool UsingClippingPlane			: USING_CLIPPING_PLANE;
float4 ClippingPlane			: CLIPPING_PLANE;

float DepthBias = 0.0015f;

Texture ShadowMap				: SHADOW_MAP;	
sampler ShadowMapSampler = sampler_state { texture = <ShadowMap>; MinFilter = POINT; MagFilter = POINT;
																  MipFilter = NONE;  AddressU = Clamp;
																  AddressV = Clamp;	 AddressW  = Wrap; };

// For any shader that can support shadow mapping, simply add 
// this function and call it from your pixel shader. Of course,
// you'll need to make sure this shader and material are setup to
// receive the ShadowMap sampler, and any needed parameters (e.g. LightViewProjection)															  
float4 ComputeShadowColor(float4 worldPos, float4 Color)
{
     // Find the position of this pixel in light space
    float4 lightingPosition = mul(worldPos, LightViewProjection);
    
    // Find the position in the shadow map for this pixel
    float2 ShadowTexCoord = 0.5 * lightingPosition.xy / 
                            lightingPosition.w + float2( 0.5, 0.5 );
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

    // Get the current depth stored in the shadow map
    float4 shadowInfo = tex2D(ShadowMapSampler, ShadowTexCoord);
    float shadowdepth = shadowInfo.r;
    float shadowOpacity = 0.5f + 0.5f * (1 - shadowInfo.g);
    
    // Calculate the current pixel depth
    // The bias is used to prevent folating point errors that occur when
    // the pixel of the occluder is being drawn
    float ourdepth = (lightingPosition.z / lightingPosition.w) - DepthBias;

    // Check to see if this pixel is in front or behind the value in the shadow map
    if ( shadowdepth < ourdepth)
    {
        // Shadow the pixel by lowering the intensity
        Color *= float4(shadowOpacity, shadowOpacity, shadowOpacity, 1);
    };
    
    return Color;
}


struct VertexShaderInput
{
    float4 Position		: POSITION0;
    float3 Normal		: NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position			: POSITION0;
    float3 Normal			: TEXCOORD0;
    float4 WorldPos			: TEXCOORD1;
	float4 ClipDistances	: TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Position = mul(input.Position, WorldViewProjection);
    output.WorldPos = mul(input.Position, World);
    output.Normal = normalize(mul(input.Normal, World));

	output.ClipDistances = dot(output.WorldPos, ClippingPlane);
	
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	if ( UsingClippingPlane )
	{
		clip(input.ClipDistances);
	}

	float4 CamToWorldPos = float4(input.WorldPos - CameraPos);
	float4 Half = normalize(-LightDir + normalize(CamToWorldPos));
    float3 Specular = pow(saturate(dot(input.Normal, Half)), SpecularPower);
    
    float LightingFactor = saturate(dot(-LightDir ,input.Normal));

	float4 Color = ModelColor;	
    Color.rgb *= AmbientColor + (DiffuseColor * LightingFactor) + (SpecularColor * Specular);    
    
    // Handle fog differently if there is water, and also handle if the camera is underwater
	float waterTerrainDiff = WaterElevation - input.WorldPos.y;
	
	float d = length(input.WorldPos - CameraPos);

	float fogValue = FogThinning;
	float fogAlt = FogAltitude;
	float fogNearDist = FogNear;
	float fogFarDist = FogFar;
	float4 fogColor = FogColor;

	// If this pixel on the mesh is underwater, and the water elevation is not the default value
	if ( waterTerrainDiff > 0 )
    {
		 if ( CameraPos.y < WaterElevation )
		 {
			fogValue = 1;	// Setting FogThinning to 1 makes elevation-based fog become distance-based
			fogAlt = WaterElevation;
			fogNearDist = -200;
			fogFarDist = 250;
		 }
		 else
		 {		
			fogValue = 1;	// Setting FogThinning to 1 makes elevation-based fog become distance-based
			fogAlt = WaterElevation;
			fogNearDist = 0;
			fogFarDist = 150;
			d = waterTerrainDiff;
		 }
		
		 fogColor = WaterColorLight;
	}
    
    Color = ComputeShadowColor(input.WorldPos, Color);
		
    float l = saturate((d - fogNearDist) / (fogFarDist - fogNearDist) / clamp(input.WorldPos.y / fogAlt + 1, 1, fogValue));

	return lerp(Color, fogColor, l);
}

technique VertColorNoTexture
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

//------- Technique: VertColorNoTextureNoShadow --------

float4 PixelShaderNoShadowFunction(VertexShaderOutput input) : COLOR0
{
	if ( UsingClippingPlane )
	{
		clip(input.ClipDistances);
	}

	float4 CamToWorldPos = float4(input.WorldPos - CameraPos);
	float4 Half = normalize(-LightDir + normalize(CamToWorldPos));
    float3 Specular = pow(saturate(dot(input.Normal, Half)), SpecularPower);
    
    float LightingFactor = saturate(dot(-LightDir ,input.Normal));

	float4 Color = ModelColor;	
    Color.rgb *= AmbientColor + (DiffuseColor * LightingFactor) + (SpecularColor * Specular);    
    
    // Handle fog differently if there is water, and also handle if the camera is underwater
	float waterTerrainDiff = WaterElevation - input.WorldPos.y;
	
	float d = length(input.WorldPos - CameraPos);

	float fogValue = FogThinning;
	float fogAlt = FogAltitude;
	float fogNearDist = FogNear;
	float fogFarDist = FogFar;
	float4 fogColor = FogColor;

	// If this pixel on the mesh is underwater, and the water elevation is not the default value
	if ( waterTerrainDiff > 0 )
    {
		 if ( CameraPos.y < WaterElevation )
		 {
			fogValue = 1;	// Setting FogThinning to 1 makes elevation-based fog become distance-based
			fogAlt = WaterElevation;
			fogNearDist = -200;
			fogFarDist = 250;
		 }
		 else
		 {		
			fogValue = 1;	// Setting FogThinning to 1 makes elevation-based fog become distance-based
			fogAlt = WaterElevation;
			fogNearDist = 0;
			fogFarDist = 150;
			d = waterTerrainDiff;
		 }
		
		 fogColor = WaterColorLight;
	}

    float l = saturate((d - fogNearDist) / (fogFarDist - fogNearDist) / clamp(input.WorldPos.y / fogAlt + 1, 1, fogValue));

	return lerp(Color, fogColor, l);
}

technique VertColorNoTextureNoShadow
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderNoShadowFunction();
    }
}

// =================================================================
// This section is for shadow mapping. This must be in any shader that
// is part of shadow mapping.

struct DrawWithShadowMap_VSIn
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
};

struct CreateShadowMap_VSOut
{
    float4 Position : POSITION;
    float Depth     : TEXCOORD0;
	float4 ClipDistances	: TEXCOORD1;
};

// Transforms the model into light space an renders out the depth of the object
CreateShadowMap_VSOut CreateShadowMap_VertexShader(DrawWithShadowMap_VSIn input)
{
    CreateShadowMap_VSOut Out;
    Out.Position = mul(input.Position, mul(World, LightViewProjection));      
    Out.Depth = Out.Position.z / Out.Position.w;

	Out.ClipDistances = dot(input.Position, ClippingPlane);

    return Out;
}

// Saves the depth value out to the 32bit floating point texture
float4 CreateShadowMap_PixelShader(CreateShadowMap_VSOut input) : COLOR
{ 
	if ( UsingClippingPlane )
	{
		clip(input.ClipDistances);
	}

    return float4(input.Depth, ModelColor.a, 0, 1);
}

// Technique for creating the shadow map
technique CreateShadowMap
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 CreateShadowMap_VertexShader();
        PixelShader = compile ps_2_0 CreateShadowMap_PixelShader();
    }
}
// =================================================================
