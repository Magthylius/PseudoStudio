using System.Collections.Generic;

namespace Tenshi.Shrine
{
    public static class ListShrine
    {
        /// <summary>
        /// Standard virtual RNG shuffling, does not generate a new list so null errors must be handled beforehand.
        /// </summary>
        /// <typeparam name="TList">Any variable type</typeparam>
        /// <param name="seed">Seed for virtual RNG initialisation</param>
        public static List<TList> Shuffle<TList>(this List<TList> list, int seed)
        {
            System.Random virtualRNG = new System.Random(seed);
            int i = -1;
            while (++i < list.Count)
            {
                int randIndex = virtualRNG.Next(i, list.Count - 1);
                TList item = list[randIndex];
                list[randIndex] = list[i];
                list[i] = item;
            }
            return list;
        }
    }
}