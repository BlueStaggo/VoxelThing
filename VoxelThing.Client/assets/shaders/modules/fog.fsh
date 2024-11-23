uniform struct {
    sampler2D skyTex;
    float skyWidth;
    float skyHeight;

    float distHor;
    float distVer;
} fogInfo;

float doFog(float fog) {
    fog = max((fog - 0.2) / 0.8, 0.0);
    fog *= fog;
    return fog;
}

float getFogFromPos(vec3 pos) {
    float fog = clamp(max(sqrt(pos.x * pos.x + pos.z * pos.z) / fogInfo.distHor, abs(pos.y) / fogInfo.distVer), 0.0, 1.0);
    fog = clamp(doFog(fog), 0.0, 1.0);
    return fog;
}

vec4 blendFogWithSky(vec4 color, float fog) {
    vec4 fogColor = texture(fogInfo.skyTex, gl_FragCoord.xy / vec2(fogInfo.skyWidth, fogInfo.skyHeight));
    return mix(color, fogColor, fog);
}
