using System;
using System.Collections.Generic;

namespace Sqlite.Database.Management.Extensions
{
    /// <summary>
    /// Contains useful extensions on IEnumerables.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Performs an action for each element in the enumerable.
        /// </summary>
        /// <typeparam name="T">Type of elements in the enumerable.</typeparam>
        /// <param name="enumerable">Enumerable to iterate over.</param>
        /// <param name="action">Action to perform for each element.</param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var element in enumerable)
            {
                action(element);
            }
        }
    }
}