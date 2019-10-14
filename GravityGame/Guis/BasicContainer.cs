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
			
			//TODO: Draw background in screen space
			/**RectangleShape background = new RectangleShape(Size);
			background.FillColor = BackgroundColor;
			background.OutlineColor = BorderColor;
			background.OutlineThickness = BorderThickness;**/
			
			foreach (GuiEntry entry in Entries)
			{
				target.Draw(entry);
			}
		}

		public override Vector2i GetAbsolutePosition(GuiEntry child)
		{
			if (Parent == null)
			{
				return new Vector2i(child.Margin.Left, child.Margin.Top);
			}
			
			return Parent.GetAbsolutePosition(child) + new Vector2i(child.Margin.Left, child.Margin.Top);
		}
	}
}