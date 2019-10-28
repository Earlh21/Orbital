using SFML.Graphics;

namespace GravityGame.Guis.PrebuiltGuis
{
	public class PlanetInfoGui
	{
		private Gui gui;
		private Planet planet;

		private GuiText planet_name;
		private GuiText composition_text;

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
			planet_name.FontSize = 1f;
			planet_name.Color = Color.White;

			column.AddEntry(planet_name);
			
			Bar bar = new Bar(200, 3);
			bar.Margin = new Margin(20, 0, 0, 0);
			
			column.AddEntry(bar);

			composition_text = new GuiText();
			composition_text.Contents = "Composition";
			composition_text.Font = Program.Font;
			composition_text.FontSize = 0.5f;
			composition_text.Color = Color.White;

			column.AddEntry(composition_text);

			Bar bar2 = new Bar(composition_text.Size.X, 2);
			bar.Margin = new Margin(0, 0, 0, 0);
			bar2.Color = new Color(200, 200, 200, 255);

			column.AddEntry(bar2);
			
			
		}

		public Gui GetGUI()
		{
			return gui;
		}
	}
}