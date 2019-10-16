using System;
using System.IO;

namespace GlslIncludeProcessor
{
	class Program
	{
		//Args - Directory to process, extensions to process
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