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

sampler diffuse
{
    Texture = (diffuse);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler shadowMap
{
    Texture = (shadowMap);
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
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

float getDiffuseComponent(float3 worldLightVector, float3 worldVertexPosition, float3 worldNormal)
{
    worldLightVector = normalize(worldLightVector);
    worldNormal = normalize(worldNormal);
    float d = dot(worldLightVector, worldNormal);
    return clamp(d, 0, 1);
}

float getSpotFactor(float3 worldLightVector, float3 worldLightDirection, float spotCutoffCos, float spotExponent)
{
    worldLightVector = normalize(worldLightVector);
    worldLightDirection = normalize(-worldLightDirection);
    float d = dot(worldLightVector, worldLightDirection);
    if (d < spotCutoffCos)
    {
        d = 0;
    }
    return pow(d, spotExponent);
}

float4 getIsOutsideShadow(float3 worldVertexPosition)
{
    float isInsideTexture = 1;
    float isCloserThanMap = 1;

    float4 lightVertexPosition = mul(float4(worldVertexPosition, 1), mul(LightView, LightProjection));
    lightVertexPosition.xyz /= lightVertexPosition.w;
    lightVertexPosition.z = 1 - lightVertexPosition.z;

    float2 shadowMapUV = lightVertexPosition.xy * 0.5 + 0.5;
    shadowMapUV.y = 1 - shadowMapUV.y;
    if (shadowMapUV.x < 0 || shadowMapUV.x > 1 || shadowMapUV.y < 0 || shadowMapUV.y > 1)
    {
        isInsideTexture = 0;
    }

    float shadowMapDepth = tex2D(shadowMap, shadowMapUV).r;
    if (lightVertexPosition.z + 0.1 < shadowMapDepth)
    {
        isCloserThanMap = 0;
    }

    return float4(isInsideTexture * isCloserThanMap, shadowMapDepth, shadowMapUV.x, shadowMapUV.y);
}

float attenuate(float contribution, float3 worldLightVector)
{
    float distance = sqrt(dot(worldLightVector, worldLightVector));
    return contribution / (LightConstantAttenuation + LightLinearAttenuation * distance + LightQuadraticAttenuation * distance * distance);
}

float4 PerPixelPSSpotLight(PerPixelVSOutput input, in bool isFrontFacing : SV_IsFrontFace) : SV_Target
{
    float3 worldLightVector = LightWorldPosition - input.WorldPosition;
    float4 diffuseColour = tex2D(diffuse, input.UV);
    float diffuseContribution = getDiffuseComponent(worldLightVector, input.WorldPosition, input.WorldNormal);
    float spotFactor = getSpotFactor(worldLightVector, LightWorldDirection, LightSpotCutoffCos, LightSpotExponent);
    float4 unshadowed = getIsOutsideShadow(input.WorldPosition);
    return unshadowed.x * float(isFrontFacing) * attenuate(diffuseContribution * spotFactor, worldLightVector) * diffuseColour;
    return isFrontFacing * (diffuseColour * unshadowed.x + unshadowed.x * float4(lerp(0, 1, unshadowed.z), lerp(0, 1, 1 - unshadowed.z), lerp(0, 1, unshadowed.w), 1));
}

void PassthroughVS(inout float4 position : SV_Position, inout float2 uv : TEXCOORD0)
{
    matrix mvp = mul(mul(World, View), Projection);
    position = mul(position, mvp);
}

float4 AmbientPS(in float4 position : SV_Position, in float2 uv : TEXCOORD0, in bool isFrontFacing : SV_IsFrontFace) : SV_Target
{
    return isFrontFacing ? 0.3 * tex2D(diffuse, uv) : 0.1;
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
