using ImGuiNET;
using Silk.NET.Vulkan.Video;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace GLSample.Rendering
{
    public struct GPUDirLight
    {
        public Vector4 direction; // Direction + isEnable
        public Vector4 color; // color + intensity
    }

    public class DirLight
    {
        public DirLight()
        {
            gpuDirLight.direction = new Vector4(0f, 0f, 1f, 1f); // Direction + isEnable
            gpuDirLight.color = new Vector4(1f, 1f, 1f, 1f); // color + intensity
        }

        public GPUDirLight gpuDirLight;

        public void DrawImGui(int _lightNumber)
        {
            if (ImGui.TreeNodeEx("Light " + _lightNumber))
            {
                bool isEnable = gpuDirLight.direction.W != 0f;
                if (ImGui.Checkbox("Enable", ref isEnable))
                    gpuDirLight.direction.W = isEnable ? 1f : 0f;
                ImGui.DragFloat4("Position: ", ref gpuDirLight.direction, 0.01f);
                ImGui.ColorEdit4("Color: ", ref gpuDirLight.color);
                ImGui.DragFloat("Intensity: ", ref gpuDirLight.color.W, 0.01f, 0f, 5f);
                ImGui.TreePop();
            }
        }
    }

    public struct GPUPointLight
    {
        public Vector4 position; // position + isEnable
        public Vector4 color; // Color + Intensity
    }

    public class PointLight
    {
        public PointLight()
        {
            gpuPointLight.position = new Vector4(0f, 0f, 0f, 1f); // position + isEnable
            gpuPointLight.color = new Vector4(1f, 1f, 1f, 1f); // Color + Intensity

            spinningSpeed = 1f;
            bIsSpinning = false;
        }

        public GPUPointLight gpuPointLight;
        public float spinningSpeed;
        public bool bIsSpinning;

        public void Turn(float _deltaTime)
        {
            float crtRotationSpeed = spinningSpeed * _deltaTime * ((float)Math.PI / 180f);
            gpuPointLight.position = Vector4.Transform(gpuPointLight.position, Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), crtRotationSpeed));
        }

        public void DrawImGui(int _lightNumber)
        {
            if (ImGui.TreeNodeEx("Light " + _lightNumber))
            {
                bool isEnable = gpuPointLight.position.W != 0f;
                if (ImGui.Checkbox("Enable", ref isEnable))
                    gpuPointLight.position.W = isEnable ? 1f : 0f;
                ImGui.DragFloat4("Position: ", ref gpuPointLight.position, 0.01f);
                ImGui.ColorEdit4("Color: ", ref gpuPointLight.color);
                ImGui.DragFloat("Intensity: ", ref gpuPointLight.color.W, 0.01f, 0f, 5f);
                ImGui.DragFloat("Spinning Speed: ", ref spinningSpeed, 1f, -360f, 360f);
                ImGui.Checkbox("Spin:", ref bIsSpinning);
                ImGui.TreePop();
            }
        }
    }

    public struct Lights
    {
        public Lights()
        {
            dirLights = new List<DirLight>();
            pointLights = new List<PointLight>();
        }

        public List<DirLight> dirLights;
        public List<PointLight> pointLights;

        #region Directional Light
        public void AddDirectionLight(ref DirLight _dirLight)
        {
            dirLights.Add(_dirLight);
        }
        public void AddDirectionLight()
        {
            DirLight dirLight = new DirLight();
            dirLights.Add(dirLight);
        }
        public void AddDirectionLight(Vector4 _dir)
        {
            DirLight dirLight = new DirLight();
            dirLight.gpuDirLight.direction = _dir;
            dirLights.Add(dirLight);
        }
        public void AddDirectionLight(Vector4 _dir, Vector4 _color)
        {
            DirLight dirLight = new DirLight();
            dirLight.gpuDirLight.direction = _dir;
            dirLight.gpuDirLight.color = _color;
            dirLights.Add(dirLight);
        }
        public void AddDirectionLight(Vector4 _dir, Vector4 _color, float _intensisty)
        {
            DirLight dirLight = new DirLight();
            dirLight.gpuDirLight.direction = _dir;
            dirLight.gpuDirLight.color = _color;
            dirLight.gpuDirLight.color.W = _intensisty;
            dirLights.Add(dirLight);
        }
        #endregion

        #region Point Light
        public void AddPointLight(ref PointLight _pointLight)
        {
            pointLights.Add(_pointLight);
        }
        public void AddPointLight()
        {
            PointLight pointLight = new PointLight();
            pointLights.Add(pointLight);
        }
        public void AddPointLight(Vector4 _pos)
        {
            PointLight pointLight = new PointLight();
            pointLight.gpuPointLight.position = _pos;
            pointLights.Add(pointLight);
        }
        public void AddPointLight(Vector4 _pos, Vector4 _color)
        {
            PointLight pointLight = new PointLight();
            pointLight.gpuPointLight.position = _pos;
            pointLight.gpuPointLight.color = _color;
            pointLights.Add(pointLight);
        }
        public void AddPointLight(Vector4 _pos, Vector4 _color, float _intensity)
        {
            PointLight pointLight = new PointLight();
            pointLight.gpuPointLight.position = _pos;
            pointLight.gpuPointLight.color = _color;
            pointLight.gpuPointLight.color.W = _intensity;
            pointLights.Add(pointLight);
        }
        #endregion

        public void DrawImGui()
        {
            ImGuiTreeNodeFlags treeFlags = ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanFullWidth;
            ImGui.Separator();
            if (ImGui.TreeNodeEx("Directional lights:", treeFlags))
            {
                for (int i = 0; i < dirLights.Count; ++i)
                    dirLights[i].DrawImGui(i);
                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("Point Lights:", treeFlags))
            {
                for (int i = 0; i < pointLights.Count; ++i)
                    pointLights[i].DrawImGui(i);
                ImGui.TreePop();
            }
        }
    }
}
