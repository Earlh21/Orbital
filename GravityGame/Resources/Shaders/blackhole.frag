uniform sampler2D screen;
uniform vec2 position;
uniform vec2 view_size;
uniform vec2 view_offset;
uniform float lensing;

void main()
{
    vec2 screen_size = textureSize(screen, 0).xy;
    
    vec2 uv = gl_TexCoord[0].xy;
    vec2 pixel_pos = uv * view_size - view_size / 2 + view_offset;
    
    /**float dist = length(pixel_pos - position);
    float angle = atan(pixel_pos.y - position.y, pixel_pos.x - position.x);
    
    float anglea = angle + 100000 / dist / dist;
    float angleb = angle + 100000 / dist / dist;
    
    vec2 samplea_pos = vec2(cos(anglea), sin(anglea)) * dist;
    vec2 sampleb_pos = vec2(cos(angleb), sin(angleb)) * dist;
    
    vec2 samplea_uv = (samplea_pos + view_size / 2 - view_offset) / view_size;
    vec2 sampleb_uv = (sampleb_pos + view_size / 2 - view_offset) / view_size;
    
    vec4 colora = texture2D(screen, samplea_uv);
    vec4 colorb = texture2D(screen, sampleb_uv);**/

    vec2 warp = normalize(pixel_pos - position) * pow(length(pixel_pos - position), -2.0) * 3000.0f * lensing;
    warp.y = -warp.y;
    
    vec2 samplepos = pixel_pos + warp;
    vec2 sampleuv = (samplepos + view_size / 2 - view_offset) / view_size;

    float light = clamp(0.1 * length(pixel_pos - position) - view_size * 1.5, 0.0, 1.0);
    vec4 color = texture2D(screen, sampleuv);
    
    vec4 frag = color * light;
    frag.w = 1;
    
    gl_FragColor = color;
}