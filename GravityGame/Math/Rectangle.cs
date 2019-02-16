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

        public bool Contains(Vector2f position)
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
    }
}