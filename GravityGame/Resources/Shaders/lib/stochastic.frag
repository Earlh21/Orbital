#lib
vec4 tex2DStochastic(sampler2D tex, vec2 uv)
{
    //triangle vertices and blend weights
    //BW_vx[0...2].xyz = triangle verts
    //BW_vx[3].xy = blend weights (z is unused)
    mat4x3 BW_vx;

    //uv transformed into triangular grid space with UV scaled by approximation of 2*sqrt(3)
    vec2 skewUV = mat2(1.0, 0.0, -0.57735027, 1.15470054) * (3.464 * uv);

    //vertex IDs and barycentric coords
    vec2 vxID = vec2 (floor(skewUV));
    vec3 barry = vec3 (fract(skewUV), 0);
    barry.z = 1.0-barry.x-barry.y;

    BW_vx = ((barry.z>0) ?
    mat4x3(vec3(vxID, 0), vec3(vxID + vec2(0, 1), 0), vec3(vxID + vec2(1, 0), 0), barry.zyx) :
    mat4x3(vec3(vxID + vec2 (1, 1), 0), vec3(vxID + vec2 (1, 0), 0), vec3(vxID + vec2 (0, 1), 0), vec3(-barry.z, 1.0-barry.y, 1.0-barry.x)));

    //calculate derivatives to avoid triangular grid artifacts
    vec2 dx = dFdx(uv);
    vec2 dy = dFdy(uv);

    //blend samples with calculated weights
    return texture2D(tex, uv + rand2D(BW_vx[0].xy), dx, dy) * BW_vx[3].x +
    texture2D(tex, uv + rand2D(BW_vx[1].xy), dx, dy) * BW_vx[3].y +
    texture2D(tex, uv + rand2D(BW_vx[2].xy), dx, dy) * BW_vx[3].z;
}