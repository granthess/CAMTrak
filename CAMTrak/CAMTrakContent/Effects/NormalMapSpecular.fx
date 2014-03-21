float4x4 LightViewProjection	: LIGHTVIEWPROJECTION;
float4x4 WorldViewProjection	: WORLDVIEWPROJECTION;
float4x4 World					: WORLD;
float4x4 view					: VIEW;

float4 LightDir					: LIGHT_DIRECTION;
float4 CameraForward			: VIEW_FORWARD;
float4 CameraPos				: VIEW_POS;

float SpecularPower				: SPECULAR_POWER;
float4 SpecularColor			: SPECULAR_COLOR;

float4 AmbientColor				: AMBIENT_COLOR;

texture diffuseMap				: DIFFUSE_MAP;
texture normalMap				: NORMAL_MAP;

bool UsingClippingPlane			: USING_CLIPPING_PLANE;
float4 ClippingPlane			: CLIPPING_PLANE;

float FogNear				: FOG_NEAR;
float FogFar				: FOG_FAR;
float FogDistanceRange		: FOG_DISTANCE_RANGE;
float4 FogColor				: FOG_COLOR;
float FogAltitude			: FOG_ALTITUDE;
float FogThinning			: FOG_THINNING;

float DepthBias = 0.0015f;

sampler diffuseSampler = sampler_state
{
	Texture = (diffuseMap);
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

sampler normalSampler = sampler_state
{
	Texture = (normalMap);
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

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
    if ( shadowdepth <= ourdepth )
    {
        // Shadow the pixel by lowering the intensity
        Color *= float4(shadowOpacity, shadowOpacity, shadowOpacity, 1);
    };
    
    return Color;
}

struct VertexInput
{
	float4 	position	: POSITION;
	float2 	texCoords	: TEXCOORD0;
	float3	normal		: NORMAL0;
	float3	binormal	: BINORMAL0;
	float3	tangent		: TANGENT0;
};

struct VertexToPixel
{
	float4		position		: POSITION;
	float2		texCoords		: TEXCOORD0;
	float3		lightDir		: TEXCOORD1;
	float3		viewDir			: TEXCOORD2;
	float4      ClipDistances	: TEXCOORD3;
	float4		WorldPos		: TEXCOORD4;
};

VertexToPixel VertexShaderFunction(VertexInput input)
{
	VertexToPixel output;
	
	// Transform vertex by world-view-projection matrix
	output.position = mul(input.position, WorldViewProjection);
	output.WorldPos = mul(input.position, World);
		
	float3 viewPos = mul(-view._m30_m31_m32, transpose(view));
	
	float3 viewDir = viewPos - output.WorldPos.xyz;
	
	float3 normNormal = normalize(input.normal);
	float3 normBinormal = normalize(input.binormal);
	float3 normTangent = normalize(input.tangent);
	
	output.viewDir.x = dot(normTangent, viewDir);
	output.viewDir.y = dot(normBinormal, viewDir);
	output.viewDir.z = dot(normNormal, viewDir);
	
	output.lightDir.x = dot(normTangent, -LightDir);
	output.lightDir.y = dot(normBinormal, -LightDir);
	output.lightDir.z = dot(normNormal, -LightDir);
	
	output.texCoords = input.texCoords;

	output.ClipDistances = dot(output.WorldPos, ClippingPlane);
	
	return output;
}

float4 PixelShaderFunction(VertexToPixel input) : COLOR0
{
	if ( UsingClippingPlane )
	{
		clip(input.ClipDistances);
	}
	
	float4 diffuseColor = tex2D(diffuseSampler, input.texCoords);
	float3 normal = normalize((tex2D(normalSampler, input.texCoords).xyz * 2.0f) - 1.0f);
	
	float3 normLightDir = normalize(input.lightDir);
	float3 normViewDir = normalize(input.viewDir);
	
	float nDotL = dot(normal, normLightDir);
	
	float3 refl = normalize(((2.0f * normal) * nDotL) - normLightDir);
	float rDotV = max(dot(refl, normViewDir), 0);
		
	float4 retColor = (AmbientColor * diffuseColor + (diffuseColor * rDotV) + (SpecularColor * pow(rDotV, SpecularPower)));
	
	// Apply shadow
	float4 finalColor = ComputeShadowColor(input.WorldPos, retColor);

	// Apply fog
	float d = length(input.WorldPos - CameraPos);  
    float l = saturate((d - FogNear) / (FogFar - FogNear) / clamp(input.WorldPos.y / FogAltitude + 1, 1, FogThinning));

    return lerp(finalColor, FogColor, l);
}

technique NormalMapSpecular
{
   pass Single_Pass
   {
		VertexShader = compile vs_1_1 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
   }
}

float4 PixelShaderNoShadowFunction(VertexToPixel input) : COLOR0
{
	if ( UsingClippingPlane )
	{
		clip(input.ClipDistances);
	}

	float4 diffuseColor = tex2D(diffuseSampler, input.texCoords);
	float3 normal = normalize((tex2D(normalSampler, input.texCoords).xyz * 2.0f) - 1.0f);
	
	float3 normLightDir = normalize(input.lightDir);
	float3 normViewDir = normalize(input.viewDir);
	
	float nDotL = dot(normal, normLightDir);
	
	float3 refl = normalize(((2.0f * normal) * nDotL) - normLightDir);
	float rDotV = max(dot(refl, normViewDir), 0);
		
	float4 retColor = (AmbientColor * diffuseColor + (diffuseColor * rDotV) + (SpecularColor * pow(rDotV, SpecularPower)));	

	// Apply fog
	float d = length(input.WorldPos - CameraPos);  
    float l = saturate((d - FogNear) / (FogFar - FogNear) / clamp(input.WorldPos.y / FogAltitude + 1, 1, FogThinning));

    return lerp(retColor, FogColor, l);
}

technique NormalMapSpecularNoShadow
{
   pass Single_Pass
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

    return float4(input.Depth, 1, 0, 1);
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