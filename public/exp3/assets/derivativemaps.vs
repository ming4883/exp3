varying vec2 v_txc;

void main()
{
    v_txc = uv;
    
    vec4 mvPosition = modelViewMatrix * vec4( position, 1.0 );
    gl_Position = projectionMatrix * mvPosition;
}
