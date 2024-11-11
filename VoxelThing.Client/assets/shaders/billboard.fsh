#version 330 core

in vec3 fPos;
in vec2 uv;

out vec4 fColor;

uniform sampler2D tex;
uniform bool hasTex;
uniform vec4 color;

#include "modules/fog.fsh"

void main() {
    if (texture(tex, uv).a == 0.0) discard;
    float fog = getFogFromPos(fPos);

    vec4 worldColor = color;
    if (hasTex) worldColor *= texture(tex, uv);
    fColor = blendFogWithSky(worldColor, fog);
}
