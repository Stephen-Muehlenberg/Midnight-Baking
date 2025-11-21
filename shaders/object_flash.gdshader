shader_type spatial;

const float speed = 5.0;

void fragment() {
	float intensitySinewave = (sin(TIME*speed)+1.0)/15.0;
	float a = (sin((TIME + PI / 2.0)*speed/2.0)+1.0)/2.0;
	float clamped = floor(a + 0.5);
	ALPHA = a;//1.0;
	EMISSION = vec3(clamped*intensitySinewave,clamped*intensitySinewave,clamped*intensitySinewave*0.5);
}
