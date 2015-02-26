using System.Collections.Generic;
using System.Linq;

namespace Assets.Classes.Foundation.Extensions
{
    public static class ListExtensions
    {
        public static T Random<T>(this List<T> l)
        {

            if (l == null || !l.Any()) return default(T);

            return l[UnityEngine.Random.Range(0, l.Count)];
        }
    }
}
