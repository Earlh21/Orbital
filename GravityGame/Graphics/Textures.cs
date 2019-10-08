using SFML.Graphics;

namespace GravityGame.Graphics
{
	public static class Textures
	{
		public static readonly Texture RedRocky;
		
		public static readonly Texture Ice;

		static Textures()
		{
			string resources = Program.GetResourcesDirectory() + "\\Textures\\";
			RedRocky = new Texture(resources + "red_rocky.jpg");
			Ice = new Texture(resources + "ice.jpg");
		}
	}
}