﻿using System;
using System.Data.SqlServerCe;

namespace PersistanceMap
{
    public class SqlCeConnectionProvider : IConnectionProvider
    {
        private readonly Lazy<ConnectionStringFactory> _connectionStringFactory;

        public SqlCeConnectionProvider(string connectionString)
        {
            // format the string
            ConnectionString = connectionString;
            _connectionStringFactory = new Lazy<ConnectionStringFactory>(() => new ConnectionStringFactory());
        }

        /// <summary>
        /// The connectionstring
        /// </summary>
        protected string ConnectionString { get; private set; }

        /// <summary>
        /// The name of the database
        /// </summary>
        public string Database
        {
            get
            {
                return _connectionStringFactory.Value.GetDatabase(ConnectionString);
            }
            set
            {
                // set new database name
                ConnectionString = _connectionStringFactory.Value.SetDatabase(value, ConnectionString);
            }
        }

        private IQueryCompiler _queryCompiler;
        /// <summary>
        /// The querycompiler that is needed to compiel a querypartsmap to a sql statement
        /// </summary>
        public virtual IQueryCompiler QueryCompiler
        {
            get
            {
                if (_queryCompiler == null)
                    _queryCompiler = new QueryCompiler();

                return _queryCompiler;
            }
        }

        /// <summary>
        /// Execute the sql string to the RDBMS
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual IReaderContext Execute(string query)
        {
            var connection = new SqlCeConnection(ConnectionString);

            connection.Open();
            var command = new SqlCeCommand(query, connection);

            return new SqlCeContextReader(command.ExecuteReader(), connection, command);
        }

        /// <summary>
        /// Execute the sql string to the RDBMS
        /// </summary>
        /// <param name="query"></param>
        public void ExecuteNonQuery(string query)
        {
            using (var connection = new SqlCeConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCeCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        #region IDisposeable Implementation

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        internal bool IsDisposed { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases resources held by the object.
        /// </summary>
        public virtual void Dispose(bool disposing)
        {
            lock (this)
            {
                if (disposing && !IsDisposed)
                {
                    IsDisposed = true;
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Releases resources before the object is reclaimed by garbage collection.
        /// </summary>
        ~SqlCeConnectionProvider()
        {
            Dispose(false);
        }

        #endregion
    }
}
