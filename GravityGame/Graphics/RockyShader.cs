using SFML.Graphics;
using SFML.Graphics.Glsl;

namespace GravityGame.Graphics
{
	public class RockyShader
	{
		public static Shader Shader { get; private set; }
		
		static RockyShader()
		{
			Shader = new Shader(null, null, Program.GetResourcesDirectory() + "\\Shaders\\rocky.frag");
		}

		public static void Load(Texture texture, Colorf color, float temp, float water_percentage, float seed)
		{
			Shader.SetUniform("texture", texture);
			Shader.SetUniform("color", new Vec4(color.R, color.G, color.B, color.A));
			Shader.SetUniform("seed", seed);
			Shader.SetUniform("water_percentage", water_percentage);
			Shader.SetUniform("temp", temp);
		}
	}
}