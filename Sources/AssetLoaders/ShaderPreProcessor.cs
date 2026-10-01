using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GLSample.AssetLoaders
{
    public static class ShaderPreProcessor
    {
        public const string kIncludePattern = @"^[ \t]*#include\s""(.*)""[\s]*$";

        public static string ProcessShaderSource(string source)
        {
            return ProcessIncludes(source);
        }

        private static string ProcessIncludes(string source)
        {
            var sourceBuilder = new StringBuilder(source);
            var includeMatches = Regex.Matches(source, kIncludePattern, RegexOptions.Multiline);

            foreach (Match match in includeMatches)
            {
                var path = match.Groups[1].Value;
                if (!File.Exists(path))
                {
                    continue;
                }

                var includeContent = File.ReadAllText(path);
                sourceBuilder.Replace(match.Value, includeContent);
            }

            return sourceBuilder.ToString();
        }
    }
}