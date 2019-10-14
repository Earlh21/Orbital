using SFML.Graphics;
using SFML.System;

namespace GravityGame.Guis
{
	public abstract class GuiEntry : Drawable
	{
		public Container Parent { get; internal set; }

		public Margin Margin { get; set; }
		public virtual Vector2i Size { get; }

		public abstract void Draw(RenderTarget target, RenderStates states);

		public GuiEntry()
		{
		}

		public GuiEntry(Vector2i size)
		{
			Size = size;
		}

		public virtual Vector2i GetAbsolutePosition(GuiEntry child)
		{
			if (Parent == null)
			{
				return new Vector2i(Margin.Left, Margin.Top);
			}
			
			return Parent.GetAbsolutePosition(this);
		}
	}
}