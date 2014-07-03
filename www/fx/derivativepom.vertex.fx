precision mediump float;

// Attributes
attribute vec3 position;
attribute vec3 normal;
attribute vec2 uv;

// Varying
varying vec2 v_txc;
varying vec3 v_nrm;
varying vec3 v_pos;

//Uniforms
uniform mat4 world;
uniform mat4 worldViewProjection;

void main()
{
    v_txc = uv;
    
    v_nrm = (world  * vec4 (normal, 0.0)).xyz;
    
    v_pos = (world  * vec4 (position, 1.0)).xyz;
    
    gl_Position = worldViewProjection * vec4 (position, 1.0);
}
