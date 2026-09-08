matrix WorldViewProjection;
float Time;
float GlowStrength;

struct VSInput
{
    float4 Position : POSITION0;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    return output;
}

float4 PixelShaderFunction(VSOutput input) : COLOR0
{
    float distanceFromCenter = abs(input.TexCoord.x);

    // 主体：红色外层 + 明亮核心。
    float outer =
        1.0 - smoothstep(0.05, 1.0, distanceFromCenter);

    float core =
        1.0 - smoothstep(0.01, 0.35, distanceFromCenter);

    // 很轻微的时间变化，避免旧版本那种高频白色闪烁。
    float pulse =
        0.97 +
        0.03 * sin(
            Time * 7.0 +
            input.TexCoord.y * 5.0
        );

    float intensity =
        outer * pulse +
        core * 2.6;

    float3 red =
        input.Color.rgb *
        (1.15 + intensity * GlowStrength * 0.45);

    // 核心只少量加入白色，不再把整条闪电变成白色。
    red += core.xxx * 0.72;

    float alpha =
        saturate(
            outer *
            input.Color.a *
            (0.72 + core * 0.45)
        );

    return float4(red, alpha);
}

technique LightningTechnique
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
