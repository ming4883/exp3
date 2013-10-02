varying vec2 vUv;
uniform sampler2D baseMap;

void main()
{
    vec4 ret = texture2D( baseMap, vUv );
    ret.xyz = 1.0 - ret.xyz;
    gl_FragColor = ret;
}
