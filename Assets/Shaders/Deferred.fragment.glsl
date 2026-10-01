#version 460 core

out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedoSpec;

struct Light {
    vec3 Position;
    vec3 Color;
    
    float Linear;
    float Quadratic;
};
//const int NR_LIGHTS = 32;
//uniform Light lights[NR_LIGHTS];
Light light = {vec3(0.5, 0.0, 0.0),
                vec3(1.0, 1.0, 1.0),
                0.7,
                1.8};

uniform vec3 viewPos;

void main()
{             
    // retrieve data from gbuffer
    vec3 FragPos = texture(gPosition, TexCoords * 2).rgb;
    vec3 Normal = texture(gNormal, TexCoords * 2).rgb;
    vec3 Diffuse = texture(gAlbedoSpec, TexCoords * 2).rgb;
    float Specular = texture(gAlbedoSpec, TexCoords * 2).a;
    
    if (TexCoords.x <= 0.5 && TexCoords.y > 0.5)
    {   
        FragColor = vec4(FragPos, 1.0);
    }
    else if (TexCoords.x > 0.5 && TexCoords.y > 0.5)
    {   
        FragColor = vec4(Normal, 1.0);
    }
    else if (TexCoords.x <= 0.5 && TexCoords.y <= 0.5)
    {   
        FragColor = vec4(Diffuse, 1.0);
    }
    else
    {
        // then calculate lighting as usual
        vec3 lighting  = Diffuse * 0.1; // hard-coded ambient component
        vec3 viewDir  = normalize(viewPos - FragPos);
        for(int i = 0; i < 1; ++i)
        {
            // diffuse
            vec3 lightDir = normalize(light.Position - FragPos);
            vec3 diffuse = max(dot(Normal, lightDir), 0.0) * Diffuse * light.Color;
            // specular
            vec3 halfwayDir = normalize(lightDir + viewDir);  
            float spec = pow(max(dot(Normal, halfwayDir), 0.0), 16.0);
            vec3 specular = light.Color * spec * Specular;
            // attenuation
            float distance = length(light.Position - FragPos);
            float attenuation = 1.0 / (1.0 + light.Linear * distance + light.Quadratic * distance * distance);
            diffuse *= attenuation;
            specular *= attenuation;
            lighting += diffuse + specular;        
        }
        FragColor = vec4(lighting, 1.0);
    }

    FragColor = mix(FragColor, vec4(1, 1, 1, 1), clamp(max(
        pow(1.0 - 10.0 * abs(TexCoords.y - 0.5), 16),
        pow(1.0 - 10.0 * abs(TexCoords.x - 0.5), 16)
    ), 0, 1));
}