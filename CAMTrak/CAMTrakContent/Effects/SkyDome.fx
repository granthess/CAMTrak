float4x4 World						: WORLD;
float4x4 WorldViewProjection		: WORLDVIEWPROJECTION;
float4 CameraPos					: VIEW_POS;
float fogNear						: FOG_NEAR;
float fogFar						: FOG_FAR;
float4 fogColor						: FOG_COLOR;
float fogAltitudeScale				: FOG_ALTITUDE;
float fogThinning					: FOG_THINNING;

float4 AmbientColor					: AMBIENT_COLOR;
float  AmbientPower					: AMBIENT_POWER;

float4 LightDirection				: LIGHT_DIRECTION;
float  LightPower					: LIGHT_POWER;

bool UsingClippingPlane				: USING_CLIPPING_PLANE;
float4 ClippingPlane				: CLIPPING_PLANE;

// -------------- Samplers -------------
Texture xTexture					: TEXTURE;
sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = CLAMP; AddressV = CLAMP;};

//------- Technique: SkyWithLighting --------

struct VertexShaderInput
{
    float4 Position		: POSITION0;
    float3 Normal		: NORMAL0;
    float2 TexCoords	: TEXCOORD0;
};

struct SkyLightingVertexToPixel
{
    float4 Position         : POSITION;    
    float3 Normal           : TEXCOORD0;
    float2 TextureCoords    : TEXCOORD1;
    float4 WorldPos			: TEXCOORD2;
};

SkyLightingVertexToPixel SkyWithLightingVS( VertexShaderInput input )
{
    SkyLightingVertexToPixel output;
    
    output.Position = mul(input.Position, WorldViewProjection);
    
    output.WorldPos = mul(input.Position, World);
    
    // Make sure depth is set to maximum to keep skybox from culling.
    output.Position.z = output.Position.w * 0.999999f;	// We don't want to use 1.0 because it causes occasional blips in the skydome
    
    output.Normal = mul(normalize(input.Normal), World);
    output.TextureCoords = input.TexCoords;
    
    return output;    
}

struct SkyLightingPixelToFrame
{
    float4 Color : COLOR0;
};

SkyLightingPixelToFrame SkyWithLightingPS(SkyLightingVertexToPixel input)
{
    SkyLightingPixelToFrame output;
    
    float lightingFactor = saturate(dot(input.Normal, -LightDirection)) * AmbientPower;
        
    output.Color = (tex2D(TextureSampler, input.TextureCoords) * LightPower) * AmbientColor;
    output.Color.a = 1;
    
    float d = length(input.WorldPos - CameraPos);  
    float l = saturate((d - fogNear) / (fogFar - fogNear) / clamp(input.WorldPos.y / fogAltitudeScale + 1, 1, fogThinning));

    output.Color = lerp(output.Color, fogColor, l);

    return output;
}

technique SkyWithLighting
{
    pass Pass0
    {
        VertexShader = compile vs_1_1 SkyWithLightingVS();
        PixelShader = compile ps_2_0 SkyWithLightingPS();
    }
}

// 'No shadow' technique should always come after the default technique,
// otherwise it is assumed that NoShadow is the default, which causes
// a problem when we later try to append "NoShadow" to the end of it.
technique SkyWithLightingNoShadow
{
pass Pass0
    {
        VertexShader = compile vs_1_1 SkyWithLightingVS();
        PixelShader = compile ps_2_0 SkyWithLightingPS();
    }
}
