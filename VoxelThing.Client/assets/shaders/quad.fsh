#version 330 core

in vec2 uv;

out vec4 fColor;

uniform sampler2D tex;
uniform bool hasTex;
uniform vec4 color;

void main() {
    if (hasTex && texture(tex, uv).a == 0.0) discard;
    vec4 screenColor = color;
    if (hasTex) screenColor *= texture(tex, uv);
    fColor = screenColor;
}
