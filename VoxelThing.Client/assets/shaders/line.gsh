#version 330 core

layout(lines) in;
layout(triangle_strip, max_vertices = 4) out;

uniform float thickness = 1.0;
uniform vec2 viewportSize;

vec2 toScreenSpace(vec4 position) {
    return vec2(position.xy / position.w) * viewportSize;
}

void main() {
    vec4 a = gl_in[0].gl_Position;
    vec4 b = gl_in[1].gl_Position;
    vec2 a2d = toScreenSpace(a);
    vec2 b2d = toScreenSpace(b);
    vec2 dir = normalize(b2d - a2d);
    vec2 normal = vec2(-dir.y, dir.x);
    
    gl_Position = vec4((a2d + normal * thickness) / viewportSize * a.w, a.z - 0.001, a.w);
    EmitVertex();
    gl_Position = vec4((a2d - normal * thickness) / viewportSize * a.w, a.z - 0.001, a.w);
    EmitVertex();
    gl_Position = vec4((b2d + normal * thickness) / viewportSize * b.w, b.z - 0.001, b.w);
    EmitVertex();
    gl_Position = vec4((b2d - normal * thickness) / viewportSize * b.w, b.z - 0.001, b.w);
    EmitVertex();
    EndPrimitive();
}