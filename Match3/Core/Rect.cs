using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3.Core
{
    public struct Rect
    {
        public readonly Vector2 Position;
        public readonly Point Size;

        public Rect(Vector2 position, Point size)
        {
            Position = position;
            Size = size;
        }

        public bool Contains(Point point)
        {
            return point.X > Position.X &&
                   point.Y > Position.Y &&
                   point.X < Position.X + Size.X &&
                   point.Y < Position.Y + Size.Y;
        }

        public Rectangle ToMonogameRectangle()
        {
            return new Rectangle(Position.ToPoint(), Size);
        }

        public Rect ScaleFromCenter(float scale)
        {
            if (scale == 1)
                return this;

            var newSize = new Point((int)(Size.X * scale), (int)(Size.Y * scale));
            var newPosition = Position - new Vector2((newSize.X - Size.X) / 2,
                                                     (newSize.Y - Size.Y) / 2);

            return new Rect(newPosition, newSize);
        }
    }
}
