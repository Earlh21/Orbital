using SFML.Graphics;

namespace GravityGame.Guis.PrebuiltGuis
{
	public class PlanetInfoGui
	{
		private Gui gui;
		private Planet planet;

		private GuiText planet_name;

		public PlanetInfoGui(Planet planet)
		{
			gui = new Gui();
			
			gui.Contents.BackgroundColor = new Color(100, 100, 100, 60);
			gui.Contents.Margin = new Margin(20, 0, 0, 20);
			
			ColumnContainer column = (ColumnContainer)gui.Contents.AddEntry(new ColumnContainer());
			column.Margin = new Margin(5, 5, 5, 5);
			
			planet_name = new GuiText();
			planet_name.Contents = "Planet Name";
			planet_name.Font = Program.Font;
			planet_name.FontSize = 0.5f;
			planet_name.Color = Color.White;

			column.AddEntry(planet_name);
			
			Bar bar = new Bar(120, 3);
			bar.Margin = new Margin(5, 0, 10, 0);
			
			column.AddEntry(bar);
		}

		public Gui GetGUI()
		{
			return gui;
		}
	}
}