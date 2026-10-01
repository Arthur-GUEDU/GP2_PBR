#version 460 core

#include "Assets/Shaders/Library/Core.glsl"

#define PI 3.1415926535897932384626433832795

out vec4 FragColor;

in VertexData {
    vec3 Normal;
    vec2 TexCoord0;
    vec3 WorldPos;
} vs_out;

uniform sampler2D textureSampler;

uniform float metallic;
uniform float roughness;
uniform float baseReflectance;
uniform float ambientOcclusion;
uniform vec4 albedo;
uniform bool visibility;

float D_GGX(float NoH, float a) 
{
    float a2 = a * a;
    float f = (NoH * a2 - NoH) * NoH + 1.0;
    return a2 / (PI * f * f);
}

vec3 F_Schlick(float u, vec3 f0, float f90) 
{
    return f0 + (vec3(f90) - f0) * pow(clamp(1.0 - u, 0.0, 1.0), 5.0);
}

float GeometrySchlickGGX(float NdotV, float k)
{
    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return nom / denom;
}
  
float GeometrySmith(vec3 N, vec3 V, vec3 L, float k)
{
    float NoV = max(dot(N, V), 0.0);
    float NoL = max(dot(N, L), 0.0);
    float ggx1 = GeometrySchlickGGX(NoV, k);
    float ggx2 = GeometrySchlickGGX(NoL, k);
	
    return ggx1 * ggx2;
}

float V_SmithGGXCorrelated(float NoV, float NoL, float a) 
{
    float a2 = a * a;
    float GGXL = NoV * sqrt((-NoL * a2 + NoL) * NoL + a2);
    float GGXV = NoL * sqrt((-NoV * a2 + NoV) * NoV + a2);
    return 0.5 / ((GGXV + GGXL) + 0.0001);
}

float Fd_Lambert() 
{
    return 1.0 / PI;
}

vec3 BRDF(vec3 v, vec3 l, vec3 n, vec3 texColor) 
{
    vec3 h = normalize(v + l);
    
    float NoV = abs(dot(n, v)) + 1e-5;
    float NoL = clamp(dot(n, l), 0.0, 1.0);
    float NoH = clamp(dot(n, h), 0.0, 1.0);
    float LoH = clamp(dot(l, h), 0.0, 1.0); 
    float VoH = clamp(dot(v, h), 0.0, 1.0);

    vec3 f0 = 0.16 * baseReflectance * baseReflectance * (1.0 - metallic) + texColor * metallic;

    float D = D_GGX(NoH, roughness);
    vec3  F = F_Schlick(VoH, f0, 1);
    float G = GeometrySmith(n, v, l, ((roughness + 1) * (roughness + 1)) / 8);
    
    float V = V_SmithGGXCorrelated(NoV, NoL, roughness * roughness);

    // specular BRDF
    vec3 Fr = (D * F);

    if (visibility)
    {
        Fr *= V;
    }
    else
    {
         Fr *= G / ((4 * NoV * NoL) + 0.0001);
    }

    // diffuse BRDF
    vec3 diffuseColor = (1.0 - metallic) * texColor;
    vec3 Fd = diffuseColor * Fd_Lambert();

    return Fd + Fr;
}

vec3 DirLightCalculus(vec3 l, vec4 lightColor, vec3 n)
{
    float NoL = clamp(dot(n, l), 0.0, 1.0);
    return lightColor.rgb * NoL * lightColor.w;
}

vec3 PointLightCalculus(vec4 lightPos, vec4 lightColor)
{
    float distance = length(lightPos.rgb - vs_out.WorldPos);
    float attenuation = 1.0 / (distance * distance);
    vec3 radiance  = lightColor.rgb * attenuation * lightColor.a; 

    return radiance;
}

void main()
{
    vec3 color = vec3(0);

    vec3 baseColor = texture(textureSampler, vs_out.TexCoord0).rgb * albedo.rgb;

    vec3 v = normalize(engine_CameraPos.rgb - vs_out.WorldPos);
    vec3 n = normalize(vs_out.Normal);
    
    // Directionnal light
    if (engine_DirLight.Direction.w != 0.0)
    {
        vec3 l = normalize(-engine_DirLight.Direction.rgb);
        color += BRDF(v, l, n, baseColor) * DirLightCalculus(l, engine_DirLight.Color, vs_out.Normal) * clamp(dot(n, l), 0, 1);
    }

    // Point light
    if (engine_PointLight.Position.w != 0.0)
    {
        vec3 l = normalize(engine_PointLight.Position.rgb - vs_out.WorldPos);
        color += BRDF(v, l, n, baseColor) * PointLightCalculus(engine_PointLight.Position, engine_PointLight.Color) * clamp(dot(n, l), 0, 1);
    }

    // Ambient occlusion
    vec3 ambient = vec3(0.03) * baseColor * ambientOcclusion;
    color += ambient;

    // Gamma correction
    color /= color + vec3(1.0);
    color = pow(color, vec3(1.0/2.2));

    FragColor = vec4(color, 1);
}
