using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Match3.Utilities
{
    public static class Utils
    {
        private static Random rand;

        public static int GetRand(int min, int max)
        {
            if (rand == null)
                rand = new Random();

            return rand.Next(min, max);
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
    }
}
