using SFML.Graphics;
using SFML.System;

namespace GravityGame.Guis
{
	public class RowContainer : Container
	{
		public int ColumnWidth { get; set; }

		public override Vector2i Size
		{
			get
			{
				int width = 0;
				int max_height = 0;
				
				foreach (GuiEntry entry in Entries)
				{
					width += entry.Size.X + entry.Margin.Left + entry.Margin.Right;
					int height = entry.Size.Y + entry.Margin.Top + entry.Margin.Bottom;
					max_height = height > max_height ? height : max_height;
				}
				
				return new Vector2i(width, max_height);
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

			int padding_left = 0;

			for (int i = 0; i < index; i++)
			{
				padding_left += Entries[i].Size.X + Entries[i].Margin.Left + Entries[i].Margin.Right;
			}

			padding_left += Entries[index].Margin.Left;

			return GetAbsolutePosition() + new Vector2i(index * ColumnWidth + padding_left, Entries[index].Margin.Top);
		}
	}
}