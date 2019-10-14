using SFML.Graphics;
using SFML.Graphics.Glsl;

namespace GravityGame.Graphics
{
	public class RockyShader
	{
		public static Shader Shader { get; private set; }

		public static Texture IceTexture
		{
			set => Shader.SetUniform("ice_texture", value);
		}
		
		public static Texture LandTexture
		{
			set => Shader.SetUniform("land_texture", value);
		}
		
		public static float Temperature
		{
			set => Shader.SetUniform("temp", value);
		}

		public static float WaterPercentage
		{
			set => Shader.SetUniform("water_percentage", value);
		}

		public static float Seed
		{
			set => Shader.SetUniform("seed", value);
		}

		public static float IcePercentage
		{
			set => Shader.SetUniform("ice_percentage", value);
		}

		public static float Radius
		{
			set => Shader.SetUniform("radius", value);
		}

		public static Colorf AtmosphereColor
		{
			set => Shader.SetUniform("atmo_color", value.ToVec4());
		}

		public static float AtmosphereStrength
		{
			set => Shader.SetUniform("atmo_strength", value);
		}
		
		static RockyShader()
		{
			Shader = new Shader(null, null, Program.GetResourcesDirectory() + "\\Shaders\\rocky.frag");
		}

		public static void Load(Texture texture)
		{
			Shader.SetUniform("texture", texture);
			Shader.SetUniform("time", Program.Time);
		}
	}
}