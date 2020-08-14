using SFML.Graphics;
using SFML.System;

namespace GravityGame.Guis
{
	public class ColumnContainer : Container
	{
		public int LineHeight { get; set; }

		public override Vector2i Size
		{
			get
			{
				int height = 0;
				int max_width = 0;
				
				foreach (GuiEntry entry in Entries)
				{
					height += entry.Size.Y + entry.Margin.Top + entry.Margin.Bottom;
					int width = entry.Size.X + entry.Margin.Left + entry.Margin.Right;
					max_width = width > max_width ? width : max_width;
				}
				
				return new Vector2i(max_width, height);
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
			int index = Entries.IndexOf(child);

			int padding_above = 0;

			for (int i = 0; i < index; i++)
			{
				padding_above += Entries[i].Size.Y + Entries[i].Margin.Top + Entries[i].Margin.Bottom;
			}

			padding_above += Entries[index].Margin.Top;

			return GetAbsolutePosition() + new Vector2i(Entries[index].Margin.Left, index * LineHeight + padding_above);
		}
	}
}