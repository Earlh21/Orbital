using Microsoft.Win32.SafeHandles;
using SFML.Graphics;
using SFML.System;

namespace GravityGame.Guis
{
	public abstract class GuiEntry : Drawable
	{
		public Container Parent { get; internal set; }
		public Margin Margin { get; set; }
		public virtual Vector2i Size { get; }

		public Color BackgroundColor { get; set; } = Color.Transparent;
		public Color BorderColor { get; set; } = Color.Transparent;
		public int BorderThickness { get; set; }

		public virtual void Draw(RenderTarget target, RenderStates states)
		{
			if (this is BasicContainer basic)
			{
				int d = 3;
			}
			
			Vector2i size = Size;
			Vector2f world_size = Program.ScreenSizeToWorld(size);
			
			RectangleShape background = new RectangleShape(world_size);
			background.FillColor = BackgroundColor;
			background.OutlineColor = BorderColor;
			background.OutlineThickness = BorderThickness;

			background.Position = Program.ScreenPositionToWorld(GetAbsolutePosition());
			target.Draw(background);
		}

		public GuiEntry()
		{
		}

		public Vector2i GetAbsolutePosition()
		{
			if (Parent == null)
			{
				return new Vector2i(Margin.Left, Margin.Top);
			}

			return Parent.GetChildAbsolutePosition(this);
		}
	}
}