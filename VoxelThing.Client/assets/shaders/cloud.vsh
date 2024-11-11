#version 330 core

layout (location = 0) in vec3 aPos;

out vec3 pos;
out vec2 uv;

uniform mat4 viewProj;
uniform vec3 offset;
uniform vec4 uvRange;

uniform float cloudHeight = 96.5;

void main() {
    vec3 worldPos = vec3(-512.0, cloudHeight - offset.y, -512.0);
    worldPos += (aPos * 1024.0);

    vec2 uvOffset = offset.xz / 4096.0;
    gl_Position = viewProj * vec4(worldPos, 1.0);

    pos = worldPos;
    uv = mix(uvRange.xy + uvOffset, uvRange.zw + uvOffset, aPos.xz);
}