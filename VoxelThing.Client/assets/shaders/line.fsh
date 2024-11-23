#version 330 core

uniform vec4 color;

out vec4 oColor;

void main() {
    if (color.a == 0.0) discard;
    oColor = color;
}