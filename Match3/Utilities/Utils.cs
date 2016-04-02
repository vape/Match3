using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Match3.Core;
using Match3.World;


namespace Match3.Utilities
{
    public static partial class Utils
    {
        private static Random rand;
        private static Texture2D square;

        static Utils()
        {
            square = GetSolidRectangleTexture(1, 1, Color.Gray);
        }

        public static bool Usable(this Block block)
        {
            return block != null && !block.IsAnimating;
        }

        public static bool Any<T>(this T[,] array, Predicate<T> predicate)
        {
            foreach (var elem in array)
                if (predicate(elem))
                    return true;

            return false;
        }

        public static bool Any<T>(this T[] array, Predicate<T> predicate)
        {
            foreach (var elem in array)
                if (predicate(elem))
                    return true;

            return false;
        }

        public static int GetRand(int min, int max)
        {
            if (rand == null)
                rand = new Random();

            return rand.Next(min, max);
        }

        public static float SmoothDamp(float current, float target, float t)
        {
            return ((current * (t - 1)) + target) / t;
        }
    }
}
