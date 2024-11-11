#version 330 core

#define PI 3.14159265359

in vec3 pos;
in vec3 color;
in vec2 uv;

out vec4 fColor;

uniform sampler2D tex;
uniform bool hasTex = true;
uniform float fade;
uniform vec3 camPos;

#include "modules/fog.fsh"

void main() {
    if (texture(tex, uv).a < 0.5) discard;
    float fog = getFogFromPos(pos - camPos);
    fog += (1.0 - fog) * fade;

    vec4 worldColor = vec4(color, 1.0);
    if (hasTex) worldColor *= texture(tex, uv);
    fColor = blendFogWithSky(worldColor, fog);
}
