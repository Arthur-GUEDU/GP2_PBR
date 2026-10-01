using GLSample.Rendering;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GLSample.Sources.Rendering
{
    internal class GLPerFrameUniformBuffer : IDisposable
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct Constants
        {
            [FieldOffset(0)] public Matrix4x4 viewProjectionMatrix;
            [FieldOffset(64)] public Vector4 cameraPos;
            [FieldOffset(80)] public GPUDirLight dirLight;
            [FieldOffset(112)] public GPUPointLight pointLight;

            public static nuint kByteSize = (nuint) Marshal.SizeOf<Constants>();
        }

        // Must match binding from "Assets/Shaders/Library/Core.glsl"
        private const uint kPerFrameConstantsBindingIndex = 0;

        public uint Handle { get; }
        private GL _gl;

        public GLPerFrameUniformBuffer(GL gl)
        {
            _gl = gl;
            Handle = gl.CreateBuffer();

            unsafe
            {
                // We will update this buffer every frame, so we set its storage as Dynamic.
                // This enable us to call glNamedBufferSubData.
                gl.NamedBufferStorage(Handle, Constants.kByteSize, null, (uint) BufferStorageMask.DynamicStorageBit);

                // We bind this buffer at creation time, because we will never use this index for anything else.
                // Thus, it will never be unbound.
                gl.BindBufferBase(BufferTargetARB.UniformBuffer, kPerFrameConstantsBindingIndex, Handle);
            }
        }

        public void Dispose()
        {
            _gl.DeleteBuffer(Handle);
        }

        public void UpdateConstants(Constants constants)
        {
            unsafe
            {
                _gl.NamedBufferSubData(Handle, 0, Constants.kByteSize, &constants);
            }
        }
    }
}
