#define VS_SHADERMODEL vs_4_0

#define PS_SHADERMODEL ps_4_0

matrix World;
matrix View;
matrix Projection;
matrix LightView;
matrix LightProjection;
float3 LightWorldPosition;
float3 LightWorldDirection;
float LightSpotCutoffCos;
float LightSpotExponent;
float LightConstantAttenuation;
float LightLinearAttenuation;
float LightQuadraticAttenuation;

texture2D wallDiffuse;
sampler2D wallSampler = sampler_state
{
    Texture = <wallDiffuse>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D shadowMap;
sampler2D shadowMapSampler = sampler_state
{
    Texture = <shadowMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct PerPixelVSInput
{
    float4 Position : SV_Position;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
};

struct PerPixelVSOutput
{
    float4 Position : SV_Position;
    float2 UV : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
};

PerPixelVSOutput PerPixelVS(in PerPixelVSInput input)
{
	PerPixelVSOutput output = (PerPixelVSOutput)0;

    matrix wvp = mul(mul(World, View), Projection);

    output.Position = mul(input.Position, wvp);
    output.UV = input.UV;
    float4 worldPosition = mul(input.Position, World);
    output.WorldPosition = worldPosition / worldPosition.w;
    output.WorldNormal = mul(float4(input.Normal, 0), World);

	return output;
}

float4 PerPixelPSSpotLight(PerPixelVSOutput input, in bool isFrontFacing : SV_IsFrontFace) : SV_Target
{
    if (!isFrontFacing)
    {
        return 0;
    }
    float4 lightRelativePosition = mul(float4(input.WorldPosition, 1), LightView);
    lightRelativePosition = mul(lightRelativePosition, LightProjection);
    lightRelativePosition.xyz /= lightRelativePosition.w;
    lightRelativePosition.z = 1 - lightRelativePosition.z;
    float2 shadowMapCoords = lightRelativePosition.xy * 0.5 + float2(0.5, 0.5);
    shadowMapCoords.y = 1 - shadowMapCoords.y;
    if (saturate(shadowMapCoords.x) != shadowMapCoords.x || saturate(shadowMapCoords.y) != shadowMapCoords.y)
    {
        return 0;
    }
    float shadowMapDepth = tex2D(shadowMapSampler, shadowMapCoords).r;
    if (lightRelativePosition.z < shadowMapDepth - 0.0001)
    {
        return 0;
    }

    float3 worldSpaceLightVector = LightWorldPosition - input.WorldPosition;
    float lightDot = clamp(dot(normalize(worldSpaceLightVector), normalize(input.WorldNormal)), 0, 1);
    float coneDot = clamp(dot(normalize(worldSpaceLightVector), normalize(-LightWorldDirection)), 0, 1);
    coneDot = coneDot < LightSpotCutoffCos ? 0 : coneDot;
    coneDot = pow(coneDot, LightSpotExponent);
    lightDot = lightDot * coneDot;
    float distance = sqrt(dot(worldSpaceLightVector, worldSpaceLightVector));
    lightDot = lightDot / (LightConstantAttenuation + LightLinearAttenuation * distance + LightQuadraticAttenuation * distance * distance);
    float lightContribution = lightDot;
    float4 diffuse = tex2D(wallSampler, input.UV);
    return lightContribution * diffuse;
}

void PassthroughVS(inout float4 position : SV_Position, inout float2 uv : TEXCOORD0)
{
    matrix mvp = mul(mul(World, View), Projection);
    position = mul(position, mvp);
}

float4 AmbientPS(in float4 position : SV_Position, in float2 uv : TEXCOORD0, in bool isFrontFacing : SV_IsFrontFace) : SV_Target
{
    return isFrontFacing ? 0.1 * tex2D(wallSampler, uv) : 0.1;
}

struct VS_OUT_SHADOW
{
    float4 Position : SV_Position;
    float Depth : TEXCOORD0;
};

VS_OUT_SHADOW SpotShadowVS(float4 position : SV_Position)
{
    VS_OUT_SHADOW output = (VS_OUT_SHADOW)0;
    matrix vp = mul(LightView, LightProjection);
    output.Position = mul(position, vp);
    output.Depth = output.Position.z / output.Position.w;
    return output;
}

float4 SpotShadowPS(VS_OUT_SHADOW input) : SV_Target
{
    return float4(1 - input.Depth, 0, 0, 1);
}

technique Ambient
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PassthroughVS();
        PixelShader = compile PS_SHADERMODEL AmbientPS();
    }
}

technique Spot
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PerPixelVS();
        PixelShader = compile PS_SHADERMODEL PerPixelPSSpotLight();
    }
}

technique SpotShadow
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL SpotShadowVS();
        PixelShader = compile PS_SHADERMODEL SpotShadowPS();
    }
}
