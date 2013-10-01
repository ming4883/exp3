varying vec2 vUv;
uniform sampler2D baseMap;

void main()
{
    gl_FragColor = texture2D( baseMap, vUv );
    //gl_FragColor = vec4( 1.0, 0.0, 0.0, 1.0 );
}
