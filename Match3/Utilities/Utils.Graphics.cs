using Match3.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3.Utilities
{
    public static partial class Utils
    {
        public static void DrawStringFromCenter(this SpriteBatch spriteBatch, BitmapFont font, 
                                                string text, Point position, Color color)
        {
            var rect = font.GetStringRectangle(text, Vector2.Zero);
            position = new Point(position.X - (rect.Size.X / 2),
                                 position.Y - (rect.Size.Y / 2));

            spriteBatch.DrawString(font, text, position.ToVector2(), color);
        }

        public static void Draw(this SpriteBatch spriteBatch, Texture2D texture,
                                Rect rectangle, Color? color = null)
        {
            spriteBatch.Draw(texture, rectangle.ToMonogameRectangle(), color ?? Color.White);
        }

        public static Texture2D GetSolidRectangleTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(App.Graphics, width, height);

            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i)
                data[i] = color;

            texture.SetData(data);

            return texture;
        }

        public static void DrawRect(this SpriteBatch sBatch, Rectangle rect, Color? color = null)
        {
            sBatch.Draw(square, rect, color ?? new Color(Color.Red, 0.75f));
        }
    }
}
