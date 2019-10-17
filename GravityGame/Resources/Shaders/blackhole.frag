#version 130

uniform sampler2D screen;
uniform vec2 position;

void main()
{
    vec2 size = textureSize(screen, 0).xy;
    
    vec2 uv = gl_TexCoord[0].xy;
    vec2 xy = uv * size;
    
    float dist = length(xy - position);
    float angle = atan(position.y - xy.y, position.x - xy.x) + radians(180);
    
    float anglea = angle + 1 / dist;
    float angleb = angle - 1 / dist;
    
    vec2 posa = vec2(sin(anglea), cos(anglea)) * dist;
    vec2 posb = vec2(sin(angleb), cos(angleb)) * dist;
    
    vec2 uva = posa / size;
    vec2 uvb = posb / size;
    
    vec4 colora = texture2D(screen, uva);
    vec4 colorb = texture2D(screen, uvb);
    
    gl_FragColor = colora + colorb;
}