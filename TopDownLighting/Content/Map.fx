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

sampler normalMap: register(s0)
{
    Texture = (normalMap);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler diffuse: register(s1)
{
    Texture = (diffuse);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler shadowMap: register(s2)
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
    float3 Tangent : TANGENT;
};

struct PerPixelVSOutput
{
    float4 Position : SV_Position;
    float2 UV : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
    float3 WorldTangent : TEXCOORD3;
};

PerPixelVSOutput PerPixelVS(in PerPixelVSInput input)
{
	PerPixelVSOutput output = (PerPixelVSOutput)0;

    matrix wvp = mul(mul(World, View), Projection);

    output.Position = mul(input.Position, wvp).xyzw;
    output.UV = input.UV;
    float4 worldPosition = mul(input.Position, World).xyzw;
    output.WorldPosition = worldPosition / worldPosition.w;
    output.WorldNormal = mul(float4(input.Normal, 0), World).xyz;
    output.WorldTangent = mul(float4(input.Tangent, 0), World).xyz;

	return output;
}

float4 getDiffuseColour(sampler s, float2 uv)
{
    return tex2D(s, uv);
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

float4 getIsOutsideShadow(float3 worldVertexPosition, float3 worldNormal)
{
    float isInsideTexture = 1;
    float isCloserThanMap = 1;

    worldVertexPosition += worldNormal * 0.1;
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
    if (lightVertexPosition.z + 0.05 < shadowMapDepth)
    {
        isCloserThanMap = 0;
    }

    return float4(isInsideTexture * isCloserThanMap, shadowMapDepth, shadowMapUV.x, shadowMapUV.y);
}

float3 calculateBumpedNormal(float3 normal, float3 tangent, float2 uv)
{
    normal = normalize(normal);
    tangent = normalize(tangent - dot(tangent, normal) * normal);
    float3 bitangent = cross(tangent, normal);
    float3x3 tbn = float3x3(tangent, bitangent, normal);
    float3 tangentSpaceNormal = tex2D(normalMap, uv).xyz;
    tangentSpaceNormal = tangentSpaceNormal * 2 - 1;
    float3 newNormal = normalize(mul(tangentSpaceNormal, tbn));
    return newNormal;
}

float attenuate(float contribution, float3 worldLightVector)
{
    float distance = sqrt(dot(worldLightVector, worldLightVector));
    return contribution / (LightConstantAttenuation + LightLinearAttenuation * distance + LightQuadraticAttenuation * distance * distance);
}

float4 PerPixelPSSpotLight(PerPixelVSOutput input, in bool isFrontFacing : SV_IsFrontFace) : SV_Target
{
    float3 normal = input.WorldNormal;
    normal = calculateBumpedNormal(input.WorldNormal, input.WorldTangent, input.UV);
    normal = normalize(0.00001 * normal + input.WorldNormal);
    float3 worldLightVector = LightWorldPosition - input.WorldPosition;
    float4 diffuseColour = getDiffuseColour(diffuse, input.UV);
    float diffuseContribution = getDiffuseComponent(worldLightVector, input.WorldPosition, normal);
    float spotFactor = getSpotFactor(worldLightVector, LightWorldDirection, LightSpotCutoffCos, LightSpotExponent);
    // This still uses surface normal for shadowing determination - helps to reduce moving shadows.
    float4 unshadowed = getIsOutsideShadow(input.WorldPosition, input.WorldNormal);
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
    return isFrontFacing ? 0.000001 * tex2D(normalMap, uv) + 0.2 * getDiffuseColour(diffuse, uv) : 0.1;
}

struct VS_OUT_SHADOW
{
    float4 Position : SV_Position;
    float Depth : TEXCOORD0;
};

VS_OUT_SHADOW SpotShadowVS(float4 position : SV_Position)
{
    VS_OUT_SHADOW output = (VS_OUT_SHADOW)0;
    matrix wvp = mul(mul(World, LightView), LightProjection);
    output.Position = mul(position, wvp);
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
