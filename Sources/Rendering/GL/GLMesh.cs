using Silk.NET.OpenGL;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GLSample.Rendering
{
    public class GLMesh : IDisposable
    {
        public struct Vertex
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector2 texCoords;
        }

        public uint VertexBufferHandle { get; }
        public uint IndexBufferHandle { get; }
        public uint VertexArrayHandle { get; }
        public uint IndexCount { get; }
        private GL _gl;

        internal GLMesh(GL gl, ReadOnlySpan<Vertex> vertices, ReadOnlySpan<uint> indices)
        {
            _gl = gl;
            VertexBufferHandle = gl.CreateBuffer();
            IndexBufferHandle = gl.CreateBuffer();
            VertexArrayHandle = gl.CreateVertexArray();
            IndexCount = (uint) indices.Length;

            // We allocate the buffer. We directly pass the data and set the StorageMask to None because
            // once the buffer is uploaded, we will never modify it again.
            _gl.NamedBufferStorage(VertexBufferHandle, vertices, BufferStorageMask.None);
            _gl.NamedBufferStorage(IndexBufferHandle, indices, BufferStorageMask.None);

            // Enable 3 vertex attributes (position, normal, texcoords)
            _gl.EnableVertexArrayAttrib(VertexArrayHandle, 0);
            _gl.EnableVertexArrayAttrib(VertexArrayHandle, 1);
            _gl.EnableVertexArrayAttrib(VertexArrayHandle, 2);

            // Define their format.
            // In C++, we could use offsetof(Vertex, attrib) to get offsets but this does not exist in C#.
            // Thus, we have to set them manually.
            _gl.VertexArrayAttribFormat(VertexArrayHandle, 0, 3, VertexAttribType.Float, false, 0);
            _gl.VertexArrayAttribFormat(VertexArrayHandle, 1, 3, VertexAttribType.Float, false, 12);
            _gl.VertexArrayAttribFormat(VertexArrayHandle, 2, 2, VertexAttribType.Float, false, 24);

            // All our attributes will come from the same buffer, so we bind all of them to buffer (0).
            _gl.VertexArrayAttribBinding(VertexArrayHandle, 0, 0);
            _gl.VertexArrayAttribBinding(VertexArrayHandle, 1, 0);
            _gl.VertexArrayAttribBinding(VertexArrayHandle, 2, 0);

            // Finally, we bind the buffers.
            _gl.VertexArrayVertexBuffer(VertexArrayHandle, 0, VertexBufferHandle, 0, (uint)Marshal.SizeOf<Vertex>());
            _gl.VertexArrayElementBuffer(VertexArrayHandle, IndexBufferHandle);
        }

        public void Dispose()
        {
            _gl.DeleteBuffer(VertexBufferHandle);
            _gl.DeleteBuffer(IndexBufferHandle);
            _gl.DeleteVertexArray(VertexArrayHandle);
        }

        public void Draw()
        {
            unsafe
            {
                _gl.BindVertexArray(VertexArrayHandle);
                _gl.DrawElements(PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, null);
            }
        }
    }
}
