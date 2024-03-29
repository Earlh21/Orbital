#version 130
#include lib\noise.frag

uniform sampler2D texture;
uniform sampler2D land_texture;
uniform sampler2D ice_texture;

uniform vec4 atmo_color;
uniform float atmo_strength;
uniform float seed;
uniform float temp;
uniform float water_percentage;
uniform float ice_percentage;
uniform float time;
uniform float radius;

float invlerp(float a, float b, float value)
{
    return (value - a) / (b - a);
}

vec4 moltenColor(float value)
{
    vec4 black = vec4(0, 0, 0, 1);
    vec4 red = vec4(1, 0, 0, 1);
    vec4 yellow = vec4(1, 1, 0, 1);
    vec4 white = vec4(1, 1, 1, 1);

    if(value < 0.5)
    {
        value = value * 2;
        return red * value;
    }
    else if(value < 0.75)
    {
        value = (value - 0.5) * 4;
        return red * (1 - value) + yellow * value;
    }
    else
    {
        value = (value - 0.75) * 4;
        return yellow * (1 - value) + white * value;
    }
}

void main()
{
    float temp_atmo_strength = atmo_strength;
    
    float size = float(textureSize(texture, 0).x);
    float radius_percent = (radius) / (size / 2);

    //Get UV across planet and padding
    vec2 uv = gl_TexCoord[0].xy;
    //Get the actual xy coordinate along the dummy texture
    vec2 xy = uv * float(textureSize(texture, 0).x);

    //Get uv values for the textures using the real xy coordinates
    //We want the same texture resolution for every planet
    vec2 landuv = mod(xy * 40 / textureSize(land_texture, 0), 1);
    vec2 iceuv = mod(xy * 40 / textureSize(ice_texture, 0), 1);

    //UV displacement from the 
    vec2 dist = abs(uv - 0.5) * 2;
    //UV distance from the center of the planet
    float r = sqrt(dist.x * dist.x + dist.y * dist.y);

    vec4 planet_color;
    
    if(r > radius_percent)
    {
        //Pixel is empty space
        planet_color = vec4(0, 0, 0, 0);
    }
    else
    {
        //Pixel is part of the planet
        float noise = perlinOctave(vec3(uv.x, uv.y, seed), 0.15, 6, 0.5);

        if(noise < water_percentage)
        {
            //Pixel is water
            if(noise < water_percentage - water_percentage * ice_percentage)
            {
                //Pixel is too deep to freeze at the current value
                planet_color = vec4(0, 0, 0.8, 1);
            }
            else
            {
                //Pixel is ice
                planet_color = texture2D(ice_texture, iceuv);
            }
        }
        else
        {
            //Pixel is land
            
            //Get the texture color at the pixel
            vec4 tex_color = texture2D(land_texture, landuv);
            //Recolor to be brighter at higher spots
            vec4 depth_color = vec4((noise - 0.8) * 0.4, (noise - 0.8) * 0.4, (noise - 0.8) * 0.4, 1);
            vec4 base_color = tex_color + depth_color;
            
            float molten_noise = perlin(vec3(uv.x, uv.y, seed + time / 40), 0.1) * 0.2;
            float molten_value = clamp(invlerp(600, 8000, temp), 0, 1);

            //Shift molten value over time
            molten_value *= 1 + (sin(time / 4) + 1) * 0.05;
            //Add noise to the molten value
            molten_value += molten_noise - 0.2;
            //Clamp it
            molten_value = clamp(molten_value, 0, 1);
            //Get the color of the lava using the gradient function
            vec4 molten_color = moltenColor(molten_value);
            
            planet_color = (base_color) * (1 - molten_value) + molten_color * molten_value;
        }
    }

    //Get the global molten value for use later
    float molten_value = clamp(invlerp(600, 8000, temp) - 0.1, 0, 1);

    //Final atmosphere color to use
    vec4 atmo_pixel_color;

    if(r > radius_percent)
    {
        //We're outside the planet, so make the atmosphere drop off sharply
        float atmo_alpha = clamp(0.5 - pow((r - radius_percent), 2) * 600, 0, 1);

        //Loewr atmosphere influence when the planet is hotter and emitting more light
        atmo_alpha = clamp(atmo_alpha - pow(molten_value, 2) * 4, 0, 1);
        temp_atmo_strength = clamp(temp_atmo_strength - pow(molten_value, 2) * 4, 0, 1);

        atmo_pixel_color = atmo_color * clamp(temp_atmo_strength + 0.1, 0, 1);
        atmo_pixel_color.w = atmo_alpha;

        temp_atmo_strength *= atmo_alpha;
    }
    else
    {
        //Atmosphere is thicker near the edges of the planet
        float thickness = pow((r / radius_percent), 4) * 0.45f + 1;
        temp_atmo_strength *= thickness;
        //Lower atmosphere influence when the planet is hotter and emitting more light
        temp_atmo_strength = clamp(temp_atmo_strength - pow(molten_value, 2) * 4, 0, 1);
        
        atmo_pixel_color = atmo_color * temp_atmo_strength;
    }

    vec4 glow_color;

    if(r > radius_percent)
    {
        float glow_alpha = clamp(molten_value - (r - radius_percent) * 6, 0, 1);
        glow_color = moltenColor(molten_value);
        glow_color.w = glow_alpha;
    }
    else
    {
        //No glow inside the planet
        glow_color = vec4(0, 0, 0, 0);
    }

    gl_FragColor = planet_color * (1 - temp_atmo_strength) + atmo_pixel_color + glow_color * (1 - temp_atmo_strength);
}