float4x4 worldViewProjection	: WORLDVIEWPROJECTION;

texture diffuseMap				: DIFFUSE_MAP;

bool UsingClippingPlane		: USING_CLIPPING_PLANE;
float4 ClippingPlane		: CLIPPING_PLANE;

sampler diffuseSampler = sampler_state
{
	Texture = (diffuseMap);
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

struct VertexShaderInput
{
    float4 position		: POSITION0;
	float2 texCoords	: TEXCOORD0;
	float3 normal		: NORMAL0;
	float3 binormal		: BINORMAL0;
	float3 tangent		: TANGENT0;
};

struct VertexShaderOutput
{
    float4 position		: POSITION0;
    float2 texCoords	: TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

	output.position = mul(input.position, worldViewProjection);
	output.texCoords = input.texCoords;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return tex2D(diffuseSampler, input.texCoords);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
