using SFML.System;
using System;
using GravityGame.Extension;
using GravityGame.Graphics;
using SFML.Graphics;

namespace GravityGame
{
    public class VectorField : Drawable
    {
        private Vector2f[,] data;
        private Vector2f step_size;
        private Rectangle domain;
        
        public Rectangle Domain => domain;

        public VectorField(Rectangle domain, int width, int height)
        {
            this.domain = domain;
            data = new Vector2f[width, height];
            step_size = new Vector2f(domain.Width / width, domain.Height / height);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            for(int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    CircleShape shape = new CircleShape();
                    shape.Radius = 5;
                    shape.FillColor = new Color(255, 0, 0, (byte)(100 * GetValue(new Vector2i(i, j)).Length()));
                    shape.Position = (GetPosition(i, j) + new Vector2f(-step_size.X, step_size.Y) / 2);//.Multiply(new Vector2f(1, 1));
                    Arrow arrow = new Arrow(GetPosition(i, j), GetValue(new Vector2i(i, j)));
                    target.Draw(shape);
                }
            }
        }

        public void AffectAdjacent(Point point)
        {
            Vector2i index = GetIndex(point.Position);

            TryAffectCell(point, index.X - 1, index.Y + 1);
            TryAffectCell(point, index.X - 1, index.Y);
            TryAffectCell(point, index.X - 1, index.Y - 1);
            TryAffectCell(point, index.X, index.Y - 1);
            TryAffectCell(point, index.X, index.Y + 1);
            TryAffectCell(point, index.X + 1, index.Y - 1);
            TryAffectCell(point, index.X + 1, index.Y);
            TryAffectCell(point, index.X + 1, index.Y + 1);
        }

        private void TryAffectCell(Point point, int i, int j)
        {
            if (i < 0)
            {
                return;
            }

            if (i > data.GetLength(0) - 1)
            {
                return;
            }

            if (j < 0)
            {
                return;
            }

            if (j > data.GetLength(1) - 1)
            {
                return;
            }

            AffectCell(point, i, j);
        }

        private void AffectCell(Point point, int i, int j)
        {
            Vector2f cell_position = GetPosition(i, j);
            Vector2f displacement = point.Position - cell_position;
            data[i, j] += displacement * point.Mass / displacement.LengthSquared();
        }

        public Vector2f GetPosition(int i, int j)
        {
            return domain.Position + new Vector2f(i, j).Multiply(step_size) + step_size / 2;
        }
        
        public Vector2i GetIndex(Vector2f position)
        {   
            return (position - domain.Position).Divide(step_size).Floor();
        }

        public Vector2f GetValue(Vector2i index)
        {
            return data[index.X, index.Y];
        }

        public Vector2f GetValue(Vector2f position)
        {
            return GetValue(GetIndex(position));
        }
    }
}