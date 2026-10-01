// Core.glsl

struct DirLight
{
	vec4 Direction; // Direction + isEnable
	vec4 Color; // Color + Intensity
};

struct PointLight 
{
	vec4 Position; // Position + isEnable
	vec4 Color; // Color + Intensity
};

layout (binding = 0, std140) uniform PerFrameConstants
{
	mat4 engine_Matrix_VP;
	vec4 engine_CameraPos;
	DirLight engine_DirLight;
	PointLight engine_PointLight;
};