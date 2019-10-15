using SFML.Graphics;
using SFML.System;

namespace GravityGame.Guis
{
	public class BasicContainer : Container
	{
		public Color BackgroundColor { get; set; }
		public Color BorderColor { get; set; }
		public float BorderThickness { get; set; }
		
		public override void Draw(RenderTarget target, RenderStates states)
		{
			base.Draw(target, states);
			
			foreach (GuiEntry entry in Entries)
			{
				target.Draw(entry);
			}
		}

		public override Vector2i GetChildAbsolutePosition(GuiEntry child)
		{
			return GetAbsolutePosition() + new Vector2i(child.Margin.Left, child.Margin.Top);
		}
	}
}