// reference http://www.rorydriscoll.com/2012/01/11/derivative-maps/

#extension GL_OES_standard_derivatives : enable

varying vec2 v_txc;
varying vec3 v_nrm;
varying vec3 v_pos;

uniform sampler2D heightMap;
uniform float tile;
uniform float bumpness;
uniform float parallaxHeight;
uniform float parallaxSampleCount;
uniform float occlusion;
uniform float useParallax;
uniform float useSilhouette;
uniform float useSpecular;
uniform float debug;
uniform vec3 camPos;

float ApplyChainRule( float dhdu, float dhdv, float dud_, float dvd_ )
{
    return dhdu * dud_ + dhdv * dvd_;
}

vec3 SurfaceGradient( vec3 n, vec3 dpdx, vec3 dpdy, float dhdx, float dhdy )
{
    vec3 r1 = cross( dpdy, n );
    vec3 r2 = cross( n, dpdx );
    float det = dot( dpdx, r1 );
 
    return ( r1 * dhdx + r2 * dhdy ) / det;
}

vec3 ParallaxDirection( vec3 v, vec3 n, vec3 dpdx, vec3 dpdy, vec2 duvdx, vec2 duvdy )
{
    vec3 r1 = cross( dpdy, n );
    vec3 r2 = cross( n, dpdx );
    float det = dot( dpdx, r1 );
 
    vec2 vscr = vec2( dot( r1, v ), dot( r2, v ) ) / det;
    vec3 vtex;
    vtex.z  = dot( n, v ) / parallaxHeight;
    //vtex.z  = dot( n, v );
    vtex.xy = duvdx * vscr.x + duvdy * vscr.y;
    
    return vtex;
}

vec3 Lighting( vec3 lightDir, vec3 lightColor, vec3 wsNormal, vec3 wsViewDir, float ao )
{
    vec3 halfDir = normalize( lightDir + wsViewDir );

    float ndotl = dot( wsNormal, lightDir );
    ndotl = clamp( ndotl, 0.0, 1.0 );
    //ndotl = ndotl * ndotl * ao;
    
    if ( useSpecular > 0.0 )
    {
        float ndoth = dot( wsNormal, halfDir );
        ndoth = clamp( ndoth, 0.0, 1.0 );
        ndoth = pow( ndoth, 32.0 );
        ndotl += ndoth;
    }
    
    ndotl *= ao;
    
    return lightColor * ndotl;
}

void main()
{
    vec3 wsViewDir = normalize( camPos - v_pos );
    vec3 wsNormal = normalize( v_nrm );
    
    vec3 dpdx = dFdx( v_pos );
    vec3 dpdy = dFdy( v_pos );

    vec2 uv = v_txc * tile;
    vec2 duvdx = dFdx( uv );
    vec2 duvdy = dFdy( uv );
    
    float ao = 1.0;
    
    if ( useParallax > 0.0 )
    {
        vec3 tsPDir = ParallaxDirection( wsViewDir, wsNormal, dpdx, dpdy, duvdx, duvdy );
        
        // The length of this vector determines the furthest amount of displacement:
        float fLength = length( tsPDir );
        fLength = sqrt( fLength * fLength - tsPDir.z * tsPDir.z ) / tsPDir.z; 
        
        // Compute the maximum reverse parallax displacement vector:
        vec2 maxParallaxOffset = normalize( tsPDir.xy ) * fLength;
        
        // Scale the maximum amount of displacement by the artist-editable parameter: parallaxHeight
        maxParallaxOffset *= parallaxHeight;
        
        int numSteps = int( mix( parallaxSampleCount * 8.0, parallaxSampleCount, clamp( dot( wsViewDir, wsNormal ), 0.0, 1.0 ) ) );
        float stepSize = 1.0 / float( numSteps );
        float currHeight = 0.0;
        float prevHeight = 1.0;
        float currBound  = 1.0;
        
        vec2 offsetPerStep = stepSize * maxParallaxOffset;
        vec2 currOffset = uv;
        
        vec2 pt1;
        vec2 pt2;
        
        for ( int it = 0; it < 512; ++it )
        {
            if ( it >= numSteps )
                break;

            currOffset -= offsetPerStep;
            
            currHeight = texture2D( heightMap, currOffset ).b;
            
            currBound -= stepSize;
            
            if ( currHeight > currBound ) 
            {
                pt1 = vec2( currBound, currHeight );
                pt2 = vec2( currBound + stepSize, prevHeight );

                break;
            }
            
            prevHeight = currHeight;
        }
        
        // compute the parallaxAmount using line intersection
        float parallaxAmount = 0.0;
        
        float delta2 = pt2.x - pt2.y;
        float delta1 = pt1.x - pt1.y;

        float denominator = delta2 - delta1;

        if ( denominator != 0.0 )
            parallaxAmount = ( pt1.x * delta2 - pt2.x * delta1 ) / denominator;
        
        uv = uv - maxParallaxOffset * ( 1.0 - parallaxAmount );
        
        if ( useSilhouette > 0.0 )
        {
            float clipMax = tile;
            float clipMin = 0.0;
            
            if ( uv.x >= clipMax || uv.y >= clipMax
              || uv.x <= clipMin || uv.y <= clipMin )
            {
                discard;
            }
        }
        
        ao = 1.0 - occlusion;
        ao = 1.0 - ao * ao;
        ao = mix( 1.0, max( 0.0, currBound ), ao );
    }
    
    vec2 dhduv = texture2D( heightMap, uv ).rg;
    dhduv = ( dhduv * 2.0 - 1.0 ) * bumpness;
    
    float dhdx = ApplyChainRule( dhduv.x, dhduv.y, duvdx.x, duvdx.y );
    float dhdy = ApplyChainRule( dhduv.x, dhduv.y, duvdy.x, duvdy.y );
    
    wsNormal = normalize( wsNormal - SurfaceGradient( wsNormal, dpdx, dpdy, dhdx, dhdy ) );
    
    if ( debug > 0.0 )
    {
        gl_FragColor = vec4( wsNormal * 0.5 + 0.5, 1.0 );
        //gl_FragColor = texture2D( heightMap, v_txc * tile );
    }
    else
    {
        vec3 diff = vec3( 0.0 );
        
        diff += Lighting( normalize( vec3( 1.0, 1.0, 1.0 ) ), vec3( 1.0, 0.702, 0.351 ) * 0.75, wsNormal, wsViewDir, ao );
        diff += Lighting( normalize( vec3(-1.0,-1.0, 1.0 ) ), vec3( 0.702, 0.702, 1.0 ) * 0.75, wsNormal, wsViewDir, ao );
        
        gl_FragColor = vec4( diff, 1.0 );
    }
}
