using Silk.NET.OpenGL;
using System;

namespace GLSample.Rendering
{
    public enum FilterMode
    {
        Nearest,
        Linear
    }

    public enum WrapMode
    {
        Clamp,
        Repeat
    }

    public struct GLTextureDescriptor
    {
        public TextureTarget dimension;
        public uint mipCount;
        public SizedInternalFormat format;
        public uint width;
        public uint height;
        public uint depth;

        public GLTextureDescriptor(uint width, uint height, SizedInternalFormat format, uint mipCount = 1)
        {
            this.dimension = TextureTarget.Texture2D;
            this.mipCount = mipCount;
            this.format = format;
            this.width = width;
            this.height = height;
            this.depth = 0;
        }
    }

    public class GLTexture : IDisposable
    {
        public GLTextureDescriptor Descriptor { get; }
        public uint Handle { get; }

        private GL _gl;

        internal GLTexture(GL gl, in GLTextureDescriptor desc)
        {
            _gl = gl;
            Handle = gl.CreateTexture(desc.dimension);
            Descriptor = desc;

            // Allocate the texture storage.
            switch (Descriptor.dimension)
            {
                case TextureTarget.Texture2D:
                    _gl.TextureStorage2D(Handle, Descriptor.mipCount, Descriptor.format, Descriptor.width, Descriptor.height);
                    break;
                case TextureTarget.Texture3D:
                    _gl.TextureStorage3D(Handle, Descriptor.mipCount, Descriptor.format, Descriptor.width, Descriptor.height, Descriptor.depth);
                    break;
                default:
                    throw new Exception($"TextureTarget {Descriptor.dimension} not implemented yet.");
            }
        }

        public void Dispose()
        {
            _gl.DeleteTexture(Handle);
        }

        public void SetData<T>(
            ReadOnlySpan<T> pixels,
            PixelFormat pixelFormat,
            PixelType pixelType,
            int mipLevel = 0
        ) where T : unmanaged
        {
            _gl.TextureSubImage2D(Handle, mipLevel, 0, 0, Descriptor.width, Descriptor.height, pixelFormat, pixelType, pixels);
        }

        public void GenerateAllMips()
        {
            _gl.GenerateTextureMipmap(Handle);
        }

        public void SetWrapMode(WrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case WrapMode.Clamp:
                    _gl.TextureParameterI(Handle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    _gl.TextureParameterI(Handle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                    break;
                case WrapMode.Repeat:
                    _gl.TextureParameterI(Handle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    _gl.TextureParameterI(Handle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    break;
            }
        }

        public void SetFilterMode(FilterMode filterMode)
        {
            switch (filterMode)
            {
                case FilterMode.Nearest:
                    _gl.TextureParameterI(Handle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    _gl.TextureParameterI(Handle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    break;
                case FilterMode.Linear:
                    _gl.TextureParameterI(Handle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    _gl.TextureParameterI(Handle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    break;
            }
        }
    }
}