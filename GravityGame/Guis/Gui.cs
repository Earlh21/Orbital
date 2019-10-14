using SFML.Graphics;

namespace GravityGame.Guis
{
	public class Gui : Drawable
	{
		public BasicContainer Contents { get; set; }

		public Gui()
		{
			Contents = new BasicContainer();
		}
		
		public void Draw(RenderTarget target, RenderStates states)
		{
			target.Draw(Contents);
		}
	}
}