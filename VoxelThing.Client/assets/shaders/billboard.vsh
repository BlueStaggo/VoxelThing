#version 330 core

layout (location = 0) in vec3 aPos;

out vec3 fPos;
out vec2 uv;

uniform mat4 modelView;
uniform mat4 projection;
uniform vec3 position;
uniform vec2 size;
uniform vec2 align;
uniform vec4 uvRange;

void main() {
    gl_Position = projection * modelView * vec4((aPos.xy - align) * size, aPos.z, 1.0);
    fPos = vec3((aPos.xy - align) * size, aPos.z) + position;
    uv = mix(uvRange.xw, uvRange.zy, aPos.xy);
}