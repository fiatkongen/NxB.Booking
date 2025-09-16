using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Munk.Utils.Object
{
    //Modified version of: https://stackoverflow.com/questions/8159772/extension-method-convertion-to-linq-expressions-and-common-methods
    //Mimicking this:
    //    return ((this.StartDate >= startDate && this.StartDate < endDate) ||
    //            (this.EndDate > startDate && this.EndDate <= endDate) ||
    //            (this.StartDate <= startDate && this.EndDate >= endDate));
    public static class QueryExtensions
    {
        public static IQueryable<TSource> OverlapsWith<TSource, TKey>(
            this IQueryable<TSource> source,
            DateInterval dateInterval,
            Expression<Func<TSource, TKey>> start,
            Expression<Func<TSource, TKey>> end)
            where TKey : IComparable<TKey>
        {
            Expression startDate = start.Body;
            Expression endDate = end.Body;

            Expression lowerBound = Expression.And(
                Expression.GreaterThanOrEqual(startDate, Expression.Constant(dateInterval.Start)),
                Expression.LessThan(startDate, Expression.Constant(dateInterval.End))
            );

            Expression middleBound = Expression.And(
                Expression.GreaterThan(endDate, Expression.Constant(dateInterval.Start)),
                Expression.LessThanOrEqual(endDate, Expression.Constant(dateInterval.End))
            );

            Expression upperBound1 = Expression.LessThanOrEqual(startDate, Expression.Constant(dateInterval.Start));
            Expression upperBound2 = Expression.GreaterThanOrEqual(endDate, Expression.Constant(dateInterval.End));

            var lambdaLower = Expression.Lambda<Func<TSource, bool>>(lowerBound, start.Parameters);
            var lambdaMiddle = Expression.Lambda<Func<TSource, bool>>(middleBound, end.Parameters);

            var lambdaUpper1 = Expression.Lambda<Func<TSource, bool>>(upperBound1, start.Parameters);
            var lambdaUpper2 = Expression.Lambda<Func<TSource, bool>>(upperBound2, end.Parameters);
            var lambdaUpper = PredicateBuilder.Create(lambdaUpper1).And(lambdaUpper2);

            var expression = PredicateBuilder.Create(lambdaLower).Or(lambdaMiddle).Or(lambdaUpper);
            return source.Where(expression);
        }
    }
}