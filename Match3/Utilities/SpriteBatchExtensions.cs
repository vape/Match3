using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

using Match3.Core;


namespace Match3.Utilities
{
    public static class SpriteBatchExtensions
    {
        private static Texture2D square = Utils.GetSolidRectangleTexture(1, 1, Color.Gray);

        public static void DrawStringWithShadow(this SpriteBatch spriteBatch, BitmapFont font,
                                                string text, Vector2 position, Color color,
                                                Color? shadowColor = null,
                                                Vector2? shadowOffset = null)
        {
            var shadowPosition = position + (shadowOffset ?? new Vector2(1, 2));

            spriteBatch.DrawString(font, text, shadowPosition, shadowColor ?? new Color(Color.Black, 0.5f));
            spriteBatch.DrawString(font, text, position, color);
        }

        public static void DrawStringFromCenter(this SpriteBatch spriteBatch, BitmapFont font,
                                                string text, Vector2 position, Color color)
        {
            var rect = font.GetStringRectangle(text, Vector2.Zero);
            position = new Vector2(position.X - (rect.Size.X / 2),
                                   position.Y - (rect.Size.Y / 2));

            spriteBatch.DrawString(font, text, position, color);
        }

        public static void DrawRect(this SpriteBatch sBatch, Rectangle rect, Color? color = null)
        {
            sBatch.Draw(square, rect, color ?? new Color(Color.Red, 0.75f));
        }

        public static void Draw(this SpriteBatch spriteBatch, Texture2D texture,
                                Rect rectangle, Color? color = null)
        {
            spriteBatch.Draw(texture, rectangle.ToMonogameRectangle(), color ?? Color.White);
        }
    }
}
