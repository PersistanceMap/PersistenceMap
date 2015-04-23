﻿using System;
using NUnit.Framework;
using PersistanceMap.QueryParts;

namespace PersistanceMap.Sqlite.UnitTest
{
    [TestFixture]
    public class QueryCompilerTest
    {
        #region Database Tests

        [Test]
        public void SqliteQueryCompilerCompileAlterTableTest()
        {
            var part = new DelegateQueryPart(OperationType.AlterTable, () => "Table");
            var parts = new QueryPartsContainer();
            parts.Add(part);

            var compiler = new QueryCompiler();
            var query = compiler.Compile(parts);

            Assert.AreEqual(query.QueryString, "ALTER TABLE Table ");
        }

        [Test]
        public void SqliteQueryCompilerCompileDropTest()
        {
            var part = new DelegateQueryPart(OperationType.DropTable, () => "Table");
            var parts = new QueryPartsContainer();
            parts.Add(part);

            var compiler = new QueryCompiler();
            var query = compiler.Compile(parts);

            Assert.AreEqual(query.QueryString, "DROP TABLE Table");
        }

        [Test]
        public void SqliteQueryCompilerCompileDropColumnTest()
        {
            var part = new DelegateQueryPart(OperationType.DropColumn, () => "ColumnName");
            var parts = new QueryPartsContainer();
            parts.Add(part);

            var compiler = new QueryCompiler();
            var query = compiler.Compile(parts);

            Assert.AreEqual(query.QueryString, "DROP COLUMN ColumnName");
        }

        [Test]
        public void SqliteQueryCompilerCompileAddColumnTest()
        {
            var part = new ValueCollectionQueryPart(OperationType.AddColumn);
            part.AddValue(KeyValuePart.MemberName, "ColumnName");
            part.AddValue(KeyValuePart.MemberType, "int");
            part.AddValue(KeyValuePart.Nullable, null);

            var parts = new QueryPartsContainer();
            parts.Add(part);

            var compiler = new QueryCompiler();
            var query = compiler.Compile(parts);

            Assert.AreEqual(query.QueryString, "ADD COLUMN ColumnName int");
        }

        [Test]
        public void SqliteQueryCompilerCompileAddColumnMissingNUllableTest()
        {
            var part = new ValueCollectionQueryPart(OperationType.AddColumn);
            part.AddValue(KeyValuePart.MemberName, "ColumnName");
            part.AddValue(KeyValuePart.MemberType, "int");

            var parts = new QueryPartsContainer();
            parts.Add(part);

            var compiler = new QueryCompiler();
            var query = compiler.Compile(parts);

            Assert.AreEqual(query.QueryString, "ADD COLUMN ColumnName int");
        }

        [Test]
        public void SqliteQueryCompilerCompileAddColumnNullableTest()
        {
            var part = new ValueCollectionQueryPart(OperationType.AddColumn);
            part.AddValue(KeyValuePart.MemberName, "ColumnName");
            part.AddValue(KeyValuePart.MemberType, "int");
            part.AddValue(KeyValuePart.Nullable, true.ToString());

            var parts = new QueryPartsContainer();
            parts.Add(part);

            var compiler = new QueryCompiler();
            var query = compiler.Compile(parts);

            Assert.AreEqual(query.QueryString, "ADD COLUMN ColumnName int");
        }

        [Test]
        public void SqliteQueryCompilerCompileAddColumnNotNullTest()
        {
            var part = new ValueCollectionQueryPart(OperationType.AddColumn);
            part.AddValue(KeyValuePart.MemberName, "ColumnName");
            part.AddValue(KeyValuePart.MemberType, "int");
            part.AddValue(KeyValuePart.Nullable, false.ToString());

            var parts = new QueryPartsContainer();
            parts.Add(part);

            var compiler = new QueryCompiler();
            var query = compiler.Compile(parts);

            Assert.AreEqual(query.QueryString, "ADD COLUMN ColumnName int NOT NULL");
        }

        [Test]
        public void SqliteQueryCompilerCompileRenameTableTest()
        {
            var part = new ValueCollectionQueryPart(OperationType.RenameTable);
            part.AddValue(KeyValuePart.Key, "OriginalTable");
            part.AddValue(KeyValuePart.Value, "NewTable");

            var parts = new QueryPartsContainer();
            parts.Add(part);

            var compiler = new QueryCompiler();
            var query = compiler.Compile(parts);

            Assert.AreEqual(query.QueryString, "ALTER TABLE OriginalTable RENAME TO NewTable");
        }

        #endregion
    }
}
