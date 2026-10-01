using GLSample.AssetLoaders;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace GLSample.Rendering
{
    public class PhysicallyBasedPass : GLRenderPass
    {
        private Renderer _renderer;
        private GLShader _shader;
        DrawingSettings settings;

        Shader Shader { get; set; }

        public PhysicallyBasedPass(Renderer renderer) : base(renderer.GL)
        {
            _renderer = renderer;
            settings = new DrawingSettings();
        }

        public override void ConfigureTargets()
        {
            colorAttachments = new List<ColorAttachment>()
            {
                new ColorAttachment()
                {
                    target = _renderer.CameraColorBuffer,
                }
            };

            depthAttachment = new DepthAttachment()
            {
                target = _renderer.CameraDepthBuffer
            };

            var vtxSrc = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/PhysicallyBased.vertex.glsl"));
            var frgSrc = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/PhysicallyBased.fragment.glsl"));
            _shader = new GLShader(gl, vtxSrc, frgSrc);
            settings.overrideShader = _shader;
        }

        public override void Render()
        {
            var cameraColorBuffer = _renderer.CameraColorBuffer;

            gl.Viewport(0, 0, cameraColorBuffer.Descriptor.width, cameraColorBuffer.Descriptor.height);

            // Clear Depth and Color
            var clearValue = new Vector4(0.25f, 0.25f, 0.25f, 0);
            gl.ClearColor(clearValue.X, clearValue.Y, clearValue.Z, clearValue.W);
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Setup Depth State
            //gl.DepthMask(true);
            //gl.DepthFunc(DepthFunction.Lequal);
            gl.Enable(EnableCap.DepthTest);
            gl.Enable(EnableCap.StencilTest);
            gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

            _renderer.OpaqueList.Draw(gl, settings);
        }
    }
}
