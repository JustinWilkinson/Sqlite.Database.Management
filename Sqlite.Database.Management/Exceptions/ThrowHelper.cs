using System;
using System.Collections.Generic;
using System.Linq;

namespace Sqlite.Database.Management.Exceptions
{
    /// <summary>
    /// Contains helper methods for throwing standard exceptions.
    /// </summary>
    public static class ThrowHelper
    {
        /// <summary>
        /// Throws an ArgumentNullException if the provided argument is null.
        /// </summary>
        /// <param name="arg">Argument to check for null.</param>
        /// <exception cref="ArgumentNullException">Thrown if arg is null</exception>
        public static void ThrowIfArgumentNull(object arg)
        {
            if (arg is null)
            {
                throw new ArgumentNullException(nameof(arg));
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException if the provided string is null, and an ArgumentException if it is all whitespace.
        /// </summary>
        /// <param name="arg">String to check for null/whitespace.</param>
        /// <exception cref="ArgumentNullException">Thrown if arg is null</exception>
        /// <exception cref="ArgumentException">Thrown if arg is all whitespace</exception>
        public static void ThrowIfArgumentNullOrWhitespace(string arg)
        {
            ThrowIfArgumentNull(arg);

            if (arg.All(char.IsWhiteSpace))
            {
                throw new ArgumentException($"{nameof(arg)} cannot be all whitespace!");
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException if the provided string is null, and an ArgumentException if it is all whitespace.
        /// </summary>
        /// <param name="arg">Enumerable to check if null or empty.</param>
        /// <exception cref="ArgumentNullException">Thrown if arg is null</exception>
        /// <exception cref="ArgumentException">Thrown if arg is empty</exception>
        public static void ThrowIfArgumentNullOrEmpty<T>(IEnumerable<T> arg)
        {
            ThrowIfArgumentNull(arg);

            if (!arg.Any())
            {
                throw new ArgumentException($"{nameof(arg)} cannot be empty!");
            }
        }

        /// <summary>
        /// Throws an InvalidOperationException if the provided object is null.
        /// </summary>
        /// <param name="arg">Value to check for null/whitespace.</param>
        /// <param name="name">Optional, name of the invalid field/property/argument.</param>
        /// <exception cref="InvalidOperationException">Thrown if arg is null</exception>
        public static void InvalidIfNull(object arg, string name = null)
        {
            if (arg is null)
            {
                throw new InvalidOperationException($"{name ?? nameof(arg)} cannot be null!");
            }
        }

        /// <summary>
        /// Throws an InvalidOperationException if the provided string is null or whitespace.
        /// </summary>
        /// <param name="arg">String to check for null/whitespace.</param>
        /// <param name="name">Optional, name of the invalid field/property/argument.</param>
        /// <exception cref="InvalidOperationException">Thrown if arg is null or all whitespace</exception>
        public static void InvalidIfNullOrWhitespace(string arg, string name = null)
        {
            InvalidIfNull(arg, name);

            if (arg.All(char.IsWhiteSpace))
            {
                throw new InvalidOperationException($"{name ?? nameof(arg)} cannot be all whitespace!");
            }
        }

        /// <summary>
        /// Throws an InvalidOperationException if the provided enumerable is null or empty.
        /// </summary>
        /// <param name="arg">Enumerable to check if null or empty.</param>
        /// <param name="name">Optional, name of the invalid field/property/argument.</param>
        /// <exception cref="InvalidOperationException">Thrown if arg is null or empty.</exception>
        public static void InvalidIfNullOrEmpty<T>(IEnumerable<T> arg, string name = null)
        {
            InvalidIfNull(arg, name);

            if (!arg.Any())
            {
                throw new InvalidOperationException($"{name ?? nameof(arg)} cannot be empty!");
            }
        }

        /// <summary>
        /// Throws an InvalidOperationException if the provided enumerable does not have any elements matching the provided predicate.
        /// </summary>
        /// <param name="arg">Enumerable to check for elements matching predicate.</param>
        /// <param name="predicate">Predicate to check for</param>
        /// <param name="name">Optional, name of the invalid field/property/argument.</param>
        /// <exception cref="InvalidOperationException">Thrown if arg does not have any elements matching predicate.</exception>
        public static void InvalidIfNotAny<T>(IEnumerable<T> arg, Func<T, bool> predicate, string name = null)
        {
            InvalidIfNull(arg, name);

            if (!arg.Any(predicate))
            {
                throw new InvalidOperationException($"{name ?? nameof(arg)} cannot be empty!");
            }
        }
    }
}