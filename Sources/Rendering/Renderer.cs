using GLSample.Core;
using GLSample.Sources.Rendering;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.SDL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GLSample.Rendering
{
    public class Renderer : IDisposable
    {
        public DrawList OpaqueList { get; } = new DrawList("Opaque Objects");
        public DrawList TransparentList { get; } = new DrawList("Transparent Objects");
        public DrawList ShadowCasterList { get; } = new DrawList("Shadow Casters");

        public List<GraphicObject> SceneObjects { get; } = new List<GraphicObject>(64);
        public GL GL => _gl;

        private GL _gl;
        private GLPerFrameUniformBuffer _perFrameUniformBuffer;

        public GLTexture CameraColorBuffer { get; private set; }
        public GLTexture CameraDepthBuffer { get; private set; }
        public GLTexture PositionBuffer { get; private set; }
        public GLTexture AlbedoBuffer { get; private set; }
        public GLTexture NormalBuffer { get; private set; }
        public GraphicObject QuadObject;

        private DrawOpaquePass _drawOpaquePass;
        private MysteryPass _mysteryPass;
        private ImGuiPass _imGuiPass;
        private GeometryPass _geometryPass;
        private DeferredLightingPass _deferredLightingPass;
        private PhysicallyBasedPass _physicsBasedPass;

        private uint _screenWidth, _screenHeight;
        public uint ScreenWidth { get { return _screenWidth; } }
        public uint ScreenHeight { get { return _screenHeight; } }

        private bool isDeferred = false, isPBRActive = true;
        public bool IsDeferred { get { return isDeferred; } }
        public bool IsPBRActive { get { return isPBRActive; } }

        public Renderer(GL gl)
        {
            _gl = gl;
        }

        public void OnKeyDown(Key key)
        {
            switch (key)
            {
                case Key.Enter: 
                    isDeferred = !isDeferred; 
                    var clearValue = new Vector4(0, 0, 0, 0);
                    _gl.ClearColor(clearValue.X, clearValue.Y, clearValue.Z, clearValue.W);
                    _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    break;
                case Key.ShiftRight:
                    isPBRActive = !isPBRActive;
                    break;
            }
        }

        public unsafe void Initialize(uint width, uint height, ImGuiController imGuiController = null)
        {
            _screenWidth = width;
            _screenHeight = height;

#if DEBUG
            SetupDebugCallback();
#endif

            _perFrameUniformBuffer = new GLPerFrameUniformBuffer(_gl);

            CameraColorBuffer = new GLTexture(_gl, new GLTextureDescriptor(width, height, SizedInternalFormat.Rgba8));
            CameraDepthBuffer = new GLTexture(_gl, new GLTextureDescriptor(width, height, SizedInternalFormat.DepthComponent24));

            PositionBuffer = new GLTexture(_gl, new GLTextureDescriptor(width, height, SizedInternalFormat.Rgb8));
            AlbedoBuffer = new GLTexture(_gl, new GLTextureDescriptor(width, height, SizedInternalFormat.Rgb8));
            NormalBuffer = new GLTexture(_gl, new GLTextureDescriptor(width, height, SizedInternalFormat.Rgb8));

            _drawOpaquePass = new DrawOpaquePass(this);
            _mysteryPass = new MysteryPass(GL);
            _imGuiPass = new ImGuiPass(this, imGuiController);
            _geometryPass = new GeometryPass(this);
            _deferredLightingPass = new DeferredLightingPass(this);
            _physicsBasedPass = new PhysicallyBasedPass(this);

            // TODO Initialize PBR render passes

            _drawOpaquePass.Initialize();
            _imGuiPass.Initialize();
            _mysteryPass.Initialize();
            _geometryPass.Initialize();
            _deferredLightingPass.Initialize();
            _physicsBasedPass.Initialize();
        }

        public void Dispose()
        {
            _perFrameUniformBuffer.Dispose();
        }

        public void RenderScene(Camera camera, Lights lights)
        {
            // Clear Depth and Color
            var clearValue = new Vector4(0.25f, 0.25f, 0.25f, 0);
            _gl.ClearColor(clearValue.X, clearValue.Y, clearValue.Z, clearValue.W);
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            UpdateDrawLists();
            SetupPerFrameConstants(camera, lights);

            if (isPBRActive)
            {
                _physicsBasedPass.ExecutePass();
                _imGuiPass.ExecutePass();
            }
            else if (isDeferred)
            {
                _geometryPass.ExecutePass();
                _deferredLightingPass.ExecutePass();
            }
            else
            {
                _drawOpaquePass.ExecutePass();
                _mysteryPass.ExecutePass();
                _imGuiPass.ExecutePass();
            }
            // Finally, present to the screen.
            _gl.BlitNamedFramebuffer(_imGuiPass.FramebufferHandle, 0,
                0, 0, (int)_screenWidth, (int)_screenHeight,
                0, 0, (int)_screenWidth, (int)_screenHeight,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Nearest);
        }

        private void SetupPerFrameConstants(Camera camera, Lights lights)
        {
            Matrix4x4.Invert(camera.Transform.LocalToWorldMatrix, out var viewMatrix);
            var projectionMatrix = camera.ProjectionMatrix;

            var perFrameConstants = new GLPerFrameUniformBuffer.Constants()
            {
                viewProjectionMatrix = viewMatrix * projectionMatrix,
                cameraPos = new Vector4(camera.Transform.Position, 1),
                dirLight = lights.dirLights[0].gpuDirLight,
                pointLight = lights.pointLights[0].gpuPointLight
            };

            _perFrameUniformBuffer.UpdateConstants(perFrameConstants);
        }

        public void UpdateDrawLists()
        {
            OpaqueList.Clear();
            TransparentList.Clear();
            ShadowCasterList.Clear();

            foreach (var obj in SceneObjects)
            {
                if (obj.IsTransparent)
                {
                    TransparentList.Add(obj);
                }
                else
                {
                    OpaqueList.Add(obj);
                }

                if (obj.CastShadows)
                {
                    ShadowCasterList.Add(obj);
                }
            }
        }

#if DEBUG
        private void SetupDebugCallback()
        {
            unsafe
            {
                DebugProc callback = (source, type, id, severity, length, message, userParam) =>
                {
                    if ((int)severity == (int)DebugSeverity.DebugSeverityNotification)
                        return;

                    var messageStr = Marshal.PtrToStringAnsi(message, length);
                    Console.WriteLine($"{source}:{type}[{severity}]({id}) {messageStr}");
                };
                _gl.DebugMessageCallback(callback, null);
                _gl.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, 0, null, true);
            }
        }
#endif
    }
}
