﻿using PersistanceMap.QueryProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistanceMap.Test.Extensions
{
    internal static class IQueryProviderExtensions
    {
        public static ISelectQueryProviderBase<TRebase> Rebase<T, TRebase>(this ISelectQueryProviderBase<T> query)
        {
            return new SelectQueryProvider<TRebase>(query.Context, query.QueryPartsMap as SelectQueryPartsMap);
        }
    }
}
