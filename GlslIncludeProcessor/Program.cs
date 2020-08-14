using System;
using System.IO;

namespace GlslIncludeProcessor
{
	class Program
	{
		/// <summary>
		/// This tool processes #include directives (not actually used by GLSL) to recursively inline library functions.
		/// Processing will stop if a circular dependency is found.
		/// </summary>
		/// <param name="args">Directory of shaders to process, file search pattern (extensions)</param>
		static void Main(string[] args)
		{	
			String[] files = Directory.GetFiles(args[0], args[1], SearchOption.AllDirectories);

			foreach (String file in files)
			{
				Shader shader = new Shader(file);
				File.WriteAllText(file, shader.Process());
			}
		}
	}
}