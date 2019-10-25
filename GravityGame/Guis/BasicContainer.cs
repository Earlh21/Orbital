using SFML.Graphics;
using SFML.System;

namespace GravityGame.Guis
{
	public class BasicContainer : Container
	{
		public override Vector2i Size
		{
			get
			{
				int max_x = 0;
				int max_y = 0;

				foreach (GuiEntry entry in Entries)
				{
					int total_x = entry.Margin.Left + entry.Size.X + entry.Margin.Right;
					int total_y = entry.Margin.Top + entry.Size.Y + entry.Margin.Bottom;
					
					if (total_x > max_x)
					{
						max_x = total_x;
					}

					if (total_y > max_y)
					{
						max_y = total_y;
					}
				}
				
				return new Vector2i(max_x, max_y);
			}
		}

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