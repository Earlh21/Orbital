using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace GravityGame.Guis
{
	public abstract class Container : GuiEntry
	{
		protected List<GuiEntry> Entries { get; set; }

		public Container()
		{
			Entries = new List<GuiEntry>();
		}
		
		public virtual GuiEntry AddEntry(GuiEntry entry)
		{
			Entries.Add(entry);
			entry.Parent = this;
			return entry;
		}

		public abstract Vector2i GetChildAbsolutePosition(GuiEntry child);
	}
}