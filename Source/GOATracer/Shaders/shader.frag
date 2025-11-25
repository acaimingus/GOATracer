#version 300 es
precision mediump float;

out vec4 FragColor;
in vec2 TexCoord;

uniform sampler2D texture0;

void main()
{
    FragColor = vec4(1.0, 1.0, 1.0, 1.0);
}