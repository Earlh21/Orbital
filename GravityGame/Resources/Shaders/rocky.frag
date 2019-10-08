uniform sampler2D texture;
uniform sampler2D land_texture;
uniform sampler2D ice_texture;

uniform float seed;
uniform float temp;
uniform float water_percentage;
uniform float ice_percentage;

float rand2D(in vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
float rand3D(in vec3 co){
    return fract(sin(dot(co.xyz ,vec3(12.9898,78.233,144.7272))) * 43758.5453);
}

float simple_interpolate(in float a, in float b, in float x)
{
    return a + smoothstep(0.0,1.0,x) * (b-a);
}
float interpolatedNoise3D(in float x, in float y, in float z)
{
    float integer_x = x - fract(x);
    float fractional_x = x - integer_x;

    float integer_y = y - fract(y);
    float fractional_y = y - integer_y;

    float integer_z = z - fract(z);
    float fractional_z = z - integer_z;

    float v1 = rand3D(vec3(integer_x, integer_y, integer_z));
    float v2 = rand3D(vec3(integer_x+1.0, integer_y, integer_z));
    float v3 = rand3D(vec3(integer_x, integer_y+1.0, integer_z));
    float v4 = rand3D(vec3(integer_x+1.0, integer_y +1.0, integer_z));

    float v5 = rand3D(vec3(integer_x, integer_y, integer_z+1.0));
    float v6 = rand3D(vec3(integer_x+1.0, integer_y, integer_z+1.0));
    float v7 = rand3D(vec3(integer_x, integer_y+1.0, integer_z+1.0));
    float v8 = rand3D(vec3(integer_x+1.0, integer_y +1.0, integer_z+1.0));

    float i1 = simple_interpolate(v1,v5, fractional_z);
    float i2 = simple_interpolate(v2,v6, fractional_z);
    float i3 = simple_interpolate(v3,v7, fractional_z);
    float i4 = simple_interpolate(v4,v8, fractional_z);

    float ii1 = simple_interpolate(i1,i2,fractional_x);
    float ii2 = simple_interpolate(i3,i4,fractional_x);

    return simple_interpolate(ii1 , ii2 , fractional_y);
}

float Noise3D(in vec3 coord, in float wavelength)
{
    return interpolatedNoise3D(coord.x/wavelength, coord.y/wavelength, coord.z/wavelength);
}

void main()
{
    vec2 uv = gl_TexCoord[0].xy;
    vec2 xy = uv * float(textureSize(texture, 0).x) * 10;
    
    vec2 landuv = mod(xy / textureSize(land_texture, 0), 1);
    vec2 iceuv = mod(xy / textureSize(ice_texture, 0), 1);
    
    vec2 dist = abs(uv - 0.5) * 2;
    float r = dist.x * dist.x + dist.y * dist.y;

    if(r > 1)
    {
        //Pixel is empty space
        
        gl_FragColor = vec4(0, 0, 0, 0);
    }
    else
    {
        //Pixel is part of the planet
        
        float noise = Noise3D(vec3(uv.x, uv.y, seed), 0.15);
        
        if(noise < water_percentage)
        {
            //Pixel is water
            
            if(noise < water_percentage - water_percentage * ice_percentage)
            {
                //Pixel is deep ocean

                gl_FragColor = vec4(0, 0, 0.8, 1);
            }
            else
            {
                //Pixel is ice

                gl_FragColor = texture2D(ice_texture, iceuv);
            }
            
            
        }
        else
        {
            //Pixel is land
            
            
            
            gl_FragColor = texture2D(land_texture, landuv) + vec4((noise - 0.8) * 0.4, (noise - 0.8) * 0.4, (noise - 0.8) * 0.4, 1);
        }
    }
}