using Silk.NET.OpenGL;
using System;
using System.Numerics;
using System.Xml.Linq;

namespace GLSample.Rendering
{
    public class GLShader : IDisposable
    {
        private GL _gl;

        public uint ProgramHandle { get; }
        public GLShaderMetadata Metadata { get; }

        internal GLShader(GL gl, string vertexSource, string fragmentSource)
        {
            _gl = gl;
            var vertexStageHandle = CreateOpenGLShader(gl, ShaderType.VertexShader, vertexSource);
            var fragmentStageHandle = CreateOpenGLShader(gl, ShaderType.FragmentShader, fragmentSource);
            ProgramHandle = CreateOpenGLProgram(gl, vertexStageHandle, fragmentStageHandle);
            gl.DeleteShader(vertexStageHandle);
            gl.DeleteShader(fragmentStageHandle);

            Metadata = GLShaderMetadata.ExtractFromShader(gl, this);
        }

        public void Dispose()
        {
            _gl.DeleteProgram(ProgramHandle);
        }

        public void Use()
        {
            _gl.UseProgram(ProgramHandle);
        }

        public void SetBool(string name, bool value) => SetBool(GLShaderUniformId.FromName(name), value);
        public void SetInt(string name, int value) => SetInt(GLShaderUniformId.FromName(name), value);
        public void SetFloat(string name, float value) => SetFloat(GLShaderUniformId.FromName(name), value);
        public void SetVector(string name, Vector3 value) => SetVector(GLShaderUniformId.FromName(name), value);
        public void SetVector(string name, Vector4 value) => SetVector(GLShaderUniformId.FromName(name), value);
        public void SetMatrix(string name, Matrix4x4 value) => SetMatrix(GLShaderUniformId.FromName(name), value);
        public void SetTexture(string name, GLTexture value, int unit) => SetTexture(GLShaderUniformId.FromName(name), value, unit);

        public void SetBool(GLShaderUniformId id, bool value)
        {
            var location = Metadata.GetUniformLocation(id);
            if (location != -1)
            {
                if (value)
                    _gl.ProgramUniform1(ProgramHandle, location, 1);
                else
                    _gl.ProgramUniform1(ProgramHandle, location, 0);
            }
        }
        public void SetInt(GLShaderUniformId id, int value)
        {
            var location = Metadata.GetUniformLocation(id);
            if (location != -1)
            {
                _gl.ProgramUniform1(ProgramHandle, location, value);
            }
        }
        public void SetFloat(GLShaderUniformId id, float value)
        {
            var location = Metadata.GetUniformLocation(id);
            if (location != -1)
            {
                _gl.ProgramUniform1(ProgramHandle, location, value);
            }
        }

        public void SetVector(GLShaderUniformId id, Vector3 value)
        {
            var location = Metadata.GetUniformLocation(id);
            if (location != -1)
            {
                _gl.ProgramUniform3(ProgramHandle, location, value);
            }
        }

        public void SetVector(GLShaderUniformId id, Vector4 value)
        {
            var location = Metadata.GetUniformLocation(id);
            if (location != -1)
            {
                _gl.ProgramUniform4(ProgramHandle, location, value);
            }
        }

        public void SetMatrix(GLShaderUniformId id, Matrix4x4 value)
        {
            var location = Metadata.GetUniformLocation(id);
            if (location != -1)
            {
                unsafe
                {
                    _gl.ProgramUniformMatrix4(ProgramHandle, location, 1, false, (float*) &value);
                }
            }
        }

        public void SetTexture(GLShaderUniformId id, GLTexture value, int unit)
        {
            if (value == null)
                return;

            var location = Metadata.GetUniformLocation(id);
            if (location != -1)
            {
                _gl.BindTextureUnit((uint)unit, value.Handle);
                _gl.ProgramUniform1(ProgramHandle, location, unit);
            }
        }

        private static uint CreateOpenGLShader(GL gl, ShaderType type, string source)
        {
            uint handle = gl.CreateShader(type);
            gl.ShaderSource(handle, source);
            gl.CompileShader(handle);
            gl.GetShader(handle, ShaderParameterName.CompileStatus, out int compiled);
            if (compiled == 0)
            {
                var log = gl.GetShaderInfoLog(handle);
                gl.DeleteShader(handle);
                throw new Exception($"Shader Compilation Failed:\n{log}");
            }

            return handle;
        }

        private static uint CreateOpenGLProgram(GL gl, uint vertexHandle, uint fragmentHandle)
        {
            uint handle = gl.CreateProgram();
            gl.AttachShader(handle, vertexHandle);
            gl.AttachShader(handle, fragmentHandle);
            gl.LinkProgram(handle);

            gl.GetProgram(handle, ProgramPropertyARB.LinkStatus, out var linked);
            if (linked == 0)
            {
                var log = gl.GetProgramInfoLog(handle);
                gl.DeleteProgram(handle);
                gl.DeleteShader(vertexHandle);
                gl.DeleteShader(fragmentHandle);
                throw new Exception($"Shader Compilation Failed:\n{log}");
            }

            gl.DetachShader(handle, vertexHandle);
            gl.DetachShader(handle, fragmentHandle);
            return handle;
        }
    }
}