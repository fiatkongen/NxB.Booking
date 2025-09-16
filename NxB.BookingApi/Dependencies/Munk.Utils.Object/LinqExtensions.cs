using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System.Linq
{
    public static class LinqExtensions
    {
        public static IEnumerable<TResult> OfSpecificType<TResult>(this IEnumerable source, string className)
        {
            return source.OfType<TResult>().Where(x => x.GetType().Name == className);
        }

        public static IEnumerable<IDateInterval> Within(this IEnumerable<IDateInterval> list, DateTime startDate, DateTime endDate)
        {
            return list.Where(rl => ((rl.Start >= startDate && rl.Start < endDate) ||
                                     (rl.End > startDate && rl.End <= endDate) ||
                                     (rl.Start <= startDate && rl.End >= endDate)));
        }
    }
}
