using SFML.Graphics;
using SFML.System;

namespace GravityGame.Guis
{
	public class Bar : GuiEntry
	{
		private int width;
		private int height;

		public override Vector2i Size
		{
			get
			{
				return new Vector2i(width + Margin.Left + Margin.Right, height + Margin.Top + Margin.Bottom);
			}
		}

		public Color Color { get; set; } = Color.White;

		public Bar(int width, int height)
		{
			this.width = width;
			this.height = height;
		}
		
		public override void Draw(RenderTarget target, RenderStates states)
		{
			base.Draw(target, states);
			
			RectangleShape bar = new RectangleShape();
			bar.Size = Program.ScreenSizeToWorld(new Vector2i(width, height));
			bar.Position = Program.ScreenPositionToWorld(GetAbsolutePosition());
			bar.FillColor = Color;

			target.Draw(bar);
		}
	}
}