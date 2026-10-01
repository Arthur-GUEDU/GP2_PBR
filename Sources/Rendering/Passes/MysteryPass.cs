using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace GLSample.Rendering
{
    public class MysteryPass : GLRenderPass
    {
        private GLTexture _colorBuffer;
        private GLMesh _mesh;
        private GLShader _shader;
        
        public MysteryPass(GL gl) : base(gl) {}

        public override void ConfigureTargets()
        {
            _colorBuffer = new GLTexture(gl, new GLTextureDescriptor()
            {
                format = SizedInternalFormat.RG16,
                dimension = TextureTarget.Texture2D,
                width = 256,
                height = 256,
                mipCount = 1
            });

            colorAttachments = new List<ColorAttachment>()
            {
                new ColorAttachment()
                {
                    target = _colorBuffer,
                    mipLevel = 0
                }
            };

            // No depth for this pass.
            depthAttachment = null;

            _mesh = new GLMesh(
                gl,
                new GLMesh.Vertex[]
                {
                    new () { position = new Vector3(-0.5f, 0, 0 ) },
                    new () { position = new Vector3(0, 0.5f, 0 ) },
                    new () { position = new Vector3(0.5f, 0, 0 ) },
                }, 
                new uint[] { 0, 1, 2 }
            );

            var vtxSrc = File.ReadAllText("Assets/Shaders/Triangle.vertex.glsl");
            var frgSrc = File.ReadAllText("Assets/Shaders/Triangle.fragment.glsl");

            _shader = new GLShader(gl, vtxSrc, frgSrc);
        }

        public override void Render()
        {
            var clearValue = new Vector4(0, 1, 0, 1);
            gl.Viewport(0, 0, 256, 256);
            gl.ClearColor(clearValue.X, clearValue.Y, clearValue.Z, clearValue.W);
            gl.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Use();
            _mesh.Draw();
        }
    }
}
