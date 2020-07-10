using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sqlite.Database.Management.Extensions
{
    /// <summary>
    /// Contains helpful extension methods to enhance performance of Reflection.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Gets all Public/Instance properties.
        /// </summary>
        /// <returns>An IEnumerable of public instance properties</returns>
        public static IEnumerable<PropertyInfo> GetPublicInstanceProperties(this Type type) => type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// Gets all Public/Instance Fields.
        /// </summary>
        /// <returns>An IEnumerable of public instance fields</returns>
        public static IEnumerable<PropertyInfo> GetPublicInstanceFields(this Type type) => type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// Gets the setter for the specified property on the class.
        /// </summary>
        /// <typeparam name="T">Type of instance.</typeparam>
        /// <param name="property">Property to obtain setter of.</param>
        /// <returns>A delegate that invokes the property setter.</returns>
        public static Action<T, object> GetSetter<T>(this PropertyInfo property) => GetSetDelegate<T>(property.SetMethod);

        #region Helpers
        private static readonly MethodInfo _genericSetHelper = typeof(ReflectionExtensions).GetMethod(nameof(GetSetDelegateHelper), BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Helper method that returns a setter for a property using the object class.
        /// </summary>
        /// <typeparam name="T">Type of instance</typeparam>
        /// <param name="method">Method Info to pass</param>
        /// <returns></returns>
        private static Action<T, object> GetSetDelegate<T>(MethodInfo method)
        {
            // Supply the type arguments to the generic helper.
            MethodInfo oConstructedHelper = _genericSetHelper.MakeGenericMethod(typeof(T), method.GetParameters()[0].ParameterType);

            // Cast the result to the right kind of delegate and return it. The null argument is because it's a static method
            return (Action<T, object>)oConstructedHelper.Invoke(null, new[] { method });
        }

        /// <summary>
        /// Generic helper method that creates a more weakly typed delegate that will call a strongly typed one.
        /// </summary>
        /// <typeparam name="TTarget">Target type</typeparam>
        /// <typeparam name="TParam">Parameter type</typeparam>
        /// <param name="method">Set Method</param>
        /// <returns>A weakly typed delegate that calls a strongly type one.</returns>
        private static Action<TTarget, object> GetSetDelegateHelper<TTarget, TParam>(MethodInfo method) where TTarget : class
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Action<TTarget, TParam> action = CreateTypedAction<TTarget, TParam>(method);

            // Now create a more weakly typed delegate which will call the strongly typed one
            return (TTarget target, object param) => action(target, (TParam)Convert.ChangeType(param, typeof(TParam)));
        }

        /// <summary>
        /// Creates a type action from a method info.
        /// </summary>
        /// <typeparam name="S">Type of instance</typeparam>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="method">Set method</param>
        /// <returns>Typed setter</returns>
        private static Action<S, T> CreateTypedAction<S, T>(MethodInfo method) => (Action<S, T>)Delegate.CreateDelegate(typeof(Action<S, T>), method);
        #endregion
    }
}