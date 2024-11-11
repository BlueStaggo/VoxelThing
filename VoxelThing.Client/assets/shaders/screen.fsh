#version 330 core

in vec3 color;
in vec2 uv;

out vec4 fColor;

uniform sampler2D tex;
uniform bool hasTex = true;

void main() {
    if (hasTex && texture(tex, uv).a == 0.0) discard;
    vec4 screenColor = vec4(color, 1.0);
    if (hasTex) screenColor *= texture(tex, uv);
    fColor = screenColor;
}
