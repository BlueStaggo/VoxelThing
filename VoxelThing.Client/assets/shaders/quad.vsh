#version 330 core

layout (location = 0) in vec2 aPos;

out vec2 uv;

uniform mat4 viewProj;
uniform vec2 offset;
uniform vec2 size;
uniform vec2 align;
uniform vec4 uvRange;
uniform float screenScale;

void main() {
    gl_Position = viewProj * vec4(floor((aPos * size + offset) * screenScale) / screenScale, 0.0, 1.0);
    uv = mix(uvRange.xy, uvRange.zw, aPos.xy);
}