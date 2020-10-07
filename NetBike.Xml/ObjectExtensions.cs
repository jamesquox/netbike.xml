using System.Collections;
using OptionalSharp;

namespace NetBike.Xml
{
    internal static class ObjectExtensions
    {
        public static bool? IsEmpty(this object @this)
        {
            switch (@this)
            {
                case IDictionary dictionary:
                    return dictionary.Count == 0;
                case IList list:
                    return list.Count == 0;
                case ICollection collection:
                    return collection.Count == 0;
                case IEnumerable enumerable:
                    return !enumerable.GetEnumerator().MoveNext();
                case IEnumerator enumerator:
                    return !enumerator.MoveNext();
            }

            return null;
        }
    }
}