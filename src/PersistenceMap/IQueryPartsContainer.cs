﻿using System;
using System.Collections.Generic;
using PersistenceMap.QueryParts;

namespace PersistenceMap
{
    /// <summary>
    /// A container for all queryparts needed for a sql statement
    /// </summary>
    public interface IQueryPartsContainer : IEnumerable<IQueryPart>
    {
        /// <summary>
        /// Add a querypart
        /// </summary>
        /// <param name="part"></param>
        void Add(IQueryPart part);

        /// <summary>
        /// Add a querypart before the last operation
        /// </summary>
        /// <param name="part"></param>
        /// <param name="operation"></param>
        void AddBefore(IQueryPart part, OperationType operation);

        /// <summary>
        /// Add a querypart after the last operation
        /// </summary>
        /// <param name="part"></param>
        /// <param name="operation"></param>
        void AddAfter(IQueryPart part, OperationType operation);

        /// <summary>
        /// Add a querypart to the query part with the operation
        /// </summary>
        /// <param name="part"></param>
        /// <param name="operation"></param>
        void AddToLast(IQueryPart part, OperationType operation);

        /// <summary>
        /// Add a querypart
        /// </summary>
        /// <param name="part"></param>
        /// <param name="predicate"></param>
        void AddToLast(IQueryPart part, Func<IQueryPart, bool> predicate);

        /// <summary>
        /// Remove the part from the Query tree
        /// </summary>
        /// <param name="part">The part to be removed</param>
        void Remove(IQueryPart part);

        /// <summary>
        /// The list of queryparts in the container
        /// </summary>
        IEnumerable<IQueryPart> Parts { get; }

        /// <summary>
        /// Gets the aggregate part for the query
        /// </summary>
        IQueryPart AggregatePart { get; }

        /// <summary>
        /// Gets or sets the aggregate type for this query
        /// </summary>
        Type AggregateType { get; set; }
    }
}
