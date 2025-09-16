using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Munk.Utils.Object;

namespace System
{
    public static class ObjectExtensions
    {
        private static readonly DanishBasicFormatter _danishBasicFormatter = new DanishBasicFormatter();
        public static T Lowest<T>(this IComparable<T> value, IComparable<T> value2) where T: IComparable<T>
        {
            return(T) (value.CompareTo((T) value2) > 0 ? value2 : value);
        }

        public static T Highest<T>(this IComparable<T> value, IComparable<T> value2) where T : IComparable<T>
        {
            return (T)(value.CompareTo((T)value2) < 0 ? value2 : value);
        }

        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        public static string ToDanishDecimalString(this decimal value, int digits = 2)
        {
            return _danishBasicFormatter.FormatDecimal(value, digits);
        }

        public static void ModifyProperties(this object obj, IDictionary<string, object> properties)
        {
            Type type = obj.GetType();

            foreach (var (key, value) in properties)
            {
                PropertyInfo prop = type.GetProperty(key);
                var propType = prop.PropertyType;
                prop.SetValue(obj, TypeDescriptor.GetConverter(propType).ConvertFrom(value?.ToString()), null);
            }
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
