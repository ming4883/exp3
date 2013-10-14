uniform vec3 uCameraPos;
varying vec3 vWorldPos;

void main()
{
    vWorldPos = vec3( uCameraPos.x, 0, uCameraPos.z - 5.0 ) + position * 10.0;
    vec4 mvPosition = modelViewMatrix * vec4( vWorldPos, 1.0 );
    gl_Position = projectionMatrix * mvPosition;
}
