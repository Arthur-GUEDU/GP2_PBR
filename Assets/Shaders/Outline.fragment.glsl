#version 460 core

#include "Assets/Shaders/Library/Core.glsl"

out vec4 FragColor;

uniform vec4 OutlineColor;

in VertexData {
    vec3 Normal;
    vec2 TexCoord0;
} vs_out;

void main()
{
    FragColor = OutlineColor;
}