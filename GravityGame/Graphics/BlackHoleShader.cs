using GravityGame.Extension;
using GravityGame.File;
using SFML.Graphics;
using SFML.System;

namespace GravityGame.Graphics
{
	public class BlackHoleShader
	{
		public static Shader Shader { get; private set; }

		public static Vector2f Position
		{
			set
			{
				Shader.SetUniform("position", value);
			}
		}

		public static float Lensing
		{
			set
			{
				Shader.SetUniform("lensing", value);
			}
		}
		
		static BlackHoleShader()
		{
			Shader = new Shader(null, null, IOManager.GetFragmentShaderPath("blackhole"));
		}
		
		public static void Load(Texture texture)
		{
			Shader.SetUniform("screen", texture);
			Shader.SetUniform("view_size", Program.ViewSize);
			Shader.SetUniform("view_offset", Program.ViewOffset.InvY());
		}
	}
}