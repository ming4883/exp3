// reference http://www.rorydriscoll.com/2012/01/11/derivative-maps/

#extension GL_OES_standard_derivatives : enable

varying vec2 v_txc;
varying vec3 v_nrm;
varying vec3 v_pos;
uniform sampler2D heightMap;
uniform float bumpness;
uniform float useSpecular;
uniform float useParallax;
uniform float debug;
uniform vec3 camPos;

vec3 SurfaceGradient( vec3 n, vec3 dpdx, vec3 dpdy, float dhdx, float dhdy )
{
    vec3 r1 = cross( dpdy, n );
    vec3 r2 = cross( n, dpdx );
    float det = dot( dpdx, r1 );
 
    return ( r1 * dhdx + r2 * dhdy ) / det;
}

vec2 ParallaxOffset( vec3 v, vec3 n, vec3 dpdx, vec3 dpdy, vec2 duvdx, vec2 duvdy )
{
    vec3 r1 = cross( dpdy, n );
    vec3 r2 = cross( n, dpdx );
    float det = dot( dpdx, r1 );
 
    vec2 vscr = vec2( dot( r1, v ), dot( r2, v ) ) / det;
    return duvdx * vscr.x + duvdy * vscr.y;
}

vec3 PerturbNormal( vec3 n, vec3 dpdx, vec3 dpdy, float dhdx, float dhdy )
{
    return normalize( n - SurfaceGradient( n, dpdx, dpdy, dhdx, dhdy ) );
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
    
    if ( useSpecular > 0.0 )
    {
        float ndoth = dot( wsNormal, halfDir );
        ndoth = clamp( ndoth, 0.0, 1.0 );
        ndoth = pow( ndoth, 32.0 );
        ndotl += ndoth;
    }
    
    return lightColor * ndotl;
}

void main()
{
    vec3 wsViewDir = normalize( camPos - v_pos );
    
    vec3 dpdx = dFdx( v_pos );
    vec3 dpdy = dFdy( v_pos );

    vec2 uv = v_txc * 2.0;
    vec2 duvdx = dFdx( uv );
    vec2 duvdy = dFdy( uv );
    
    float HEIGHT_SCALE = bumpness / 512.0;
    
    vec3 gradH = texture2D( heightMap, uv ).rgb;
    
    if ( useParallax > 0.0 )
    {
        gradH.z  = ( 1.0 - gradH.z ) * HEIGHT_SCALE;
        
        for ( int it = 0; it < 128; ++it )
        {
            uv += ParallaxOffset( wsViewDir, normalize( v_nrm ), dpdx, dpdy, duvdx, duvdy ) * vec2( -gradH.z, -gradH.z );
            
            if ( debug > 0.0 )
            {
                duvdx = dFdx( uv );
                duvdy = dFdy( uv );
            }
            
            gradH = texture2D( heightMap, uv ).rgb;
            gradH.z  = ( 1.0 - gradH.z ) * HEIGHT_SCALE;
        }
    }
    
    gradH.xy = ( gradH.xy * 2.0 - 1.0 ) * bumpness * 64.0;
    
    float dhdx = ApplyChainRule( gradH.x, gradH.y, duvdx.x, duvdx.y );
    float dhdy = ApplyChainRule( gradH.x, gradH.y, duvdy.x, duvdy.y );
    
    vec3 wsNormal = PerturbNormal( normalize( v_nrm ), dpdx, dpdy, dhdx, dhdy );
    
    vec3 diff = vec3( 0.0 );
    
    diff += Lighting( normalize( vec3( 1.0, 1.0, 1.0 ) ), vec3( 1.0, 0.702, 0.351 ) * 0.75, wsNormal, wsViewDir );
    diff += Lighting( normalize( vec3(-1.0,-1.0, 1.0 ) ), vec3( 0.702, 0.702, 1.0 ) * 0.75, wsNormal, wsViewDir );
    
    gl_FragColor = vec4( diff, 1.0 );
}
