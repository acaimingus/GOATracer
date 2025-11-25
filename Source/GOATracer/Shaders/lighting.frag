#version 300 es
precision mediump float;
precision mediump int;

#define MAX_LIGHTS 128

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};

struct Light {
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

uniform Light lights[MAX_LIGHTS];
uniform int activeLightCount;

uniform Material material;
uniform vec3 viewPos;

in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoord;

uniform sampler2D texture0;
uniform int hasTexture;

out vec4 FragColor;

void main()
{
    vec3 ambient = vec3(0.0);
    vec3 diffuse = vec3(0.0);
    vec3 specular = vec3(0.0);

    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    vec3 texColor;
    if (hasTexture == 1) {
        texColor = texture(texture0, TexCoord).rgb;
    } else {
        texColor = vec3(1.0, 1.0, 1.0);
    }
    
    for(int i = 0; i < activeLightCount; i++)
    {
        // Ambient
        ambient += lights[i].ambient * material.ambient;

        // Diffuse
        vec3 lightDir = normalize(lights[i].position - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);
        diffuse += lights[i].diffuse * (diff * material.diffuse * texColor);

        // Specular
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
        specular += lights[i].specular * (spec * material.specular);
    }

    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0);
}