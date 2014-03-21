//-----------------------------------------------------------------------------
// Billboard.fx
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Camera parameters.
float4x4 View;
float4x4 Projection;


// Lighting parameters.
float3 LightDirection;
float3 LightColor;
float3 AmbientColor;
float AmbientPower;


// Parameters controlling the wind effect.
float3 WindDirection = float3(1, 0, 0);
float WindWaveSize = 0.1;
float WindRandomness = 1;
float WindSpeed = 4;
float WindAmount;
float WindTime;


// Parameters describing the billboard itself.
float BillboardWidth;
float BillboardHeight;

texture Texture0;
texture Texture1;
texture Texture2;


sampler Texture0Sampler = sampler_state
{
    Texture = (Texture0);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};


sampler Texture1Sampler = sampler_state
{
    Texture = (Texture1);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};


sampler Texture2Sampler = sampler_state
{
    Texture = (Texture2);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VS_INPUT
{
    float3 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float4 IDHeightWidthWind: COLOR0;
    float Random : TEXCOORD1;
};


struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};


VS_OUTPUT VertexShader2(VS_INPUT input)
{
    VS_OUTPUT output;

    // Apply a scaling factor to make some of the billboards
    // shorter and fatter while others are taller and thinner.
    float squishFactor = 0.75 + abs(input.Random) / 2;

    float width = BillboardWidth * squishFactor;
    float height = BillboardHeight / squishFactor;

    // Flip half of the billboards from left to right. This gives visual variety
    // even though we are actually just repeating the same texture over and over.
    if (input.Random < 0)
        width = -width;

    // Work out what direction we are viewing the billboard from.
    float3 viewDirection = View._m02_m12_m22;

    float3 rightVector = normalize(cross(viewDirection, input.Normal));

    // Calculate the position of this billboard vertex.
    float3 position = input.Position;

    // Offset to the left or right.
    position += rightVector * (input.TexCoord.x - 0.5) * input.IDHeightWidthWind.b;
    
    // Offset upward if we are one of the top two vertices.
    position += input.Normal * (1 - input.TexCoord.y) * input.IDHeightWidthWind.g;

    // Work out how this vertex should be affected by the wind effect.
    float waveOffset = dot(position, WindDirection) * WindWaveSize;
    
    waveOffset += input.Random * WindRandomness;
    
    // Wind makes things wave back and forth in a sine wave pattern.
    float wind = sin(WindTime * WindSpeed + waveOffset) * input.IDHeightWidthWind.a;
    
    // But it should only affect the top two vertices of the billboard!
    wind *= (1 - input.TexCoord.y);
    
    position += WindDirection * wind;

    // Apply the camera transform.
    float4 viewPosition = mul(float4(position, 1), View);

    output.Position = mul(viewPosition, Projection);

    output.TexCoord = input.TexCoord;
    
    // Compute lighting.
    float diffuseLight = max(-dot(input.Normal, LightDirection), 0);
    
    //output.Color.rgb = diffuseLight * LightColor + AmbientColor;
    output.Color.rgb = diffuseLight * (LightColor * AmbientPower) + (AmbientColor * AmbientPower);
    output.Color.a = 1;
    
    return output;
}

float4 PixelShader2(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	if ( color.r = 0 )
		return tex2D(Texture0Sampler, texCoord) * color;
    else if ( color.r = 1 )
		return tex2D(Texture1Sampler, texCoord) * color;
	else
		return tex2D(Texture2Sampler, texCoord) * color;
    
}


technique Billboards
{
    // We use a two-pass technique to render alpha blended geometry with almost-correct
    // depth sorting. The only way to make blending truly proper for alpha objects is
    // to draw everything in sorted order, but manually sorting all our billboards
    // would be very expensive. Instead, we draw our billboards in two passes.
    //
    // The first pass has alpha blending turned off, alpha testing set to only accept
    // 100% opaque pixels, and the depth buffer turned on. Because this is only
    // rendering the fully solid parts of each billboard, the depth buffer works as
    // normal to give correct sorting, but obviously only part of each billboard will
    // be rendered.
    //
    // Then in the second pass we enable alpha blending, set alpha test to only accept
    // pixels with fractional alpha values, and set the depth buffer to test against
    // the existing data but not to write new depth values. This means the translucent
    // areas of each billboard will be sorted correctly against the depth buffer
    // information that was previously written while drawing the opaque parts, although
    // there can still be sorting errors between the translucent areas of different
    // billboards.
    //
    // In practice, sorting errors between translucent pixels tend not to be too
    // noticable as long as the opaque pixels are sorted correctly, so this technique
    // often looks ok, and is much faster than trying to sort everything 100%
    // correctly. It is particularly effective for organic textures like grass and
    // trees.
    
    pass RenderOpaquePixels
    {
        VertexShader = compile vs_1_1 VertexShader2();
        PixelShader = compile ps_2_0 PixelShader2();

        AlphaBlendEnable = false;
        
        //AlphaTestEnable = true;
        //AlphaFunc = Equal;
        //AlphaRef = 255;
        
        ZEnable = true;
        ZWriteEnable = true;

        CullMode = None;
    }

    pass RenderAlphaBlendedFringes
    {
        VertexShader = compile vs_1_1 VertexShader2();
        PixelShader = compile ps_2_0 PixelShader2();
        
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
        
        //AlphaTestEnable = true;
        //AlphaFunc = NotEqual;
        //AlphaRef = 255;

        ZEnable = true;
        ZWriteEnable = false;

        CullMode = None;
    }
}
