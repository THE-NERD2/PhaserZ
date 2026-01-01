shader_type spatial;

uniform sampler2D heightmap;
varying vec2 xzpos;

void vertex() {
	xzpos = VERTEX.xz;
	VERTEX.y += texture(heightmap, xzpos * 0.5).r * 3.0;
}
void fragment() {
	NORMAL = normalize(cross(vec3(dFdx(xzpos), 0), vec3(dFdy(xzpos), 0)));
}