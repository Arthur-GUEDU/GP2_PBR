using GLSample.AssetLoaders;
using GLSample.Core;
using GLSample.Rendering;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GLSample
{
    class Application
    {
        private const int kDefaultWidth = 1280;
        private const int kDefaultHeight = 720;

        private static void Main(string[] args)
        {
            var app = new Application();
            app.Start();
        }

        // Context Objects
        private IWindow _window;
        private Camera _camera;
        private CameraController _cameraController;
        private IInputContext _inputContext;
        private GL _gl;
        private ImGuiController _imGuiController;
        private Renderer _renderer;
        private Lights _lights;

        private GraphicObject[] _penguinObjects;
        private GLTexture _penguinBaseColorTexture;
        private Vector3 _penguinColor = new Vector3(0f, 1f, 0f);

        public void Start()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(kDefaultWidth, kDefaultHeight);
            options.Title = "OpenGL with Silk.NET";
            options.WindowBorder = WindowBorder.Fixed;

            var flags = ContextFlags.ForwardCompatible;
#if DEBUG
            flags |= ContextFlags.Debug;
#endif

            options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, flags, new APIVersion(4, 6));
            options.PreferredDepthBufferBits = 24;

            _window = Window.Create(options);
            _window.Load += OnLoad;
            _window.Render += OnRender;
            _window.Update += OnUpdate;
            _window.Closing += OnClose;
            _window.FramebufferResize += OnResize;

            _window.Run();
            _window.Dispose();
        }

        private void OnResize(Vector2D<int> newSize)
        {
            _camera.AspectRatio = (float)newSize.X / newSize.Y;
            _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
        }

        private unsafe void OnLoad()
        {
            _gl = _window.CreateOpenGL();
            _inputContext = _window.CreateInput();
            _imGuiController = new ImGuiController(_gl, _window, _inputContext);
            _renderer = new Renderer(_gl);
            _renderer.Initialize(kDefaultWidth, kDefaultHeight, _imGuiController);

            foreach (var keyboard in _inputContext.Keyboards)
            {
                keyboard.KeyDown += KeyDown;
                keyboard.KeyDown += (keyboard, key, state) => _cameraController.OnKeyDown(key);
                keyboard.KeyDown += (keyboard, key, state) => _renderer.OnKeyDown(key);
                keyboard.KeyUp += (keyboard, key, state) => _cameraController.OnKeyUp(key);
            }

            foreach (var mice in _inputContext.Mice)
            {
                mice.MouseMove += (mouse, delta) => _cameraController.OnMouseMove(delta);
            }

            CreateScene();
        }

        private void CreateScene()
        {
            _camera = new Camera();
            _camera.AspectRatio = (float)kDefaultWidth / kDefaultHeight;
            _camera.Transform.Position = new Vector3(0, 0, 1);
            _camera.Transform.EulerAngles = new Vector3(0, 0, 0);

            _cameraController = new CameraController(_camera);

            _lights = new Lights();
            _lights.AddDirectionLight(new Vector4(0, 0, -1, 1), new Vector4(1, 1, 1, 1));
            _lights.AddPointLight(new Vector4(1, 1, 0, 1), new Vector4(1, 0, 0, 1));

            // All spheres share the same transform.
            var penguinTransform = new Transform();
            penguinTransform.EulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
            penguinTransform.Scale = new Vector3(0.4f, 0.4f, 0.4f);

            _penguinBaseColorTexture = TextureLoader.LoadRGB8Texture2DFromFile(_gl, "Assets/Textures/penguin.png");

            var vertexSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Sphere.vertex.glsl"));
            var fragmentSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Sphere.fragment.glsl"));
            var sphereShader = new GLShader(_gl, vertexSource, fragmentSource);
            var sphereMeshes = AssimpLoader.LoadMeshes(_gl, "Assets/Models/penguin.glb");
            
            var QuadMeshes = AssimpLoader.LoadMeshes(_gl, "Assets/Models/Quad.obj");

            // Load Outline Shader
            var outlineVertexSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Outline.vertex.glsl"));
            var outlineFragmentSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Outline.fragment.glsl"));
            var outlineShader = new GLShader(_gl, outlineVertexSource, outlineFragmentSource);

            _penguinObjects = sphereMeshes.Select(mesh => new GraphicObject
            {
                IsTransparent = false,
                CastShadows = false,
                isOutlineEnable = true,
                QueueOrder = 0,
                Mesh = mesh,
                Shader = sphereShader,
                OutlineShader = outlineShader,
                Transform = penguinTransform,
                Texture = _penguinBaseColorTexture
            }).ToArray();
            _renderer.SceneObjects.AddRange(_penguinObjects);

            GraphicObject _quadObject = new GraphicObject
            {
                IsTransparent = false,
                CastShadows = false,
                isOutlineEnable = true,
                QueueOrder = 0,
                Mesh = QuadMeshes[0],
                Shader = sphereShader,
                OutlineShader = outlineShader,
                Transform = penguinTransform,
            };
            _renderer.QuadObject = _quadObject;
        }

        private void OnUpdate(double deltaTime)
        {
            _cameraController.Update((float)deltaTime);

            foreach (PointLight pointLight in _lights.pointLights)
            {
                if (pointLight.bIsSpinning)
                    pointLight.Turn((float)deltaTime);
            }
        }

        private void OnRender(double deltaTime)
        {
            _imGuiController.Update((float)deltaTime);
            
            if (!_renderer.IsPBRActive)
            {
                ImGui.ColorEdit3("Color", ref _penguinColor);
                if (!_renderer.IsDeferred)
                {
                    // Outline
                    ImGui.Separator();
                    ImGui.Checkbox(": Outline Enable", ref _penguinObjects[0].isOutlineEnable);
                    if (_penguinObjects[0].isOutlineEnable)
                    {
                        ImGui.ColorEdit4(": Outline Color", ref _penguinObjects[0].outlineColor);
                        ImGui.DragFloat(": Outline Size", ref _penguinObjects[0].outline, 0.001f, 0.005f, 0.1f);
                    }

                    ImGui.Separator();
                    ImGui.DragInt(": Toon Shading", ref _penguinObjects[0].isToonShadingEnable, 1f, 0, 1);
                    if (_penguinObjects[0].isToonShadingEnable == 1)
                    {
                        ImGui.DragInt(": Toon Level Color", ref _penguinObjects[0].toonLevelColor, 0.05f, 0, 20);
                    }

                    ImGui.Separator();
                    ImGui.DragInt(": Gooch Shading", ref _penguinObjects[0].isGoochShadingEnable, 1f, 0, 1);
                    if (_penguinObjects[0].isGoochShadingEnable == 1)
                    {
                        _penguinObjects[0].isOutlineEnable = false;
                        ImGui.DragFloat(": B value", ref _penguinObjects[0].goochBValue, 0.01f, 0f, 1f);
                        ImGui.DragFloat(": Y value", ref _penguinObjects[0].goochYValue, 0.01f, 0f, 1f);
                        ImGui.DragFloat(": Alpha value", ref _penguinObjects[0].goochAlphaValue, 0.01f, 0f, 1f);
                        ImGui.DragFloat(": Beta value", ref _penguinObjects[0].goochBetaValue, 0.01f, 0f, 1f);
                        ImGui.ColorEdit3(": Cool Color Base", ref _penguinObjects[0].goochCoolColorBase);
                        ImGui.ColorEdit3(": Warm Color Base", ref _penguinObjects[0].goochWarmColorBase);
                    }
                }
                _penguinObjects[0].Shader.SetVector("_Color", new Vector4(_penguinColor, 1.0f));
                _penguinObjects[0].Shader.SetTexture("_BaseColor", _penguinBaseColorTexture, 0);
            }
            else
            {
                _penguinObjects[0].isOutlineEnable = false;
                ImGui.Separator();
                ImGui.DragFloat(": Metallic value", ref _penguinObjects[0].metallic, 0.01f, 0f, 1f);
                ImGui.DragFloat(": Roughness value", ref _penguinObjects[0].roughness, 0.01f, 0.01f, 1f);
                ImGui.DragFloat(": Base reflectance value", ref _penguinObjects[0].baseReflectance, 0.01f, 0f, 1f);
                ImGui.DragFloat(": Ambient occlusion value", ref _penguinObjects[0].ambientOcclusion, 0.01f, 0f, 1f);
                ImGui.ColorEdit3(": Albedo value", ref _penguinObjects[0].albedo);
                ImGui.Checkbox(": Visibility Enabled", ref _penguinObjects[0].visibility);
                _lights.DrawImGui();
                //ImGui.ColorEdit3(": Point light color", ref pointLightColor);
                //ImGui.DragFloat3(": Point light position", ref pointLightPosition, 0.01f);
                //ImGui.Checkbox(": Point light spin", ref isPointLightSpinning);
                //ImGui.DragFloat(": Point light spin speed", ref pointLightSpinningSpeed, 0.01f, 0.1f, 50.0f);
            }

            _renderer.RenderScene(_camera, _lights);
        }

        private void OnClose()
        {
            _penguinBaseColorTexture.Dispose();
            _penguinObjects[0].Shader.Dispose();
            foreach (var penguinObject in _penguinObjects)
            {
                penguinObject.Mesh.Dispose();
            }
            _renderer.Dispose();
            _imGuiController.Dispose();
            _inputContext.Dispose();
            _gl.Dispose();
        }

        private void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (arg2 == Key.Escape)
            {
                _window.Close();
            }
        }
    }
}