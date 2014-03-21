float4x4 World					: WORLD;
float4x4 worldViewProjection	: WORLDVIEWPROJECTION;
float4x4 LightViewProjection	: LIGHTVIEWPROJECTION;

bool UsingClippingPlane		: USING_CLIPPING_PLANE;
float4 ClippingPlane		: CLIPPING_PLANE;


struct VertexShaderInput
{
    float4 position		: POSITION0;
	float2 texCoord		: TEXCOORD0;
	float3 normal		: NORMAL0;
	float3 binormal		: BINORMAL0;
	float3 tangent		: TANGENT0;
};

struct VertexShaderOutput
{
    float4 position : POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

	output.position = mul(input.position, worldViewProjection);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return float4(1, 0, 0, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
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