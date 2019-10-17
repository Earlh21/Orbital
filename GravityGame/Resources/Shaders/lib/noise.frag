#lib
#include rand.frag
float simpleInterpolate(in float a, in float b, in float x)
{
    return a + smoothstep(0.0,1.0,x) * (b-a);
}
float interpolatedPerlin(in float x, in float y, in float z)
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

    float i1 = simpleInterpolate(v1,v5, fractional_z);
    float i2 = simpleInterpolate(v2,v6, fractional_z);
    float i3 = simpleInterpolate(v3,v7, fractional_z);
    float i4 = simpleInterpolate(v4,v8, fractional_z);

    float ii1 = simpleInterpolate(i1,i2,fractional_x);
    float ii2 = simpleInterpolate(i3,i4,fractional_x);

    return simpleInterpolate(ii1 , ii2 , fractional_y);
}

float perlin(in vec3 coord, in float wavelength)
{
    return interpolatedPerlin(coord.x/wavelength, coord.y/wavelength, coord.z/wavelength);
}

float perlinOctave(vec3 coord, float wavelength, int octaves, float wavelength_change)
{
    float val = 0;

    for(int i = 0; i < octaves; i++)
    {
        val += perlin(coord, wavelength);
        wavelength *= wavelength_change;
    }

    return val / octaves;
}