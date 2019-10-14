using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace GravityGame.Guis
{
	public abstract class Container : GuiEntry
	{
		protected List<GuiEntry> Entries { get; set; }

		public abstract override void Draw(RenderTarget target, RenderStates states);

		public Container()
		{
			Entries = new List<GuiEntry>();
		}
		
		public virtual void AddEntry(GuiEntry entry)
		{
			Entries.Add(entry);
			entry.Parent = this;
		}
	}
}