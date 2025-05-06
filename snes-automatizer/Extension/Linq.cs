using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snes_automatizer.Extension
{
    internal static class LinqExtension
    {
        /// <summary>
        /// Removes specific elements from the collection and returns a new (List<T>) collection based on the 
        /// provided predicate.
        /// </summary>
        public static IEnumerable<T> RemoveWhere<T>(this IEnumerable<T> collection, SimpleLinqPredicate<T> predicate)
        {
            var result = new List<T>();
            var index = 0;

            foreach (var item in collection)
            {
                if (predicate(item, index++))
                    result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Performs an iteration of the collection; and calls the provided callback.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> collection, SimpleLinqCallback<T> callback)
        {
            var index = 0;

            foreach (var item in collection)
            {
                callback(item, index++);
            }
        }
    }
}
