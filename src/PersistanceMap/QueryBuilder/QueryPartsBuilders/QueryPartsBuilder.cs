﻿using PersistanceMap.QueryParts;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace PersistanceMap.QueryBuilder.QueryPartsBuilders
{
    internal class QueryPartsBuilder
    {
        protected QueryPartsBuilder()
        {
        }

        private static QueryPartsBuilder instance;

        /// <summary>
        /// Gets the Singleton instance of the QueryPartsBuilder
        /// </summary>
        public static QueryPartsBuilder Instance
        {
            get
            {
                if (instance == null)
                    instance = new QueryPartsBuilder();

                return instance;
            }
        }

        internal IFieldQueryPart AddFieldQueryMap<TProp>(IQueryPartsMap queryParts, string field, string alias, string entity, string entityalias, Expression<Func<TProp, object>> valueConverter)
        {
            var part = new FieldQueryPart(field, alias, entityalias, entity, alias ?? field, ConvertExpression(valueConverter))
            {
                OperationType = OperationType.Include
            };

            queryParts.Add(part);
            
            return part;
        }

        internal void AddFiedlParts(SelectQueryPartsMap queryParts, FieldQueryPart[] fields)
        {
            foreach (var map in queryParts.Parts.OfType<IQueryPartDecorator>().Where(p => p.OperationType == OperationType.Select))
            {
                // add all mapped fields to a collection to ensure that they are used in the query
                var unusedMappedFields = map.Parts.OfType<FieldQueryPart>().ToList();

                foreach (var field in fields)
                {
                    // check if the field was allready mapped previously
                    var mappedFields = map.Parts.OfType<FieldQueryPart>().Where(f => f.Field == field.Field || f.FieldAlias == field.Field);
                    if (mappedFields.Any())
                    {
                        foreach (var mappedField in mappedFields)
                        {
                            mappedField.Sufix = ", ";
                            unusedMappedFields.Remove(mappedField);
                        }

                        continue;
                    }

                    if (map.IsSealded)
                        continue;

                    // add the new field
                    field.Sufix = ", ";
                    map.Add(field);
                }

                // remove all mapped fields that were not included in the select fields
                foreach (var field in unusedMappedFields)
                {
                    map.Remove(field);
                }

                var last = map.Parts.LastOrDefault(p => p is FieldQueryPart) as FieldQueryPart;
                if (last != null)
                    last.Sufix = " ";
            }
        }

        /// <summary>
        /// Converts a Func{T,object} expression to a Func{object,object} expression
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal Expression<Func<object, object>> ConvertExpression<TProp>(Expression<Func<TProp, object>> expression)
        {
            if (expression == null)
                return null;

            var p = Expression.Parameter(typeof(object));

            return Expression.Lambda<Func<object, object>>(Expression.Invoke(expression, Expression.Convert(p, typeof(TProp))), p);
        }
    }
}
