using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using AutoMapper;

namespace AutoMapper
{
    public static class AutoMapperExtensions
    {
        public static IMappingExpression<TSource, TDestination> Ignore<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> map,
            Expression<Func<TDestination, object>> selector)
        {
            map.ForMember(selector, config => config.Ignore());
            return map;
        }
    }
}
