using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphical.Geometry;
using EdgeKey = Graphical.Graphs.EdgeKey;

namespace Graphical.Extensions
{
    /// <summary>
    /// Static class extending List funtionality
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// List extension method.
        /// Given an ascending sorted list add an item mantaining list's order. This method modifies the list instance.
        /// </summary>
        /// <typeparam name="T">IComparable</typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        public static void AddItemSorted<T>(this List<T> list, T item) where T : IComparable<T>
        {
            int lo = 0;
            int hi = list.Count();
            while (lo < hi)
            {
                int mid = (int)(lo + hi) / 2;
                if (item.CompareTo(list[mid]) < 0)
                {
                    hi = mid;
                }
                else
                {
                    lo = mid + 1;
                }
            }
            list.Insert(lo, item);
        }

        /// <summary>
        /// List extension method.
        /// Given an ascending sorted list, adds items mantaining list's order. This method modifies the list instance.
        /// </summary>
        /// <typeparam name="T">IComparable</typeparam>
        /// <param name="list"></param>
        /// <param name="items"></param>
        public static void AddItemsSorted<T>(this List<T> list, T[] items) where T : IComparable<T>
        {
            foreach(var item in items)
            {
                list.AddItemSorted<T>(item);
            }
        }

        /// <summary>
        /// List extension method.
        /// Returns the index of an item on a ascending sorted list.
        /// </summary>
        /// <typeparam name="T">IComparable</typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns name="index">Item's index</returns>
        public static int BisectIndex<T> (this List<T> list, T item) where T : IComparable<T>
        {
            int lo = 0, hi = list.Count;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (item.CompareTo(list[mid]) < 0)
                {
                    hi = mid;
                }
                else
                {
                    lo = mid + 1;
                }
            }
            return lo;
        }

        /// <summary>
        /// List extension method.
        /// Splits a list into sublist with the given length of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static List<List<T>> Chop<T>(this List<T> list, int length)
        {
            return list
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / length)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

    }
}
