#version 330 core

in vec3 pos;
in vec2 uv;

out vec4 fColor;

uniform sampler2D tex;

#include "modules/fog.fsh"

void main() {
    vec4 cloudColor = texture(tex, uv);
    if (cloudColor.a == 0) discard;

    float fog = getFogFromPos(pos);
    fColor = vec4(cloudColor.xyz, cloudColor.a * (1.0 - fog));
}