using SFML.Graphics;
using SFML.System;

namespace GravityGame.Guis
{
	public class GuiText : GuiEntry
	{
		public Color Color { get; set; }
		public Font Font { get; set; }
		public float FontSize { get; set; }
		public string Contents { get; set; }

		public override Vector2i Size
		{
			get
			{
				Text text = new Text(Contents, Font);

				text.CharacterSize = 80;
				text.Scale = new Vector2f(GetRealScale(Program.Window), GetRealScale(Program.Window));
				FloatRect bounds = text.GetGlobalBounds();
				return Program.WorldSizeToScreen(new Vector2f(bounds.Width, bounds.Height));
			}
		}

		public override void Draw(RenderTarget target, RenderStates states)
		{
			base.Draw(target, states);
			
			Text text = new Text(Contents, Font);

			text.CharacterSize = 80;
			text.Scale = new Vector2f(GetRealScale(Program.Window), GetRealScale(Program.Window));
			text.FillColor = Color;
			
			FloatRect bounds = text.GetGlobalBounds();
			Vector2f offset = new Vector2f(bounds.Left, bounds.Top);
			text.Position = Program.ScreenPositionToWorld(GetAbsolutePosition()) - offset;
			
			target.Draw(text);
		}

		private float GetRealScale(RenderWindow window)
		{
			View view = window.GetView();
			return FontSize / 3.0f * view.Size.X / window.Size.X;
		}
	}
}