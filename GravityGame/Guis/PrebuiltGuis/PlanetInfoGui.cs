using System;
using SFML.Graphics;

namespace GravityGame.Guis.PrebuiltGuis
{
	public class PlanetInfoGui
	{
		private Gui gui;
		private Planet planet;

		private ColumnContainer main_column;
		private ColumnContainer composition_column;

		private GuiText mass_text;
		private GuiText radius_text;
		private GuiText density_text;
		private GuiText temperature_text;

		public PlanetInfoGui(Planet planet)
		{
			if (planet == null)
			{
				throw new NullReferenceException();
			}

			this.planet = planet;
			gui = new Gui();
			
			gui.Contents.BackgroundColor = new Color(100, 100, 100, 60);
			gui.Contents.Margin = new Margin(20, 0, 0, 20);
			
			main_column = (ColumnContainer)gui.Contents.AddEntry(new ColumnContainer());
			main_column.Margin = new Margin(5, 5, 5, 5);
			
			GuiText planet_name = new GuiText();
			planet_name.Contents = "Planet Name";
			FormatTitle(planet_name);

			main_column.AddEntry(planet_name);
			
			Bar bar = new Bar(200, 3);
			bar.Margin = new Margin(4, 0, 15, 0);
			
			main_column.AddEntry(bar);

			GuiText properties_text = new GuiText();
			properties_text.Contents = "Properties";
			FormatHeader(properties_text);

			main_column.AddEntry(properties_text);

			Bar bar2 = new Bar(200, 2);
			bar2.Margin = new Margin(2, 0, 5, 0);
			bar2.Color = new Color(200, 200, 200, 255);

			main_column.AddEntry(bar2);
			
			mass_text = new GuiText();
			mass_text.Margin = new Margin(2, 0, 0, 0);
			radius_text = new GuiText();
			radius_text.Margin = new Margin(2, 0, 0, 0);
			density_text = new GuiText();
			density_text.Margin = new Margin(2, 0, 0, 0);
			temperature_text = new GuiText();
			temperature_text.Margin = new Margin(2, 0, 0, 0);
			
			FormatSmallText(mass_text);
			FormatSmallText(radius_text);
			FormatSmallText(density_text);
			FormatSmallText(temperature_text);

			main_column.AddEntry(mass_text);
			main_column.AddEntry(radius_text);
			main_column.AddEntry(density_text);
			main_column.AddEntry(temperature_text);
			
			GuiText composition_text = new GuiText();
			composition_text.Contents = "Composition";
			composition_text.Margin = new Margin(5, 0, 0, 0);
			FormatHeader(composition_text);

			main_column.AddEntry(composition_text);

			Bar bar3 = new Bar(200, 2);
			bar3.Margin = new Margin(4, 0, 0, 0);
			bar3.Color = new Color(200, 200, 200, 255);

			main_column.AddEntry(bar2);
			
			
		}
		
		

		public void Update()
		{
			mass_text.Contents = "Mass: " + planet.Mass;
			radius_text.Contents = "Radius: " + planet.Radius;
			density_text.Contents = "Density: " + planet.Density;
			temperature_text.Contents = "Temperature: " + planet.Temperature + "K";
		}
		
		private void FormatTitle(GuiText text)
		{
			text.Font = Program.Font;
			text.FontSize = 1.5f;
			text.Color = Color.White;
		}
		
		private void FormatHeader(GuiText text)
		{
			text.Font = Program.Font;
			text.FontSize = 0.9f;
			text.Color = Color.White;
		}
		
		private void FormatSmallText(GuiText text)
		{
			text.Font = Program.Font;
			text.FontSize = 0.6f;
			text.Color = Color.White;
		}

		public Gui GetGUI()
		{
			if (planet == null || !planet.Exists)
			{
				return null;
			}
			
			return gui;
		}
	}
}