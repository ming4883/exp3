uniform mat4 uCameraMatInv;

varying vec3 vWorldPos;

#define VIEW_OFFSET vec3( 0.0, 0.0, -1.0 )
#define VIEW_SCALE vec3( 20.0, 0.0, 10.0 )

void main()
{
    vec3 viewPos = ( position.xyz + VIEW_OFFSET ) * VIEW_SCALE;
    vWorldPos = ( uCameraMatInv * vec4( viewPos, 1.0 ) ).xyz;
    vWorldPos.y = 0.0;
    
    vec4 mvPosition = modelViewMatrix * vec4( vWorldPos, 1.0 );
    gl_Position = projectionMatrix * mvPosition;
}
