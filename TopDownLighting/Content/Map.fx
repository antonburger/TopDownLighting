#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0

//matrix WorldViewProjection;
matrix World;
matrix View;
matrix Projection;
float3 LightWorldPosition;
float3 LightWorldDirection;
float LightTightness;
float LightBrightness;

texture floorDiffuse;
texture wallDiffuse;

sampler2D floorSampler = sampler_state
{
    Texture = <floorDiffuse>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct PerVertexVSInput
{
	float4 Position : SV_Position;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD;
};

struct PerVertexVSOutput
{
	float4 Position : SV_Position;
	float4 Color : COLOR0;
    float2 UV : TEXCOORD;
};

struct PerPixelVSInput
{
    float4 Position : SV_Position;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD;
};

struct PerPixelVSOutput
{
    float4 Position : SV_Position;
    float2 UV : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
};

PerVertexVSOutput PerVertexVS(in PerVertexVSInput input)
{
	PerVertexVSOutput output = (PerVertexVSOutput)0;

    matrix wvp = mul(mul(World, View), Projection);

    float4 worldSpaceVertex = mul(input.Position, World);
    worldSpaceVertex = worldSpaceVertex / worldSpaceVertex.w;
    float3 worldSpaceNormal = mul(float4(input.Normal, 0), World);

    float3 worldSpaceLightVector = LightWorldPosition - worldSpaceVertex.xyz;
    float lightDot = clamp(dot(normalize(worldSpaceLightVector), normalize(worldSpaceNormal)), 0, 1);
    float distance = clamp(dot(worldSpaceLightVector, worldSpaceLightVector), 1, 1000);
    float lightContribution = lightDot / distance;

	output.Position = mul(input.Position, wvp);
    output.Color = float4((float3)clamp(0.05 + lightContribution, 0, 1), float(1));
    output.UV = input.UV;
	return output;
}

float4 PerVertexPS(PerVertexVSOutput input, in bool isFrontFacing : SV_IsFrontFace) : SV_Target
{
    float4 diffuse = tex2D(floorSampler, input.UV);
    return isFrontFacing ? input.Color * diffuse : 0.1f;
}

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

float4 PerPixelPSPointLight(PerPixelVSOutput input, in bool isFrontFacing : SV_IsFrontFace) : SV_Target
{
    float3 worldSpaceLightVector = LightWorldPosition - input.WorldPosition;
    float lightDot = clamp(dot(normalize(worldSpaceLightVector), normalize(input.WorldNormal)), 0, 1);
    float distance = clamp(dot(worldSpaceLightVector, worldSpaceLightVector), 1, 1000);
    float lightContribution = clamp(0.05 + LightBrightness * lightDot / distance, 0, 1);
    float4 diffuse = tex2D(floorSampler, input.UV);
    return isFrontFacing ? lightContribution * diffuse : 0.1f;
}

float4 PerPixelPSConeLight(PerPixelVSOutput input, in bool isFrontFacing : SV_IsFrontFace) : SV_Target
{
    float3 worldSpaceLightVector = LightWorldPosition - input.WorldPosition;
    float lightDot = clamp(dot(normalize(worldSpaceLightVector), normalize(input.WorldNormal)), 0, 1);
    float coneDot = clamp(dot(normalize(worldSpaceLightVector), normalize(-LightWorldDirection)), 0, 1);
    coneDot = pow(coneDot, LightTightness);
    lightDot = lightDot * coneDot;
    float distance = clamp(dot(worldSpaceLightVector, worldSpaceLightVector), 1, 1000);
    float lightContribution = clamp(0.05 + LightBrightness * lightDot / distance, 0, 1);
    float4 diffuse = tex2D(floorSampler, input.UV);
    return isFrontFacing ? lightContribution * diffuse : 0.1f;
}

technique PerVertex
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL PerVertexVS();
		PixelShader = compile PS_SHADERMODEL PerVertexPS();
	}
};

technique PerPixelPointLight
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PerPixelVS();
        PixelShader = compile PS_SHADERMODEL PerPixelPSPointLight();
    }
}

technique PerPixelConeLight
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PerPixelVS();
        PixelShader = compile PS_SHADERMODEL PerPixelPSConeLight();
    }
}