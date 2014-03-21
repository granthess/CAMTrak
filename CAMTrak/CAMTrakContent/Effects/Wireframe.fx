float4x4 WorldViewProjection		: WORLDVIEWPROJECTION;
float4 Ambient_Color				: AMBIENT_COLOR;

bool UsingClippingPlane				: USING_CLIPPING_PLANE;
float4 ClippingPlane				: CLIPPING_PLANE;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position			: POSITION0;
	float4 ClipDistances	: TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);

	output.ClipDistances = dot(input.Position, ClippingPlane);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	if ( UsingClippingPlane	)
	{
		clip(input.ClipDistances);
	}

    return Ambient_Color;
}

technique Color
{
    pass Color
    {
        CullMode = None;
        FillMode = Wireframe;

        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
