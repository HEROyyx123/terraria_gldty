matrix WorldViewProjection;
float Time;
float GlowStrength;

// 增加贴图采样（用于采样 RenderTarget 的渲染结果）
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

// 绘制闪电网格体到 RT 时使用的 PS
float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float dist = abs(input.TexCoord.x);
    
    // 1. 核心高能区域
    float core = saturate(1.0 - dist * 1.8);
    core = pow(core, 3.0);

    // 高频流动闪烁
    float flicker = 0.85 + 0.15 * sin(Time * 80.0 + input.TexCoord.y * 50.0);
    
    // 2. 边缘辉光 (Glow)
    float glow = 1.0 / (1.0 + dist * 6.0);
    
    float intensity = (core * 2.0 + glow * GlowStrength) * flicker;
    
    // 渐变衰减：边缘很远的地方强度直接清零，防止微弱残余把屏幕擦亮
    float alphaCutoff = smoothstep(0.9, 0.0, dist);

    float3 finalColor = input.Color.rgb * intensity;
    
    // 核心提亮为白色
    finalColor += float3(core, core, core) * 1.2;

    // 乘以 alphaCutoff 确保边缘外纯黑且 alpha 为 0
    return float4(finalColor * alphaCutoff, input.Color.a * alphaCutoff);
}

technique LightningTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_2_0 MainPS();
    }
}