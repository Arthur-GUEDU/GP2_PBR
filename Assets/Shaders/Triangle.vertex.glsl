#version 460 core

layout (location = 0) in vec3 InPosition;
layout (location = 1) in vec3 InNormal;
layout (location = 2) in vec2 InTexCoord0;

void main()
{
    gl_Position = vec4(InPosition, 1);
}