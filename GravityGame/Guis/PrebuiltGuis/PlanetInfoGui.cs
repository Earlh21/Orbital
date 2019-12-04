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

		private PropertyText mass_property;
		private PropertyText radius_property;
		private PropertyText density_property;
		private PropertyText temperature_property;

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
			
			mass_property = new PropertyText("Mass: ", planet.Mass, "", FormatSmallText);
			FormatProperty(mass_property);
			mass_property.Margin = new Margin(2, 0, 0, 0);
			radius_property = new PropertyText("Radius: ", planet.Radius, "", FormatSmallText);
			FormatProperty(radius_property);
			radius_property.Margin = new Margin(2, 0, 0, 0);
			density_property = new PropertyText("Density: ", planet.Density, "", FormatSmallText);
			FormatProperty(density_property);
			density_property.Margin = new Margin(2, 0, 0, 0);
			temperature_property = new PropertyText("Temperature: ", planet.Temperature, " K", FormatSmallText);

			main_column.AddEntry(mass_property);
			main_column.AddEntry(radius_property);
			main_column.AddEntry(density_property);
			main_column.AddEntry(temperature_property);
			
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
			mass_property.Value = planet.Mass;
			radius_property.Value = planet.Radius;
			density_property.Value = planet.Density;
			temperature_property.Value = planet.Temperature;

			temperature_property.Value = 1;
		}

		private void FormatProperty(PropertyText property)
		{
			property.DecimalColor = new Color(200, 200, 200, 255);
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

		private class PropertyText : RowContainer
		{
			private GuiText prepend_text;
			private GuiText unit_text;
			private GuiText decimal_text;
			private GuiText append_text;

			public Color AppendColor
			{
				get => append_text.Color;
				set => append_text.Color = value;
			}

			public Color PrependColor
			{
				get => prepend_text.Color;
				set => prepend_text.Color = value;
			}

			public Color UnitColor
			{
				get => unit_text.Color;
				set => unit_text.Color = value;
			}

			public Color DecimalColor
			{
				get => decimal_text.Color;
				set => decimal_text.Color = value;
			}

			public float Value
			{
				set
				{
					unit_text.Contents = ((int) value).ToString();

					string decimal_part = (value - (int) value).ToString();

					int length = decimal_part.Length;
					string d_text = ".";
					if (length > 3)
					{
						d_text += decimal_part.Substring(2, 2);
					}
					else if (length == 3)
					{
						d_text += decimal_part.Substring(2, 1) + "0";
					}
					else if(length == 1)
					{
						d_text += "00";
					}
					else
					{
						throw new Exception("Failure to parse decimal.");
					}

					decimal_text.Contents = d_text;
				}
			}
			
			public PropertyText(string prepend, float value, string append, Action<GuiText> format)
			{
				prepend_text = new GuiText();
				format(prepend_text);
				prepend_text.Contents = prepend;

				unit_text = new GuiText();
				format(unit_text);
				
				decimal_text = new GuiText();
				decimal_text.Margin = new Margin(0, 0, 0, 2);
				format(decimal_text);
				
				append_text = new GuiText();
				format(append_text);
				append_text.Contents = append;

				Value = value;

				PrependColor = Color.White;
				UnitColor = Color.White;
				DecimalColor = Color.White;
				AppendColor = Color.White;
				
				AddEntry(prepend_text);
				AddEntry(unit_text);
				AddEntry(decimal_text);
				AddEntry(append_text);
			}
		}
	}
}