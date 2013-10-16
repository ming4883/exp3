varying vec3 vWorldPos;

void main()
{
    vec3 distToGridLine = fract( vWorldPos );
    
    if ( distToGridLine.x < 0.5 )
        distToGridLine.x = 1.0 - distToGridLine.x;
        
    if ( distToGridLine.z < 0.5 )
        distToGridLine.z = 1.0 - distToGridLine.z;
    
    float f = max( distToGridLine.x, distToGridLine.z );
    f = smoothstep( 0.90, 0.95, f );
    gl_FragColor = vec4( f, f, f, 1.0 );
}
