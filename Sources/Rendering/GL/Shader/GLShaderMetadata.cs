using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;

namespace GLSample.Rendering
{
    public class GLShaderMetadata
    {
        private static ProgramResourceProperty[] kUniformProperties =
{
            ProgramResourceProperty.Type,
            ProgramResourceProperty.NameLength,
            ProgramResourceProperty.Location,
            ProgramResourceProperty.BlockIndex
        };

        private Dictionary<GLShaderUniformId, int> _uniformLocations = new Dictionary<GLShaderUniformId, int>(16);
        private Dictionary<string, int> _uniformNameLocations = new Dictionary<string, int>(16);

        private GLShaderMetadata() {}

        public int GetUniformLocation(string name) => GetUniformLocation(GLShaderUniformId.FromName(name));
        public int GetUniformLocation(GLShaderUniformId uniformId) 
        {
            if (_uniformLocations.TryGetValue(uniformId, out var location))
                return location;

            return -1;
        }

        public static GLShaderMetadata ExtractFromShader(GL gl, GLShader shader)
        {
            var metadata = new GLShaderMetadata();

            var programHandle = shader.ProgramHandle;
            int uniformCount = gl.GetProgramInterface(programHandle, ProgramInterface.Uniform, ProgramInterfacePName.ActiveResources);

            for (uint i = 0; i < uniformCount; i++)
            {
                var properties = new ReadOnlySpan<ProgramResourceProperty>(kUniformProperties);
                int[] propertyValues = new int[4]; // [Type, NameLength, Location, BlockIndex]

                gl.GetProgramResource(programHandle, ProgramInterface.Uniform, i, properties, out uint _, propertyValues.AsSpan());

                int type = propertyValues[0];
                int nameLength = propertyValues[1];
                int location = propertyValues[2];
                int blockIndex = propertyValues[3];

                if (blockIndex != -1)
                {
                    // We exclude properties not belonging to a uniform block.
                    continue;
                }

                gl.GetProgramResourceName(programHandle, ProgramInterface.Uniform, i, (uint) nameLength, out _, out string name);
                var uniformId = GLShaderUniformId.FromName(name);
                metadata._uniformLocations[uniformId] = location;
                metadata._uniformNameLocations[name] = location;
            }

            return metadata;
        }
    }
}
