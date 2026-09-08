matrix WorldViewProjection;

float Time;
float GlowStrength;

texture RenderTargetTexture;

sampler ImplicitTexture = sampler_state
{
    Texture = <RenderTargetTexture>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;

    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float dist = abs(input.TexCoord.x);

    // 非常窄的高亮核心。
    // 不再把整条电弧刷成白色。
    float core =
        saturate(1.0 - dist * 3.0);

    core = pow(core, 2.6);

    // 红色主体辉光。
    float glow =
        1.0 / (1.0 + dist * 7.0);

    // 很宽但很淡的外围红光。
    float outerGlow =
        1.0 / (1.0 + dist * 18.0);

    // 完全取消高频 flicker。
    // 电流亮度稳定，只依靠几何折线表现“电”。
    float intensity =
        core * 1.55 +
        glow * GlowStrength * 0.78 +
        outerGlow * GlowStrength * 0.16;

    // 让边缘自然衰减。
    float alphaCutoff =
        smoothstep(
            0.96,
            0.12,
            dist
        );

    float3 finalColor =
        input.Color.rgb * intensity;

    // 只有最中心的一小条区域接近白色，
    // 保留原版 Arc Surge 的红白电弧观感。
    finalColor +=
        float3(1.0, 0.88, 0.88) *
        core *
        0.92;

    finalColor = saturate(finalColor);

    return float4(
        finalColor * alphaCutoff,
        input.Color.a * alphaCutoff
    );
}

technique LightningTechnique
{
    pass Pass1
    {
        VertexShader =
            compile vs_2_0 MainVS();

        PixelShader =
            compile ps_2_0 MainPS();
    }
}
