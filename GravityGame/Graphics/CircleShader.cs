using SFML.Graphics;
using SFML.Graphics.Glsl;

namespace GravityGame.Graphics
{
	public static class CircleShader
	{
		public static Shader Shader { get; private set; }
		
		static CircleShader()
		{
			Shader = new Shader(null, null, Program.GetResourcesDirectory() + "\\Shaders\\circle.frag");
		}

		public static void Load(Texture texture, Colorf color)
		{
			Shader.SetUniform("texture", texture);
			Shader.SetUniform("color", new Vec4(color.R, color.G, color.B, color.A));
		}
	}
}