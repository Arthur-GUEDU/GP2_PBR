#version 460 core

#include "Assets/Shaders/Library/Core.glsl"
#include "Assets/Shaders/Library/SpaceTransforms.glsl"

layout (location = 0) in vec3 InPosition;
layout (location = 1) in vec3 InNormal;
layout (location = 2) in vec2 InTexCoord0;

out VertexData {
    vec3 Normal;
    vec2 TexCoord0;
} vs_out;

uniform mat4 _LocalToWorld;
uniform float _outline;

void main()
{
    vs_out.Normal = normalize((_LocalToWorld * vec4(InNormal, 0.0)).xyz);
    vs_out.TexCoord0 = InTexCoord0;

    vec3 worldPosition = (_LocalToWorld * vec4(InPosition + InNormal * _outline, 1.0)).xyz;
    gl_Position = TransformWorldToClip(worldPosition); 
}