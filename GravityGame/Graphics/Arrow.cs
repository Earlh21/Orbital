using System;
using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame.Graphics
{
    public class Arrow : Drawable
    {
        public Vector2f Position { get; set; }
        public Vector2f Direction { get; set; }
        public float Angle => (float)Math.Atan2(Direction.Y, Direction.X);

        public Arrow(Vector2f position, Vector2f direction)
        {
            Position = position;
            Direction = direction;
        }
        
        public void Draw(RenderTarget target, RenderStates states)
        {
            VertexArray main = new VertexArray();
            main.Append(new Vertex(Position.Multiply(new Vector2f(1, -1)), Color.Green));
            main.Append(new Vertex((Direction + Position).Multiply(new Vector2f(1, -1)), Color.Red));
            main.PrimitiveType = PrimitiveType.Lines;

            target.Draw(main);
        }
    }
}