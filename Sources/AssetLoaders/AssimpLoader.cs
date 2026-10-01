using GLSample.Rendering;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using System.Diagnostics;
using System.Numerics;

namespace GLSample.AssetLoaders
{
    public static class AssimpLoader
    {
        private static readonly Assimp kAssimp = Assimp.GetApi();

        public static GLMesh[] LoadMeshes(GL gl, string path)
        {
            var flags = PostProcessSteps.Triangulate;
            unsafe
            {
                var scene = kAssimp.ImportFile(path, (uint)flags);
                var meshCount = scene->MNumMeshes;

                var meshes = new GLMesh[meshCount];
                for (int i = 0; i < meshCount; i++)
                {
                    meshes[i] = ConvertMesh(gl, ref *(scene->MMeshes[i]));
                }

                return meshes;
            }
        }

        private unsafe static GLMesh ConvertMesh(GL gl, ref Mesh mesh)
        {

            int vertexCount = (int)mesh.MNumVertices;
            int faceCount = (int)mesh.MNumFaces;

            GLMesh.Vertex[] vertices = new GLMesh.Vertex[vertexCount];
            uint[] indices = new uint[faceCount * 3];

            for (int i = 0; i < vertexCount; i++)
            {
                ref var vertex = ref vertices[i];

                vertex.position = mesh.MVertices[i];

                if (mesh.MNormals != null)
                {
                    vertex.normal = mesh.MNormals[i];
                }

                if (mesh.MTextureCoords[0] != null)
                {
                    var texCoord0 = mesh.MTextureCoords[0][i];
                    vertex.texCoords = new Vector2(texCoord0.X, texCoord0.Y);
                }
            }

            for (int i = 0; i < faceCount; i++)
            {
                var face = mesh.MFaces[i];
                indices[i * 3 + 0] = face.MIndices[0];
                indices[i * 3 + 1] = face.MIndices[1];
                indices[i * 3 + 2] = face.MIndices[2];
            }

            return new GLMesh(gl, vertices, indices);
        }
    }
}
