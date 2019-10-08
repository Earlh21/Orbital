uniform sampler2D texture;
uniform vec4 color;

void main()
{
    vec2 dist = abs(gl_TexCoord[0].xy - 0.5) * 2;
    float r = dist.x * dist.x + dist.y * dist.y;

    if(r > 1)
    {
        gl_FragColor = vec4(0, 0, 0, 0);    
    }
    else
    {
        gl_FragColor = color;
    }
}