using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.IO;

namespace GLSample.Rendering
{
    public class DeferredLightingPass : GLRenderPass
    {
        private Renderer _renderer;
        private GLShader _shader;
        DrawingSettings settings;

        Shader Shader { get; set; }

        public DeferredLightingPass(Renderer renderer) : base(renderer.GL)
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

            var vtxSrc = File.ReadAllText("Assets/Shaders/Deferred.vertex.glsl");
            var frgSrc = File.ReadAllText("Assets/Shaders/Deferred.fragment.glsl");
            _shader = new GLShader(gl, vtxSrc, frgSrc);
            settings.overrideShader = _shader;
        }

        public override void Render()
        {
            var cameraColorBuffer = _renderer.CameraColorBuffer;
            gl.Viewport(0, 0, cameraColorBuffer.Descriptor.width, cameraColorBuffer.Descriptor.height);

            _shader.Use();
            _shader.SetTexture("gPosition", _renderer.PositionBuffer, 0);
            _shader.SetTexture("gNormal", _renderer.NormalBuffer, 1);
            _shader.SetTexture("gAlbedoSpec", _renderer.AlbedoBuffer, 2);

            _renderer.QuadObject.Draw(gl, settings);
        }
    }
}
