using GLSample.Rendering;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace GLSample.AssetLoaders
{
    public class TextureLoader
    {
        public static GLTexture LoadRGBA8Texture2DFromFile(GL gl, string path, bool useMips = true)
        {
            Image<Rgba32> image = (Image<Rgba32>) Image.Load(path);
            image.Mutate(ctx => ctx.Flip(FlipMode.Vertical));

            var pixels = new Rgba32[image.Width * image.Height];
            image.CopyPixelDataTo(pixels);

            uint mipCount = 1;
            if (useMips)
            {
                // Compute the maximum number of mipmaps.
                mipCount = (uint) MathF.Floor(MathF.Log2(Math.Max(image.Width, image.Height))) + 1;
            }

            var textureDescriptor = new GLTextureDescriptor((uint)image.Width, (uint)image.Height, SizedInternalFormat.Rgba8, mipCount);
            var texture = new GLTexture(gl, textureDescriptor);
            texture.SetFilterMode(FilterMode.Linear);
            texture.SetWrapMode(WrapMode.Clamp);

            texture.SetData<Rgba32>(pixels, PixelFormat.Rgba, PixelType.UnsignedByte);
            if (useMips && textureDescriptor.mipCount > 1)
            {
                texture.GenerateAllMips();
            }

            return texture;
        }

        public static GLTexture LoadRGB8Texture2DFromFile(GL gl, string path, bool useMips = true)
        {
            Image<Rgb24> image = (Image<Rgb24>)Image.Load(path);

            var pixels = new Rgb24[image.Width * image.Height];
            image.CopyPixelDataTo(pixels);

            uint mipCount = 1;
            if (useMips)
            {
                // Compute the maximum number of mipmaps.
                mipCount = (uint)MathF.Floor(MathF.Log2(Math.Max(image.Width, image.Height))) + 1;
            }

            var textureDescriptor = new GLTextureDescriptor((uint)image.Width, (uint)image.Height, SizedInternalFormat.Rgb8, mipCount);
            var texture = new GLTexture(gl, textureDescriptor);
            texture.SetFilterMode(FilterMode.Linear);
            texture.SetWrapMode(WrapMode.Clamp);

            texture.SetData<Rgb24>(pixels, PixelFormat.Rgb, PixelType.UnsignedByte);
            if (useMips && textureDescriptor.mipCount > 1)
            {
                texture.GenerateAllMips();
            }

            return texture;
        }
    }
}
