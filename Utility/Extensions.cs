using System.Collections.Generic;
using UnityEngine;

namespace KnightCrawlers.Extensions
{
    public static class Extensions
    {
        public static T GetRandom<T>(this IList<T> list, int min = 0) => list[Random.Range(min, list.Count)];
        public static Vector3 Flat(this Vector3 v) => new Vector3(v.x, 0.05f, v.z);
        public static bool CompareTypes<T>(T a, T b) => a.Equals(b) ? true : false;
        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = Random.Range(i, list.Count);
                T shuffledValue = list[j];
                list[j] = list[i];
                list[i] = shuffledValue;
            }
        }
        //
        public static string AllCaps(this string input)
        {
            char[] chars = input.ToCharArray();
            //
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = char.ToUpperInvariant(chars[i]);
            }
            //
            return new string(chars);
        }
    }
}
