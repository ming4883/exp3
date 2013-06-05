// reference http://www.rorydriscoll.com/2012/01/11/derivative-maps/

#extension GL_OES_standard_derivatives : enable

varying vec2 v_txc;
varying vec3 v_nrm;
varying vec3 v_pos;
uniform sampler2D heightMap;

vec3 CalculateSurfaceGradient( vec3 n, vec3 dpdx, vec3 dpdy, float dhdx, float dhdy )
{
    vec3 r1 = cross( dpdy, n );
    vec3 r2 = cross( n, dpdx );
 
    return ( r1 * dhdx + r2 * dhdy ) / dot( dpdx, r1 );
}

vec3 PerturbNormal( vec3 n, vec3 dpdx, vec3 dpdy, float dhdx, float dhdy )
{
    return normalize( n - CalculateSurfaceGradient( n, dpdx, dpdy, dhdx, dhdy ) );
}

void main()
{
    float bumpHeight = texture2D( heightMap, v_txc ).r;
    
    vec3 wsNormal = normalize( v_nrm );
    
    vec3 dpdx = dFdx( v_pos );
    vec3 dpdy = dFdy( v_pos );
 
    float dhdx = dFdx( bumpHeight );
    float dhdy = dFdy( bumpHeight );
    
    wsNormal = PerturbNormal( wsNormal, dpdx, dpdy, dhdx, dhdy );
    
    float ndotl = dot( wsNormal, vec3( 0.0, 1.0, 0.0 ) );
    ndotl = clamp( ndotl, 0.0, 1.0 );
    
    gl_FragColor = vec4( vec3( ndotl ), 1.0 );
}
