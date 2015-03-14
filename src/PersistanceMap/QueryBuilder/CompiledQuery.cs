﻿
namespace PersistanceMap.QueryBuilder
{
    public class CompiledQuery
    {
        public string QueryString { get; internal set; }

        public IQueryPartsContainer QueryParts { get; internal set; }

        ///// <summary>
        ///// Converters that convert a db value to the desired object value
        ///// </summary>
        //public IEnumerable<MapValueConverter> Converters { get; set; }
    }
}
