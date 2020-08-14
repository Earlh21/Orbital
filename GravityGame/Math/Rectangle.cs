using SFML.System;

namespace GravityGame
{
    public struct Rectangle
    {
        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }

        public float Width => Size.X;
        public float Height => Size.Y;
        public float X => Position.X;
        public float Y => Position.Y;

        public Rectangle(Vector2f position, Vector2f size)
        {
            Position = position;
            Size = size;
        }

        public Rectangle(float x, float y, float width, float height)
        {
            Position = new Vector2f(x, y);
            Size = new Vector2f(width, height);
        }

        public bool ContainsPoint(Vector2f position)
        {
            if (position.X < X || position.Y < Y)
            {
                return false;
            }

            if (position.X > X + Width || position.Y > Y + Height)
            {
                return false;
            }

            return true;
        }

        public bool FullyContains(Body body)
        {
            if (body.Position.X - body.Radius < X || body.Position.X + body.Radius > X + Width)
            {
                return false;
            }

            if (body.Position.Y - body.Radius < Y || body.Position.Y + body.Radius > Y + Height)
            {
                return false;
            }

            return true;
        }

        public bool FullyContains(Vector2f position, float radius)
        {
            if (position.X - radius < X || position.X + radius > X + Width)
            {
                return false;
            }

            if (position.Y - radius < Y || position.Y + radius > Y + Height)
            {
                return false;
            }

            return true;
        }
        
        public bool PartiallyContains(Body body)
        {
            if (ContainsPoint(body.Position))
            {
                return true;
            }
            
            if (body.Position.X + body.Radius < X || body.Position.X - body.Radius > X + Width)
            {
                return false;
            }

            if (body.Position.Y + body.Radius < Y || body.Position.Y - body.Radius > Y + Height)
            {
                return false;
            }

            return true;
        }
    }
}