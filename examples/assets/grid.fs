varying vec3 vWorldPos;
varying vec4 vNDCPos;

void main()
{
    vec3 distToGridLine = fract( vWorldPos );
    
    if ( distToGridLine.x < 0.5 )
        distToGridLine.x = 1.0 - distToGridLine.x;
        
    if ( distToGridLine.z < 0.5 )
        distToGridLine.z = 1.0 - distToGridLine.z;
    
    float f = max( distToGridLine.x, distToGridLine.z );
    f = smoothstep( 0.90, 0.95, f );
    
    float d = vNDCPos.z / vNDCPos.w;
    d = clamp( d - 0.25, 0.0, 1.0 ) / 0.625;
    d = 1.0 - clamp( d, 0.0, 1.0 );
    f *= d;
    gl_FragColor = vec4( f, f, f, 1.0 );
}
