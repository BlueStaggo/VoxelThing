#version 330 core

in vec3 pos;

out vec4 fColor;

uniform vec4 fogCol;
uniform vec4 skyCol;

void main() {
    fColor = mix(fogCol, skyCol, (pos.y * 2.0));
}