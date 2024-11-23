#version 330 core

#define M_PI 3.14159265358979323846

in vec3 pos;

out vec4 fColor;

uniform vec4 fogCol;
uniform vec4 skyCol;

void main() {
    float baseDist = sqrt(pos.x * pos.x + pos.z * pos.z);
    float angle = atan(baseDist, pos.y) / M_PI;
    angle = (angle - 0.5) * -4.0;
    fColor = mix(fogCol, skyCol, clamp(angle, 0.0, 1.0));
}