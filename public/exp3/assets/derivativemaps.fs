// reference http://www.rorydriscoll.com/2012/01/11/derivative-maps/

#extension GL_OES_standard_derivatives : enable

varying vec2 v_txc;
varying vec3 v_nrm;
varying vec3 v_pos;
uniform sampler2D heightMap;
uniform float bumpness;
uniform vec3 camPos;

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

float ApplyChainRule( float dhdu, float dhdv, float dud_, float dvd_ )
{
    return dhdu * dud_ + dhdv * dvd_;
}

vec3 Lighting( vec3 lightDir, vec3 lightColor, vec3 wsNormal, vec3 wsViewDir )
{
    vec3 halfDir = normalize( lightDir + wsViewDir );

    float ndotl = dot( wsNormal, lightDir );
    ndotl = clamp( ndotl, 0.0, 1.0 );
    
    float ndoth = dot( wsNormal, halfDir );
    ndoth = clamp( ndoth, 0.0, 1.0 );
    ndoth = pow( ndoth, 32.0 );
    
    return lightColor * ( ndotl + ndoth );
}

void main()
{
    vec3 dpdx = dFdx( v_pos );
    vec3 dpdy = dFdy( v_pos );
 
#if 0

    float bumpHeight = texture2D( heightMap, v_txc ).b * bumpness;
    
    float dhdx = dFdx( bumpHeight );
    float dhdy = dFdy( bumpHeight );

#else
    vec2 bumpGrad = ( texture2D( heightMap, v_txc ).rg * 2.0 - 1.0 );
    
    vec2 uv = v_txc;
    //uv.xy *= -1.0;
    vec2 duvdx = dFdx( uv ) * bumpness * 64.0;
    vec2 duvdy = dFdy( uv ) * bumpness * 64.0;
    
    float dhdx = ApplyChainRule( bumpGrad.x, bumpGrad.y, duvdx.x, duvdx.y );
    float dhdy = ApplyChainRule( bumpGrad.x, bumpGrad.y, duvdy.x, duvdy.y );

#endif
    
    vec3 wsNormal = PerturbNormal( normalize( v_nrm ), dpdx, dpdy, dhdx, dhdy );
    vec3 wsViewDir = normalize( camPos - v_pos );
    
    vec3 diff = vec3( 0.0 );
    
    diff += Lighting( normalize( vec3( 1.0, 1.0, 1.0 ) ), vec3( 1.0, 0.702, 0.351 ) * 0.75, wsNormal, wsViewDir );
    diff += Lighting( normalize( vec3(-1.0,-1.0, 1.0 ) ), vec3( 0.702, 0.702, 1.0 ) * 0.75, wsNormal, wsViewDir );
    
    gl_FragColor = vec4( diff, 1.0 );
}
