using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sqlite.Database.Management.Extensions
{
    /// <summary>
    /// Contains useful extensions on <see cref="IEnumerable{T}"/> and <see cref="IAsyncEnumerable{T}"/>.
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

        /// <summary>
        /// Creates a new list with the provided item as its only element.
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="item">Item to put in list.</param>
        public static List<T> AsList<T>(this T item) => new(1) { item };

#if !NETSTANDARD2_0
        /// <summary>
        /// Filters an <see cref="IAsyncEnumerable{T}"/> with the provided predicate.
        /// </summary>
        /// <typeparam name="T">Type of elements in the collection.</typeparam>
        /// <param name="asyncEnumerable">Enumerable to iterate over.</param>
        /// <param name="predicate">Predicate used to filter the collection.</param>
        /// <returns>A filtered <see cref="IAsyncEnumerable{T}"/>.</returns>
        public static async IAsyncEnumerable<T> WhereAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, Func<T, bool> predicate)
        {
            await foreach (var element in asyncEnumerable.ConfigureAwait(false))
            {
                if (predicate(element))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Projects an <see cref="IAsyncEnumerable{TIn}"/> into a new sequence using the provided selector..
        /// </summary>
        /// <typeparam name="TIn">Type of elements in the input collection.</typeparam>
        /// <typeparam name="TOut">Type of elements in the output collection.</typeparam>
        /// <param name="asyncEnumerable">Enumerable to iterate over.</param>
        /// <param name="selector">Function used to project the elements in the collection to a new type.</param>
        /// <returns>A filtered <see cref="IAsyncEnumerable{TOut}"/>.</returns>
        public static async IAsyncEnumerable<TOut> SelectAsync<TIn, TOut>(this IAsyncEnumerable<TIn> asyncEnumerable, Func<TIn, TOut> selector)
        {
            await foreach (var element in asyncEnumerable.ConfigureAwait(false))
            {
                yield return selector(element);
            }
        }

        /// <summary>
        /// Returns the single item present in an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of elements in the collection.</typeparam>
        /// <param name="asyncEnumerable">Enumerable to iterate over.</param>
        /// <returns>The single item in the <see cref="IAsyncEnumerable{T}"/>.</returns>
        public static async Task<T> SingleAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var enumerator = asyncEnumerable.GetAsyncEnumerator();

            if (!await enumerator.MoveNextAsync())
            {
                throw new InvalidOperationException("Collection contains no elements!");
            }

            var result = enumerator.Current;

            if (await enumerator.MoveNextAsync())
            {
                throw new InvalidOperationException("Collection contains more than one element!");
            }

            return result;
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to a <see cref="List{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of elements in the collection.</typeparam>
        /// <param name="asyncEnumerable">Enumerable to iterate over.</param>
        /// <returns>A task representing the list.</returns>
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var list = new List<T>();
            await foreach (var element in asyncEnumerable.ConfigureAwait(false))
            {
                list.Add(element);
            }
            return list;
        }

        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to a <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of elements in the collection.</typeparam>
        /// <param name="asyncEnumerable">Enumerable to iterate over.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> formed from the items in the <see cref="IEnumerable{T}"/>.</returns>
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
        {
            await Task.CompletedTask;

            foreach (var element in enumerable)
            {
                yield return element;
            }
        }
#endif
    }
}