uniform vec4 color;
uniform float expand;
uniform float radius;
uniform vec2 center;
uniform float window_height;

void main(void)
{
    vec2 centerFromSfml = vec2(center.x, window_height - center.y);
    vec2 p = (gl_FragCoord.xy - centerFromSfml) / radius;
    float r = sqrt(dot(p, p));
    if (r < 1.0)
    {
        gl_FragColor = mix(color, gl_Color, (r - expand) / (1 - expand));
    }
    else
    {
        gl_FragColor = gl_Color;
    }
}