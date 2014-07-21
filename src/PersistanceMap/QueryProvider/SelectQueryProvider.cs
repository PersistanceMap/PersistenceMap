﻿using PersistanceMap.Compiler;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using PersistanceMap.QueryBuilder;
using PersistanceMap.QueryBuilder.Decorators;
using System.Reflection;
using System.Linq;
using PersistanceMap.Internals;

namespace PersistanceMap.QueryProvider
{
    public class SelectQueryProvider<T> : ISelectQueryProvider<T>, IQueryProvider
    {
        public SelectQueryProvider(IDatabaseContext context)
        {
            _context = context;
        }

        public SelectQueryProvider(IDatabaseContext context, SelectQueryPartsMap container)
        {
            _context = context;
            _queryPartsMap = container;
        }

        #region IQueryProvider Implementation

        readonly IDatabaseContext _context;
        public IDatabaseContext Context
        {
            get
            {
                return _context;
            }
        }

        SelectQueryPartsMap _queryPartsMap;
        public SelectQueryPartsMap QueryPartsMap
        {
            get
            {
                if (_queryPartsMap == null)
                    _queryPartsMap = new SelectQueryPartsMap();
                return _queryPartsMap;
            }
        }

        IQueryPartsMap IQueryProvider.QueryPartsMap
        {
            get
            {
                return QueryPartsMap;
            }
        }

        #endregion

        #region Internal Implementation

        internal ISelectQueryProvider<T2> From<T2>()
        {
            System.Diagnostics.Debug.Assert(false, "Change from extension method to factory method or helper!");
            typeof(T2).ToFromQueryPart<T>(QueryPartsMap);
            //QueryPartsMap.Add();

            return new SelectQueryProvider<T2>(Context, QueryPartsMap);
        }

        internal ISelectQueryProvider<T2> From<T2>(params IQueryMap[] parts)
        {
            parts.EnsureArgumentNotNull("part");
            System.Diagnostics.Debug.Assert(false, "Change from extension method to factory method or helper!");
            var fromPart = typeof(T2).ToFromQueryPart<T>(QueryPartsMap, parts);

            return new SelectQueryProvider<T2>(Context, QueryPartsMap);
        }

        #endregion

        #region ISqlExpression<T> Implementation

        public ISelectQueryProvider<T> Join<TJoin>(Expression<Func<TJoin, T, bool>> predicate)
        {
            System.Diagnostics.Debug.Assert(false, "Change from extension method to factory method or helper!");
            var part = typeof(TJoin).ToJoinQueryPart(QueryPartsMap, predicate);

            //QueryPartsMap.Add(part);

            return new SelectQueryProvider<T>(Context, QueryPartsMap);
        }

        public ISelectQueryProvider<T> Join<TJoin>(params Expression<Func<SelectMapOption<TJoin, T>, IQueryMap>>[] args)
        {
            System.Diagnostics.Debug.Assert(false, "Change from extension method to factory method or helper!");
            typeof(TJoin).ToJoinQueryPart<TJoin, T>(QueryPartsMap, MapOptionCompiler.Compile(args).ToArray());
            //QueryPartsMap.Add(typeof(TJoin).ToJoinQueryPart<TJoin, T>(MapOptionCompiler.Compile(args).ToArray()));

            return new SelectQueryProvider<T>(Context, QueryPartsMap);
        }




        public ISelectQueryProvider<T> Where(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public ISelectQueryProvider<T> Where<T2>(Expression<Func<T2, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public ISelectQueryProvider<T> Where<T2, T3>(params Expression<Func<SelectMapOption<T2, T3>, IQueryMap>>[] args)
        {
            throw new NotImplementedException();
        }



        public IEnumerable<T2> Select<T2>()
        {
            var expr = Context.ContextProvider.ExpressionCompiler;
            var query = expr.Compile<T2>(QueryPartsMap);

            return Context.Execute<T2>(query);
        }

        public IEnumerable<T> Select()
        {
            var expr = Context.ContextProvider.ExpressionCompiler;
            var query = expr.Compile<T>(QueryPartsMap);

            return Context.Execute<T>(query);
        }

        public IEnumerable<T2> Select<T2>(params Expression<Func<MapOption<T2>, IQueryMap>>[] mappings)
        {
            throw new NotImplementedException();
        }




        public T2 Single<T2>()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
