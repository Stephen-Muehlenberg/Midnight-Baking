shader_type spatial;
render_mode cull_disabled;

uniform float scale = 5.0;
uniform float speed = 2.0;
uniform float wiggle_amount = 10.0;

uniform float drop_strength = 20;
uniform float trail_strength = 0.3;
uniform float refraction = 0.3;

uniform float blur = 3.0;
uniform int layers = 1;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;

float RandomNumber(vec2 seed) {
	seed = fract(seed * vec2(123.456, 234.789));
	seed += dot(seed, seed + 34.73);
	return fract(seed.x * seed.y);
}

vec2 RainDrops(vec2 uv, float drop_scale, float drop_speed, float drop_wiggle) {
	float t = TIME * drop_speed;
	float base_uv_y = (1.0 - uv.y) * 0.3 + 0.7;
	float wiggle = uv.y * drop_wiggle;
	uv *= drop_scale;
	uv.y -= t * 0.25;
	vec2 grid_uv = fract(uv) - 0.5;
	vec2 id = floor(uv);
	
	float random_number = RandomNumber(id);
	t += random_number * TAU;
	wiggle *= random_number;
	
	float drop_radius = 0.05 * base_uv_y;
	float drop_offset = 0.5 - drop_radius;
	float x = (random_number - 0.5) * drop_offset;
	x += sin(wiggle + cos(wiggle * 4.0) * pow(sin(wiggle), 2)) * (drop_offset - abs(x));
	float y = cos(t + cos(t + cos(t) * 0.5)) * drop_offset;
	y += (grid_uv.x - x) * (grid_uv.x - x);
	vec2 drop_position = grid_uv - vec2(x, y);
	float drop = smoothstep(drop_radius, 0.01, length(drop_position));
	
	float trail = smoothstep(drop_radius, -drop_radius, drop_position.y);
	trail *= smoothstep(drop_radius, 0.01, abs(drop_position.x));
	trail *= smoothstep(-0.5, y, grid_uv.y);
	
	vec2 output = vec2(0);
	output += drop * drop_strength * base_uv_y;
	output += trail * trail_strength;
	return output * drop_position * refraction;
}

void fragment() {
	vec2 droplets = vec2(0);
	for (float i = 1.0; i <= float(layers); i++) {
		droplets += RainDrops(UV, scale * i * 0.2, speed * i * 0.22, wiggle_amount * i * 0.7);
	}
	ALBEDO = textureLod(screen_texture, SCREEN_UV - droplets, smoothstep(0.01, 0.001, droplets.x) * blur).rgb;
}
