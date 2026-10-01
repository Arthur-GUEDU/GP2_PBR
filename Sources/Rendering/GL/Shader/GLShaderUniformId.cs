using System;

namespace GLSample.Rendering
{
    public struct GLShaderUniformId : IEquatable<GLShaderUniformId>
    {
        private int _hash;

        private GLShaderUniformId(int hash)
        {
            _hash = hash;
        }

        public static GLShaderUniformId FromName(string name)
        {
            return new GLShaderUniformId(name.GetHashCode());
        }

        public bool Equals(GLShaderUniformId other) => _hash == other._hash;
        public override int GetHashCode() => _hash;

        public static implicit operator int(in GLShaderUniformId id)
        {
            return id._hash;
        }
    }
}
