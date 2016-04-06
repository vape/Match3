using System;
using System.Linq;

namespace Match3.Utilities
{
    public static class ArrayExtensions
    {
        public static bool Any<T>(this T[,] array, Predicate<T> predicate)
        {
            return array.Cast<T>().Any(elem => predicate(elem));
        }

        public static bool Any<T>(this T[] array, Predicate<T> predicate)
        {
            return Enumerable.Any(array, elem => predicate(elem));
        }
    }
}
