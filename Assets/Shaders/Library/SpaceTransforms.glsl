// SpaceTransforms.glsl

vec4 TransformWorldToClip(vec3 position)
{
	return engine_Matrix_VP * vec4(position, 1.0);
}