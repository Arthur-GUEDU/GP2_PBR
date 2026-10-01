using GLSample.Core;
using Silk.NET.OpenGL;
using System;
using System.Numerics;

namespace GLSample.Rendering
{
    public class GraphicObject
    {
        private static readonly GLShaderUniformId s_LocalToWorldId = GLShaderUniformId.FromName("_LocalToWorld");

        public bool IsTransparent { get; set; }
        public bool CastShadows { get; set; }
        public int QueueOrder { get; set; }

        public GLMesh Mesh { get; set; }
        public GLShader Shader { get; set; }
        public Transform Transform { get; set; }
        public GLTexture Texture { get; set; }

        // Effect Variables
        // Outline
        public bool isOutlineEnable = false;
        public Vector4 outlineColor = new Vector4(1f, 1f, 1f, 1f);
        public float outline = 0.01f;
        public GLShader OutlineShader { get; set; }
        // Toon Shading
        public int isToonShadingEnable = 0;
        public int toonLevelColor = 6;
        // Gooch Shading
        public int isGoochShadingEnable = 0;
        public float goochBValue = 0.5f;
        public float goochYValue = 0.5f;
        public float goochAlphaValue = 0.5f;
        public float goochBetaValue = 0.5f;
        public Vector3 goochCoolColorBase = new Vector3(0f, 0f, 1f);
        public Vector3 goochWarmColorBase = new Vector3(1f, 1f, 0f);

        // Material variables
        public float metallic = 0.5f;
        public float roughness = 0.5f;
        public float baseReflectance = 0.01f;
        public float ambientOcclusion;
        public Vector3 albedo = new Vector3(1f, 1f, 1f);
        public bool visibility = false;

        public void Draw(GL gl, DrawingSettings settings)
        {
            var shader = settings.overrideShader != null ? settings.overrideShader : Shader;
            var outlineShader = settings.overrideShader != null ? settings.overrideShader : OutlineShader;

            if (shader != null && Mesh != null)
            {
                shader.SetTexture("textureSampler", Texture, 1);
                if (outlineShader != null && isOutlineEnable)
                {
                    gl.StencilFunc(GLEnum.Notequal, 1, 0xFF);
                    gl.StencilMask(0x00);
                    gl.Disable(GLEnum.DepthTest);


                    outlineShader.Use();
                    outlineShader.SetMatrix(s_LocalToWorldId, Transform.LocalToWorldMatrix);
                    outlineShader.SetVector("OutlineColor", outlineColor);
                    outlineShader.SetFloat("_outline", outline);
                    Mesh.Draw();


                    gl.StencilMask(0xFF);
                    gl.StencilFunc(GLEnum.Always, 0, 0xFF);
                    gl.Enable(GLEnum.DepthTest);


                }
                shader.Use();
                shader.SetMatrix(s_LocalToWorldId, Transform.LocalToWorldMatrix);
                shader.SetInt("isToon", isToonShadingEnable);
                shader.SetInt("toonColorLevel", toonLevelColor);
                shader.SetInt("isGooch", isGoochShadingEnable);
                shader.SetFloat("goochB", goochBValue);
                shader.SetFloat("goochY", goochYValue);
                shader.SetFloat("goochAlpha", goochAlphaValue);
                shader.SetFloat("goochBeta", goochBetaValue);
                shader.SetVector("goochCoolColorBase", new Vector4(goochCoolColorBase, 1f));
                shader.SetVector("goochWarmColorBase", new Vector4(goochWarmColorBase, 1f));
                shader.SetFloat("metallic", metallic);
                shader.SetFloat("roughness", roughness);
                shader.SetFloat("baseReflectance", baseReflectance);
                shader.SetFloat("ambientOcclusion", ambientOcclusion);
                shader.SetVector("albedo", new Vector4(albedo, 0));
                shader.SetBool("visibility", visibility);
                Mesh.Draw();
            }
        }
    }
}
