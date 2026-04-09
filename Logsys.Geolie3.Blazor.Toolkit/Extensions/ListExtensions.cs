using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.DEMO.Toolkit.Extensions
{
    public static class StringListExtensions
    {
        public static void TrimAll(this List<string> stringList)
        {
            for (int i = 0; i < stringList.Count; i++)
            {
                stringList[i] = stringList[i].Trim(); //warning: do not change this to lambda expression (.ForEach() uses a copy)
            }
        }

        public static void ToUpperAll(this List<string> stringList)
        {
            for (int i = 0; i < stringList.Count; i++)
            {
                stringList[i] = stringList[i].ToUpper(); //warning: do not change this to lambda expression (.ForEach() uses a copy)
            }
        }
    }

    public static class LinqExtensions
    {
        // Méthode d'extension pour DistinctBy basée sur une clé spécifique
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
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
