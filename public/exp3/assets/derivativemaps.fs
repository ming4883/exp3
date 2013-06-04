varying vec2 v_txc;
uniform sampler2D baseMap;

void main()
{
    vec4 baseColor = texture2D( baseMap, v_txc );
    
    float baseLuma = dot( baseColor.rgb, vec3( 0.2126, 0.7152, 0.0722 ) );
    
    gl_FragColor = vec4( vec3( baseLuma ), 1.0 );
}
