﻿using PersistanceMap.Factories;
using PersistanceMap.QueryBuilder;
using PersistanceMap.QueryBuilder.Commands;
using PersistanceMap.QueryParts;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace PersistanceMap.Sqlite.QueryBuilder
{
    internal class DatabaseQueryBuilder : QueryBuilderBase<SqliteDatabaseContext>, IDatabaseQueryExpression, IQueryExpression
    {
        public DatabaseQueryBuilder(SqliteDatabaseContext context)
            : base(context)
        {
        }

        public DatabaseQueryBuilder(SqliteDatabaseContext context, IQueryPartsMap container)
            : base(context, container)
        {
        }
        
        #region IDatabaseQueryExpression Implementation

        public PersistanceMap.Sqlite.ITableQueryExpression<T> Table<T>()
        {
            return new TableQueryBuilder<T>(Context, QueryPartsMap);
        }

        #endregion
    }

    internal class TableQueryBuilder<T> : TableQueryBuilder<T, SqliteDatabaseContext>, PersistanceMap.Sqlite.ITableQueryExpression<T>
    {
        public TableQueryBuilder(SqliteDatabaseContext context, IQueryPartsMap container)
            : base(context, container)
        {
        }


        private IQueryPart CreateColumn(string name, Type type, bool isNullable)
        {
            Func<string> expression = () => string.Format("{0} {1}{2}{3}",
                    name,
                    type.ToSqlDbType(),
                    isNullable ? "" : " NOT NULL",
                    QueryPartsMap.Parts.Last(p => p.OperationType == OperationType.Column || p.OperationType == OperationType.TableKeys).ID == name ? "" : ", ");

            return new DelegateQueryPart(OperationType.Column, expression, name);
        }

        #region ITableQueryExpression Implementation

        /// <summary>
        /// Create a create table expression
        /// </summary>
        public override void Create()
        {
            var createPart = new DelegateQueryPart(OperationType.CreateTable, () => string.Format("CREATE TABLE IF NOT EXISTS {0} (", typeof(T).Name));
            QueryPartsMap.AddBefore(createPart, OperationType.None);

            var fields = TypeDefinitionFactory.GetFieldDefinitions<T>();
            foreach (var field in fields.Reverse())
            {
                var existing = QueryPartsMap.Parts.Where(p => (p.OperationType == OperationType.Column || p.OperationType == OperationType.IgnoreColumn) && p.ID == field.MemberName);
                if (existing.Any())
                    continue;

                var fieldPart = CreateColumn(field.MemberName, field.MemberType, field.IsNullable);

                if (QueryPartsMap.Parts.Any(p => p.OperationType == OperationType.Column))
                {
                    QueryPartsMap.AddBefore(fieldPart, OperationType.Column);
                }
                else
                    QueryPartsMap.AddAfter(fieldPart, OperationType.CreateTable);
            }

            // add closing bracked
            QueryPartsMap.Add(new DelegateQueryPart(OperationType.None, () => ")"));

            Context.AddQuery(new MapQueryCommand(QueryPartsMap));
        }

        /// <summary>
        /// Creates a expression to rename a table
        /// </summary>
        /// <typeparam name="TNew">The type of the new table</typeparam>
        public void RenameTo<TNew>()
        {
            var part = new DelegateQueryPart(OperationType.RenameTable, () => string.Format("ALTER TABLE {0} RENAME TO {1}", typeof(T).Name, typeof(TNew).Name));
            QueryPartsMap.Add(part);

            Context.AddQuery(new MapQueryCommand(QueryPartsMap));
        }

        /// <summary>
        /// Ignore the field when creating the table
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public override PersistanceMap.ITableQueryExpression<T> Ignore(Expression<Func<T, object>> field)
        {
            base.Ignore(field);
            return new TableQueryBuilder<T>(Context, QueryPartsMap);
        }

        /// <summary>
        /// Marks a column to be a primary key column
        /// </summary>
        /// <param name="key">The field that marks the key</param>
        /// <param name="isAutoIncrement">Is the column a auto incrementing column</param>
        /// <returns></returns>
        public override PersistanceMap.ITableQueryExpression<T> Key(Expression<Func<T, object>> key, bool isAutoIncrement = false)
        {
            base.Key(key, isAutoIncrement);

            return new TableQueryBuilder<T>(Context, QueryPartsMap);
        }

        /// <summary>
        /// Marks a set of columns to be a combined primary key of a table
        /// </summary>
        /// <param name="keyFields">Properties marking the primary keys of the table</param>
        /// <returns></returns>
        public override PersistanceMap.ITableQueryExpression<T> Key(params Expression<Func<T, object>>[] keyFields)
        {
            base.Key(keyFields);

            return new TableQueryBuilder<T>(Context, QueryPartsMap);
        }

        /// <summary>
        /// Marks a field to be a foreignkey column
        /// </summary>
        /// <typeparam name="TRef">The referenced table for the foreign key</typeparam>
        /// <param name="field">The foreign key field</param>
        /// <param name="reference">The key field in the referenced table</param>
        /// <returns></returns>
        public override PersistanceMap.ITableQueryExpression<T> ForeignKey<TRef>(Expression<Func<T, object>> field, Expression<Func<TRef, object>> reference)
        {
            base.ForeignKey<TRef>(field, reference);

            return new TableQueryBuilder<T>(Context, QueryPartsMap);
        }

        /// <summary>
        /// Creates a expression that is created for operations for a table field
        /// </summary>
        /// <param name="column">The field to alter</param>
        /// <param name="operation">The type of operation for the field</param>
        /// <param name="precision">Precision of the field</param>
        /// <param name="isNullable">Is the field nullable</param>
        /// <returns></returns>
        public override PersistanceMap.ITableQueryExpression<T> Column(Expression<Func<T, object>> column, FieldOperation operation = FieldOperation.None, string precision = null, bool? isNullable = null)
        {
            var memberName = FieldHelper.TryExtractPropertyName(column);
            var fields = TypeDefinitionFactory.GetFieldDefinitions<T>();
            var field = fields.FirstOrDefault(f => f.MemberName == memberName);

            switch (operation)
            {
                case FieldOperation.None:
                    //TODO: precision???
                    var part = CreateColumn(field.MemberName, field.MemberType, isNullable ?? field.IsNullable);
                    QueryPartsMap.AddAfter(part, QueryPartsMap.Parts.Any(p => p.OperationType == OperationType.Column) ? OperationType.Column : OperationType.CreateTable);
                    break;

                case FieldOperation.Add:
                    //TODO: precision???
                    var nullable = isNullable != null ? (isNullable.Value ? "" : " NOT NULL") : field.IsNullable ? "" : " NOT NULL";
                    var expression = string.Format("ADD COLUMN {0} {1}{2}", field.MemberName, field.MemberType.ToSqlDbType(), nullable);
                    QueryPartsMap.Add(new DelegateQueryPart(OperationType.AlterField, () => expression));
                    break;

                default:
                    throw new NotSupportedException("SQLite only supports ADD column");
            }

            return new TableQueryBuilder<T>(Context, QueryPartsMap);
        }

        /// <summary>
        /// Creates a expression that is created for operations for a table field
        /// </summary>
        /// <param name="column">The column to alter</param>
        /// <param name="operation">The type of operation for the field</param>
        /// <param name="fieldType">The type of the column</param>
        /// <param name="precision">Precision of the field</param>
        /// <param name="isNullable">Is the field nullable</param>
        /// <returns></returns>
        public override PersistanceMap.ITableQueryExpression<T> Column(string column, FieldOperation operation = FieldOperation.None, Type fieldType = null, string precision = null, bool? isNullable = null)
        {
            string expression = "";

            switch (operation)
            {
                case FieldOperation.None:
                    //TODO: precision???
                    var part = CreateColumn(column, fieldType, isNullable ?? true);
                    QueryPartsMap.AddAfter(part, QueryPartsMap.Parts.Any(p => p.OperationType == OperationType.Column) ? OperationType.Column : OperationType.CreateTable);
                    break;

                case FieldOperation.Add:
                    //TODO: precision???
                    if (fieldType == null)
                    {
                        throw new ArgumentNullException("fieldType", "Argument Fieldtype is not allowed to be null when adding a column");
                    }

                    expression = string.Format("ADD COLUMN {0} {1}{2}", column, fieldType.ToSqlDbType(), isNullable != null && !isNullable.Value ? " NOT NULL" : "");
                    QueryPartsMap.Add(new DelegateQueryPart(OperationType.AlterField, () => expression));
                    break;

                default:
                    throw new NotSupportedException("SQLite only supports ADD column");
            }

            return new TableQueryBuilder<T>(Context, QueryPartsMap);
        }

        #endregion
    }
}
