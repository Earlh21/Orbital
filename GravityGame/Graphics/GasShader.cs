using SFML.Graphics;

namespace GravityGame.Graphics
{
	public class GasShader
	{
		public static Shader Shader { get; private set; }

		public static float Radius
		{
			set => Shader.SetUniform("radius", value);
		}

		static GasShader()
		{
			Shader = new Shader(null, null, Program.GetResourcesDirectory() + "\\Shaders\\gas.frag");
		}

		public static void Load(Texture texture)
		{
			Shader.SetUniform("texture", texture);
			//Shader.SetUniform("time", Program.Time);
		}
	}
}