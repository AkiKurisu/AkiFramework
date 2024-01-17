using System;
using System.Collections.Generic;
using System.Linq;
namespace Kurisu.Framework
{
    /// <summary>
    /// Shuffling extension used for card game
    /// </summary>
    public static class ShufflingExtension
    {
        private static readonly Random rng = new();
        /// <summary>
        /// Fisher–Yates Shuffle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }

        public static T Random<T>(this IReadOnlyList<T> list)
        {
            return list[rng.Next(list.Count)];
        }

        public static T Last<T>(this IReadOnlyList<T> list)
        {
            return list[list.Count - 1];
        }

        public static List<T> GetRandomElements<T>(this List<T> list, int elementsCount)
        {
            return list.OrderBy(arg => Guid.NewGuid()).Take(list.Count < elementsCount ? list.Count : elementsCount)
                .ToList();
        }
    }
}