using BlueLightSoftware.Common;
using System;
using System.Collections.Generic;

namespace LSFV.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts comma seperated values to an <see cref="Enum"/> array of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T[] CSVToEnumArray<T>(this string str, bool logErrors = false) where T : struct
        {
            string[] vals = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var items = new List<T>(vals.Length);
            foreach (string v in vals)
            {
                if (Enum.TryParse(v.Trim(), out T flag))
                {
                    items.Add(flag);
                }
                else if (logErrors)
                {
                    Log.Debug($"Unable to parse enum value of '{v}' for type '{typeof(T).Name}'");
                }
            }

            return items.ToArray();
        }

        /// <summary>
        /// Converts comma seperated values to an <see cref="Enum"/> <see cref="List{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<T> CSVToEnumList<T>(this string str, bool logErrors = false) where T : struct
        {
            string[] vals = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var items = new List<T>(vals.Length);
            foreach (string v in vals)
            {
                if (Enum.TryParse(v.Trim(), out T flag))
                {
                    items.Add(flag);
                }
                else if (logErrors)
                {
                    Log.Debug($"Unable to parse enum value of '{v}' for type '{typeof(T).Name}'");
                }
            }

            return items;
        }

        /// <summary>
        /// Converts comma seperated integer values into an array of integers
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IEnumerable<int> ToIntList(this string str)
        {
            if (String.IsNullOrEmpty(str))
                yield break;

            foreach (var s in str.Split(','))
            {
                if (int.TryParse(s.Trim(), out int num))
                    yield return num;
            }
        }

        /// <summary>
        /// Returns a random element from an array of strings
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static string GetRandomString(this string[] items)
        {
            if (items.Length == 0) return String.Empty;

            int count = items.Length - 1;
            int index = new CryptoRandom().Next(0, count);
            return items[index];
        }
    }
}
