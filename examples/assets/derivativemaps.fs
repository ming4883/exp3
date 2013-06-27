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

vec3 SurfaceGradient( vec3 n, vec3 dpdx, vec3 dpdy, float dhdx, float dhdy )
{
    vec3 r1 = cross( dpdy, n );
    vec3 r2 = cross( n, dpdx );
    float det = dot( dpdx, r1 );
 
    return ( r1 * dhdx + r2 * dhdy ) / det;
}

vec3 ParallaxOffset( vec3 v, vec3 n, vec3 dpdx, vec3 dpdy, vec2 duvdx, vec2 duvdy )
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

vec3 PerturbNormal( vec3 n, vec3 dpdx, vec3 dpdy, float dhdx, float dhdy )
{
    return normalize( n - SurfaceGradient( n, dpdx, dpdy, dhdx, dhdy ) );
}

float ApplyChainRule( float dhdu, float dhdv, float dud_, float dvd_ )
{
    return dhdu * dud_ + dhdv * dvd_;
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

float Displacement( float h, float s, float b )
{
    return h * s + b;
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
    
    vec3 gradH;
    
    float ao = 1.0;
    vec4 debugVal = vec4( 0.0 );
    
    if ( useParallax > 0.0 )
    {
        vec3 vViewTS = ParallaxOffset( wsViewDir, wsNormal, dpdx, dpdy, duvdx, duvdy );
        
        // Compute initial parallax displacement direction:
        vec2 vParallaxDirection = normalize(  vViewTS.xy );
           
        // The length of this vector determines the furthest amount of displacement:
        float fLength         = length( vViewTS );
        float fParallaxLength = sqrt( fLength * fLength - vViewTS.z * vViewTS.z ) / vViewTS.z; 
           
        // Compute the actual reverse parallax displacement vector:
        vec2 vParallaxOffsetTS = vParallaxDirection * fParallaxLength;
           
        // Need to scale the amount of displacement to account for different height ranges
        // in height maps. This is controlled by an artist-editable parameter:
        vParallaxOffsetTS *= parallaxHeight;
        
        //int nNumSteps   = 64;
        int nNumSteps   = int( mix( parallaxSampleCount * 2.0, parallaxSampleCount, clamp( dot( wsViewDir, wsNormal ), 0.0, 1.0 ) ) );
        float fStepSize = 1.0 / float( nNumSteps );
        float fCurrHeight = 0.0;
        float fPrevHeight = 1.0;
        
        vec2   vTexOffsetPerStep = fStepSize * vParallaxOffsetTS;
        vec2   vTexCurrentOffset = uv;
        float  fCurrentBound     = 1.0;
        float  fParallaxAmount   = 0.0;
        
        vec2 pt1;
        vec2 pt2;
        
        for ( int it = 0; it < 512; ++it )
        {
            if ( it >= nNumSteps )
                break;

            vTexCurrentOffset -= vTexOffsetPerStep;
            
            gradH = texture2D( heightMap, vTexCurrentOffset ).rgb;
            fCurrHeight = gradH.b;
            
            fCurrentBound -= fStepSize;
            
            if ( fCurrHeight > fCurrentBound ) 
            {
                pt1 = vec2( fCurrentBound, fCurrHeight );
                pt2 = vec2( fCurrentBound + fStepSize, fPrevHeight );

                break;
            }
            
            fPrevHeight = fCurrHeight;
        }
        
        float fDelta2 = pt2.x - pt2.y;
        float fDelta1 = pt1.x - pt1.y;

        float fDenominator = fDelta2 - fDelta1;

        // SM 3.0 requires a check for divide by zero, since that operation will generate
        // an 'Inf' number instead of 0, as previous models (conveniently) did:
        if ( fDenominator == 0.0 )
        {
            fParallaxAmount = 0.0;
        }
        else
        {
            fParallaxAmount = (pt1.x * fDelta2 - pt2.x * fDelta1 ) / fDenominator;
        }
        
        vec2 vParallaxOffset = vParallaxOffsetTS * ( 1.0 - fParallaxAmount );

        // The computed texture offset for the displaced point on the pseudo-extruded surface:
        uv = uv - vParallaxOffset;
        
        if ( useSilhouette > 0.0 )
        {
            float clipMax = tile;
            float clipMin = 0.0;
            
            
            if ( uv.x >= clipMax || uv.y >= clipMax
              || uv.x <= clipMin || uv.y <= clipMin )
            {
                discard;
            }
            
            /*uv = clamp( uv, clipMin, clipMax );*/
        }
        
        ao = mix( 1.0, max( 0.0, fCurrentBound ), occlusion );
        
        debugVal = vec4( ao );
        
        //debugVal = vec4( length( vParallaxOffset ) > clipLimit ? 1.0 : 0.0 );
        //debugVal = vec4( clipLimit );
    }
    
    gradH = texture2D( heightMap, uv ).rgb;
    
    gradH.xy = ( gradH.xy * 2.0 - 1.0 ) * bumpness;
    
    float dhdx = ApplyChainRule( gradH.x, gradH.y, duvdx.x, duvdx.y );
    float dhdy = ApplyChainRule( gradH.x, gradH.y, duvdy.x, duvdy.y );
    
    wsNormal = PerturbNormal( wsNormal, dpdx, dpdy, dhdx, dhdy );
    
    if ( debug > 0.0 )
    {
        gl_FragColor = vec4( wsNormal * 0.5 + 0.5, 1.0 );
        gl_FragColor = texture2D( heightMap, v_txc * tile );
    }
    else
    {
        vec3 diff = vec3( 0.0 );
        
        diff += Lighting( normalize( vec3( 1.0, 1.0, 1.0 ) ), vec3( 1.0, 0.702, 0.351 ) * 0.75, wsNormal, wsViewDir, ao );
        diff += Lighting( normalize( vec3(-1.0,-1.0, 1.0 ) ), vec3( 0.702, 0.702, 1.0 ) * 0.75, wsNormal, wsViewDir, ao );
        
        gl_FragColor = vec4( diff, 1.0 );
    }
}
