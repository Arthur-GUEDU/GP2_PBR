#version 460 core

#include "Assets/Shaders/Library/Core.glsl"
#include "Assets/Shaders/Library/SpaceTransforms.glsl"

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

out vec3 FragPos;
out vec2 TexCoords;
out vec3 Normal;

uniform mat4 _LocalToWorld;

void main()
{
    vec3 worldPosition = (_LocalToWorld * vec4(aPos, 1.0)).xyz;

    FragPos = worldPosition;
    Normal = normalize((_LocalToWorld * vec4(aNormal, 0.0)).xyz);
    TexCoords = aTexCoords;
    
    gl_Position = TransformWorldToClip(worldPosition); 
}