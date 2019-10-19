using System;

namespace GravityGame.File
{
	public static class IOManager
	{
		public static string GetWorkingDirectory()
		{
			return AppDomain.CurrentDomain.BaseDirectory;
		}
		
		public static string GetResourcesDirectory()
		{
			return GetWorkingDirectory() + "Resources\\";
		}
		
		public static string GetFragmentShaderPath(string name)
		{
			return GetResourcesDirectory() + "Shaders\\" + name + ".frag";
		}
	}
}