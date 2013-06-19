varying vec2 v_txc;
varying vec3 v_nrm;
varying vec3 v_pos;

void main()
{
    v_txc = uv;
    
    v_nrm = normalMatrix * normal;
    
    v_pos = ( modelMatrix  * vec4( position, 1.0 ) ).xyz;
    
    vec4 mvPosition = modelViewMatrix * vec4( position, 1.0 );
    gl_Position = projectionMatrix * mvPosition;
}
