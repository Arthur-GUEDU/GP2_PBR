using ImGuiNET;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GLSample.Rendering
{
    public class ImGuiPass : GLRenderPass
    {
        private Renderer _renderer;
        private ImGuiController _imGuiController;

        public ImGuiPass(Renderer renderer, ImGuiController imGuiController) : base(renderer.GL) 
        {
            _renderer = renderer;
            _imGuiController = imGuiController;
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

            // We don't need depth buffer
            depthAttachment = null;
        }

        public override void Render()
        {
            var cameraColorBuffer = _renderer.CameraColorBuffer;
            gl.Viewport(0, 0, cameraColorBuffer.Descriptor.width, cameraColorBuffer.Descriptor.height);
            _imGuiController.Render();
        }
    }
}
