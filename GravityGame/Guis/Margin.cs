namespace GravityGame.Guis
{
	public struct Margin
	{
		public int Top { get; }
		public int Right { get; }
		public int Bottom { get; }
		public int Left { get; }

		public Margin(int top, int right, int bottom, int left)
		{
			Top = top;
			Right = right;
			Bottom = bottom;
			Left = left;
		}
	}
}