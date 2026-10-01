using System;
using System.Numerics;

namespace GLSample.Core
{
    public class Camera
    {
        public float FieldOfView { get; set; } = 60.0f;
        public float AspectRatio { get; set; } = 16.0f / 9.0f;
        public float NearPlane { get; set; } = 0.3f;
        public float FarPlane { get; set; } = 1000.0f;

        public Transform Transform { get; } = new Transform();

        public Matrix4x4 ProjectionMatrix => Matrix4x4.CreatePerspectiveFieldOfView(
                        FieldOfView * MathF.PI / 180f,
                        AspectRatio,
                        NearPlane,
                        FarPlane
            );

        public Camera() {}
    }
}
