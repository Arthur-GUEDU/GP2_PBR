using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Numerics;
using System.IO;
using GLSample.AssetLoaders;

namespace GLSample.Rendering
{
    public class GeometryPass : GLRenderPass
    {
        private Renderer _renderer;
        private GLShader _shader;
        private DrawingSettings settings;

        Shader Shader { get; set; }

        public GeometryPass(Renderer renderer) : base(renderer.GL)
        {
            _renderer = renderer;
            settings = new DrawingSettings();
        }

        public override void ConfigureTargets()
        {
            colorAttachments = new List<ColorAttachment>(){
                new ColorAttachment(){ target = _renderer.PositionBuffer },
                new ColorAttachment(){ target = _renderer.NormalBuffer },
                new ColorAttachment(){ target = _renderer.AlbedoBuffer }
            };

            depthAttachment = new DepthAttachment()
            {
                target = _renderer.CameraDepthBuffer
            };

            var vertexSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Geometry.vertex.glsl"));
            var fragmentSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Geometry.fragment.glsl"));
            _shader = new GLShader(gl, vertexSource, fragmentSource);
            settings.overrideShader = _shader;
        }

        public override void Render()
        {
            var cameraColorBuffer = _renderer.CameraColorBuffer;
            gl.Viewport(0, 0, cameraColorBuffer.Descriptor.width, cameraColorBuffer.Descriptor.height);

            //Clear Depth and Color
            var clearValue = new Vector4(0, 0, 0, 0);
            gl.ClearColor(clearValue.X, clearValue.Y, clearValue.Z, clearValue.W);
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Setup Depth State
            gl.DepthMask(true);
            gl.DepthFunc(DepthFunction.Lequal);
            gl.Enable(EnableCap.DepthTest);

            _renderer.OpaqueList.Draw(gl, settings);

            //// Position
            //gl.BlitNamedFramebuffer(_renderer.PositionBuffer.Handle, 0,
            //    0, 0, (int)_renderer.ScreenWidth, (int)_renderer.ScreenHeight,
            //    0, (int)_renderer.ScreenHeight/2, (int)_renderer.ScreenWidth / 2, (int)_renderer.ScreenHeight,
            //    ClearBufferMask.ColorBufferBit,
            //    BlitFramebufferFilter.Nearest);
            
        }
    }
}