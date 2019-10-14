using NUnit.Framework;
using SFML.Graphics;

namespace GravityGame.Graphics
{
	public static class StarShader
	{
		public static Shader Shader { get; private set; }

		public static Colorf DimColor
		{
			set => Shader.SetUniform("color1", value.ToVec4());
		}

		public static Colorf BrightColor
		{
			set => Shader.SetUniform("color2", value.ToVec4());
		}

		public static float Radius
		{
			set => Shader.SetUniform("radius", value);
		}
		
		static StarShader()
		{
			Shader = new Shader(null, null, Program.GetResourcesDirectory() + "\\Shaders\\star.frag");
		}

		public static void Load(Texture texture)
		{
			Shader.SetUniform("texture", texture);
			Shader.SetUniform("time", Program.Time);
		}
	}
}