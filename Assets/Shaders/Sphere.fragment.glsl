#version 460 core

#include "Assets/Shaders/Library/Core.glsl"

out vec4 FragColor;

uniform vec4 _Color;
uniform sampler2D _BaseColor;

in VertexData {
    vec3 Normal;
    vec2 TexCoord0;
} vs_out;

uniform int isToon;
uniform int toonColorLevel;

uniform int isGooch;
uniform float goochB = 0.5;
uniform float goochY = 0.5;
uniform float goochAlpha = 0.5;
uniform float goochBeta = 0.5;
uniform vec4 goochCoolColorBase;
uniform vec4 goochWarmColorBase;

vec4 _ModelColor = vec4(0.0, 0.0, 1.0, 0.0);

void main()
{
    if (isToon == 1)
    {
        float lightIntensity = 0.5 + 0.5 * dot(engine_DirLight.Direction.rgb, vs_out.Normal);
        float diffuseFactor = floor(lightIntensity * (toonColorLevel + 1)) * (1.0 / (toonColorLevel + 1));
        FragColor = _Color * texture(_BaseColor, vs_out.TexCoord0) * diffuseFactor;
    }
    else if (isGooch == 1)
    {

        vec3 coolColor = goochB * vec3(goochCoolColorBase) + goochAlpha * vec3(_ModelColor);
        vec3 warmColor = goochY * vec3(goochWarmColorBase) + goochBeta * vec3(_ModelColor);
        
        float diffuse = dot(engine_DirLight.Direction.rgb, vs_out.Normal) * 0.5 + 0.5;
        vec3 resultColor = mix(coolColor, warmColor, diffuse);
        FragColor = vec4(resultColor, 1.0);
    }
    else 
    {
        //FragColor = texture(_BaseColor, vs_out.TexCoord0);
        FragColor = _Color;
    }
}