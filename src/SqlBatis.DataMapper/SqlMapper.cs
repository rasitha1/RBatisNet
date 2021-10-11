
#region Apache Notice
/*****************************************************************************
 * $Revision: 591621 $
 * $LastChangedDate: 2007-11-03 14:44:57 +0100 (sam., 03 nov. 2007) $
 * $LastChangedBy: gbayon $
 * 
 * iBATIS.NET Data Mapper
 * Copyright (C) 2006/2005 - The Apache Software Foundation
 *  
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 ********************************************************************************/
#endregion

#region Using
using System.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using Microsoft.Extensions.Logging;
using SqlBatis.DataMapper.Utilities;
using SqlBatis.DataMapper.Utilities.Objects;
using SqlBatis.DataMapper.Utilities.Objects.Members;
using SqlBatis.DataMapper.Configuration.ParameterMapping;
using SqlBatis.DataMapper.Configuration.ResultMapping;
using SqlBatis.DataMapper.DataExchange;
using SqlBatis.DataMapper.Exceptions;
using SqlBatis.DataMapper.MappedStatements;
using SqlBatis.DataMapper.SessionStore;
using SqlBatis.DataMapper.TypeHandlers;

#endregion

namespace SqlBatis.DataMapper
{
	/// <summary>
	/// Summary description for SqlMap.
	/// </summary>
	public class SqlMapper : ISqlMapper
	{

		#region Fields
		private readonly HybridDictionary _mappedStatements = new HybridDictionary();
		private readonly HybridDictionary _resultMaps = new HybridDictionary();
		private readonly HybridDictionary _parameterMaps = new HybridDictionary();
		private readonly TypeHandlerFactory _typeHandlerFactory; 
        private readonly DBHelperParameterCache _dbHelperParameterCache;
        private readonly ILoggerFactory _loggerFactory;


        /// <summary>
		/// Container session unique for each thread. 
		/// </summary>
        private ISessionStore _sessionStore;
        private readonly IObjectFactory _objectFactory;
        private readonly AccessorFactory _accessorFactory;
        private readonly DataExchangeFactory _dataExchangeFactory;
		#endregion

		#region Properties

        /// <summary>
        /// Name used to identify the the <see cref="SqlMapper"/>
        /// </summary>
        public string Id { get; }

		/// <summary>
		/// Allow to set a custom session store like the <see cref="AsyncLocalSessionStore"/>
		/// </summary>
		/// <remarks>Set it after the configuration and before use of the <see cref="SqlMapper"/></remarks>
		/// <example>
		/// sqlMapper.SessionStore = new AsyncLocalSessionStore( sqlMapper.Id );
		/// </example>
		public ISessionStore SessionStore
        {
            set => _sessionStore = value;
        }
	    
		/// <summary>
		///  Returns the DalSession instance 
		///  currently being used by the SqlMap.
		/// </summary>
		public ISqlMapSession LocalSession => _sessionStore.LocalSession;

        /// <summary>
        /// Gets a value indicating whether this instance is session started.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is session started; otherwise, <c>false</c>.
        /// </value>
		public bool IsSessionStarted => (_sessionStore.LocalSession != null);

        /// <summary>
        /// Gets the DB helper parameter cache.
        /// </summary>
        /// <value>The DB helper parameter cache.</value>
        public DBHelperParameterCache DBHelperParameterCache => _dbHelperParameterCache;

        /// <summary>
		/// Factory for DataExchange objects
		/// </summary>
        public DataExchangeFactory DataExchangeFactory => _dataExchangeFactory;

        /// <summary>
		/// The TypeHandlerFactory
		/// </summary>
        public TypeHandlerFactory TypeHandlerFactory => _typeHandlerFactory;

        /// <summary>
        /// The meta factory for object factory
        /// </summary>
        public IObjectFactory ObjectFactory => _objectFactory;

        /// <summary>
        /// The factory which build <see cref="IAccessor"/>
        /// </summary>
        public AccessorFactory AccessorFactory => _accessorFactory;

        #endregion

		#region Constructor (s) / Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMapper"/> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="objectFactory">The object factory.</param>
        /// <param name="accessorFactory">The accessor factory.</param>
        /// <param name="typeHandlerFactory"></param>
        /// <param name="parameterCache"></param>
        /// <param name="sessionStoreFactory"></param>
        /// <param name="loggerFactory"></param>
        internal SqlMapper(string id, IObjectFactory objectFactory,
            AccessorFactory accessorFactory, 
            TypeHandlerFactory typeHandlerFactory, 
            DBHelperParameterCache parameterCache,
            SessionStoreFactory sessionStoreFactory,
            ILoggerFactory loggerFactory) 
		{
            _typeHandlerFactory = typeHandlerFactory;
            _dbHelperParameterCache = parameterCache;
            _loggerFactory = loggerFactory;
            _objectFactory = objectFactory;
            _accessorFactory = accessorFactory;

            _dataExchangeFactory = new DataExchangeFactory(_typeHandlerFactory, _objectFactory, accessorFactory);
			Id = id;
            _sessionStore = sessionStoreFactory.GetSessionStore(Id);
		}
		#endregion

		#region Methods

		#region Manage Connection, Transaction
		
		/// <summary>
		/// Open a connection
		/// </summary>
		/// <returns></returns>
		public ISqlMapSession OpenConnection() 
		{
			if (_sessionStore.LocalSession != null) 
			{
				throw new DataMapperException("SqlMap could not invoke OpenConnection(). A connection is already started. Call CloseConnection first.");
			}
            ISqlMapSession session = CreateSqlMapSession();
			_sessionStore.Store(session);
			return session;
		}

		/// <summary>
		/// Open a connection, on the specified connection string.
		/// </summary>
		/// <param name="connectionString">The connection string</param>
		public ISqlMapSession OpenConnection(string connectionString)
		{
			if (_sessionStore.LocalSession != null) 
			{
				throw new DataMapperException("SqlMap could not invoke OpenConnection(). A connection is already started. Call CloseConnection first.");
			}
            ISqlMapSession session = CreateSqlMapSession(connectionString);
			_sessionStore.Store(session);
			return session;
		}

		/// <summary>
		/// Open a connection
		/// </summary>
		public void CloseConnection()
		{
			if (_sessionStore.LocalSession == null) 
			{
				throw new DataMapperException("SqlMap could not invoke CloseConnection(). No connection was started. Call OpenConnection() first.");
			}
			try
			{
				ISqlMapSession session = _sessionStore.LocalSession;
				session.CloseConnection();			
			} 
			catch(Exception ex)
			{
				throw new DataMapperException("SqlMapper could not CloseConnection(). Cause :"+ex.Message, ex);
			}
			finally 
			{
				_sessionStore.Dispose();
			}
		}


		/// <summary>
		/// Begins a database transaction.
		/// </summary>
		public ISqlMapSession BeginTransaction() 
		{
			if (_sessionStore.LocalSession != null) 
			{
				throw new DataMapperException("SqlMap could not invoke BeginTransaction(). A Transaction is already started. Call CommitTransaction() or RollbackTransaction first.");
			}
            ISqlMapSession session = CreateSqlMapSession();
			_sessionStore.Store(session);
			session.BeginTransaction();
			return session ;
		}

		/// <summary>
		/// Open a connection and begin a transaction on the specified connection string.
		/// </summary>
		/// <param name="connectionString">The connection string</param>
		public ISqlMapSession BeginTransaction(string connectionString)
		{
			if (_sessionStore.LocalSession != null) 
			{
				throw new DataMapperException("SqlMap could not invoke BeginTransaction(). A Transaction is already started. Call CommitTransaction() or RollbackTransaction first.");
			}
            ISqlMapSession session = CreateSqlMapSession(connectionString);
			_sessionStore.Store(session);
			session.BeginTransaction( connectionString );
			return session ;
		}

		/// <summary>
		/// Begins a database transaction on the currect session
		/// </summary>
		/// <param name="openConnection">Open a connection.</param>
		public ISqlMapSession BeginTransaction(bool openConnection)
		{
			ISqlMapSession session;

			if (openConnection)
			{
				session = this.BeginTransaction();
			}
			else
			{
				session = _sessionStore.LocalSession;
				if (session == null) 
				{
					throw new DataMapperException("SqlMap could not invoke BeginTransaction(). A session must be Open. Call OpenConnection() first.");
				}
				session.BeginTransaction(false);
			}

			return session;
		}

		/// <summary>
		/// Begins a database transaction with the specified isolation level.
		/// </summary>
		/// <param name="isolationLevel">
		/// The isolation level under which the transaction should run.
		/// </param>
		public ISqlMapSession BeginTransaction(IsolationLevel isolationLevel)
		{
			if (_sessionStore.LocalSession != null) 
			{
				throw new DataMapperException("SqlMap could not invoke BeginTransaction(). A Transaction is already started. Call CommitTransaction() or RollbackTransaction first.");
			}
            ISqlMapSession session = CreateSqlMapSession();
			_sessionStore.Store(session);
			session.BeginTransaction(isolationLevel);
			return session;
		}

		/// <summary>
		/// Open a connection and begin a transaction on the specified connection string.
		/// </summary>
		/// <param name="connectionString">The connection string</param>
		/// <param name="isolationLevel">The transaction isolation level for this connection.</param>
		public ISqlMapSession BeginTransaction(string connectionString, IsolationLevel isolationLevel)
		{
			if (_sessionStore.LocalSession != null) 
			{
				throw new DataMapperException("SqlMap could not invoke BeginTransaction(). A Transaction is already started. Call CommitTransaction() or RollbackTransaction first.");
			}
            ISqlMapSession session = CreateSqlMapSession(connectionString);
			_sessionStore.Store(session);
			session.BeginTransaction( connectionString, isolationLevel);
			return session;
		}

		/// <summary>
		/// Start a database transaction on the current session
		/// with the specified isolation level.
		/// </summary>
		/// <param name="openNewConnection">Open a connection.</param>
		/// <param name="isolationLevel">
		/// The isolation level under which the transaction should run.
		/// </param>
		public ISqlMapSession BeginTransaction(bool openNewConnection, IsolationLevel isolationLevel)
		{
			ISqlMapSession session;

			if (openNewConnection)
			{
				session = this.BeginTransaction(isolationLevel);
			}
			else
			{
				session = _sessionStore.LocalSession;
				if (session == null) 
				{
					throw new DataMapperException("SqlMap could not invoke BeginTransaction(). A session must be Open. Call OpenConnection() first.");
				}
				session.BeginTransaction(false, isolationLevel);
			}
			return session;
		}

		/// <summary>
		/// Begins a transaction on the current connection
		/// with the specified IsolationLevel value.
		/// </summary>
		/// <param name="isolationLevel">The transaction isolation level for this connection.</param>
		/// <param name="connectionString">The connection string</param>
		/// <param name="openNewConnection">Open a connection.</param>
		public ISqlMapSession BeginTransaction(string connectionString, bool openNewConnection, IsolationLevel isolationLevel)
		{
			ISqlMapSession session;

			if (openNewConnection)
			{
				session = this.BeginTransaction(connectionString, isolationLevel);
			}
			else
			{
				session = _sessionStore.LocalSession;
				if (session == null) 
				{
					throw new DataMapperException("SqlMap could not invoke BeginTransaction(). A session must be Open. Call OpenConnection() first.");
				}
				session.BeginTransaction(connectionString, false, isolationLevel);
			}
			return session;
		}

		/// <summary>
		/// Commits the database transaction.
		/// </summary>
		/// <remarks>
		/// Will close the connection.
		/// </remarks>
		public void CommitTransaction()
		{
			if (_sessionStore.LocalSession == null) 
			{
				throw new DataMapperException("SqlMap could not invoke CommitTransaction(). No Transaction was started. Call BeginTransaction() first.");
			}
			try
			{
				ISqlMapSession session = _sessionStore.LocalSession;
				session.CommitTransaction();
			} 
			finally 
			{
				_sessionStore.Dispose();
			}
		}

		/// <summary>
		/// Commits the database transaction.
		/// </summary>
		/// <param name="closeConnection">Close the connection</param>
		public void CommitTransaction(bool closeConnection)
		{
			if (_sessionStore.LocalSession == null) 
			{
				throw new DataMapperException("SqlMap could not invoke CommitTransaction(). No Transaction was started. Call BeginTransaction() first.");
			}
			try
			{
				ISqlMapSession session = _sessionStore.LocalSession;
				session.CommitTransaction(closeConnection);
			} 
			finally 
			{
				if (closeConnection)
				{
					_sessionStore.Dispose();
				}
			}
		}

		/// <summary>
		/// Rolls back a transaction from a pending state.
		/// </summary>
		/// <remarks>
		/// Will close the connection.
		/// </remarks>
		public void RollBackTransaction()
		{
			if (_sessionStore.LocalSession == null) 
			{
				throw new DataMapperException("SqlMap could not invoke RollBackTransaction(). No Transaction was started. Call BeginTransaction() first.");
			}
			try
			{
				ISqlMapSession session = _sessionStore.LocalSession;
				session.RollBackTransaction();			
			} 
			finally 
			{
				_sessionStore.Dispose();
			}
		}

		/// <summary>
		/// Rolls back a transaction from a pending state.
		/// </summary>
		/// <param name="closeConnection">Close the connection</param>
		public void RollBackTransaction(bool closeConnection)
		{
			if (_sessionStore.LocalSession == null) 
			{
				throw new DataMapperException("SqlMap could not invoke RollBackTransaction(). No Transaction was started. Call BeginTransaction() first.");
			}
			try
			{
				ISqlMapSession session = _sessionStore.LocalSession;
				session.RollBackTransaction(closeConnection);			
			} 
			finally 
			{
				if (closeConnection)
				{
					_sessionStore.Dispose();
				}
			}
		}

		#endregion

        private T GetWithLocalSessionSupport<T>(Func<ISqlMapSession,T> logic)
        {
            var isSessionLocal = false;
            var session = _sessionStore.LocalSession;

            if (session == null)
            {
                session = CreateSqlMapSession();
                isSessionLocal = true;
            }

            try
            {
                return logic(session);
            }
            finally
            {
                if (isSessionLocal)
                {
                    session.CloseConnection();
                }
            }
		}
        private void RunWithLocalSessionSupport<T>(Action<ISqlMapSession> logic)
        {
            GetWithLocalSessionSupport<T>(session =>
            {
                logic(session);
                return default;
            });
        }

		#region QueryForObject

		/// <summary>
		/// Executes a Sql SELECT statement that returns that returns data 
		/// to populate a single object instance.
		/// <p/>
		/// The parameter object is generally used to supply the input
		/// data for the WHERE clause parameter(s) of the SELECT statement.
		/// </summary>
		/// <param name="statementName">The name of the sql statement to execute.</param>
		/// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
		/// <returns> The single result object populated with the result set data.</returns>
		public object QueryForObject(string statementName, object parameterObject)
        {
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForObject(session, parameterObject);
            });
        }

		/// <summary>
		/// Executes a Sql SELECT statement that returns a single object of the type of the
		/// resultObject parameter.
		/// </summary>
		/// <param name="statementName">The name of the sql statement to execute.</param>
		/// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
		/// <param name="resultObject">An object of the type to be returned.</param>
		/// <returns>The single result object populated with the result set data.</returns>
		public object QueryForObject(string statementName, object parameterObject, object resultObject)
		{
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForObject(session, parameterObject, resultObject);
            });
        }

        #endregion

        #region QueryForObject .NET 2.0
        /// <summary>
        /// Executes a Sql SELECT statement that returns that returns data 
        /// to populate a single object instance.
        /// <p/>
        /// The parameter object is generally used to supply the input
        /// data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <returns> The single result object populated with the result set data.</returns>
        public T QueryForObject<T>(string statementName, object parameterObject)
        {
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForObject<T>(session, parameterObject);
            });
        }

        /// <summary>
        /// Executes a Sql SELECT statement that returns a single object of the type of the
        /// resultObject parameter.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="instanceObject">An object of the type to be returned.</param>
        /// <returns>The single result object populated with the result set data.</returns>
        public T QueryForObject<T>(string statementName, object parameterObject, T instanceObject)
        {
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForObject(session, parameterObject, instanceObject);
            });
        }
        #endregion

        #region QueryForMap, QueryForDictionary

        /// <summary>
		///  Alias to QueryForMap, .NET spirit.
		///  Feature idea by Ted Husted.
		/// </summary>
		/// <param name="statementName">The name of the sql statement to execute.</param>
		/// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
		/// <param name="keyProperty">The property of the result object to be used as the key.</param>
		/// <returns>A IDictionary (Hashtable) of object containing the rows keyed by keyProperty.</returns>
		public IDictionary QueryForDictionary(string statementName, object parameterObject, string keyProperty)
		{
			return QueryForMap( statementName, parameterObject, keyProperty);
		}

		/// <summary>
		/// Alias to QueryForMap, .NET spirit.
		///  Feature idea by Ted Husted.
		/// </summary>
		/// <param name="statementName">The name of the sql statement to execute.</param>
		/// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
		/// <param name="keyProperty">The property of the result object to be used as the key.</param>
		/// <param name="valueProperty">The property of the result object to be used as the value (or null)</param>
		/// <returns>A IDictionary (Hashtable) of object containing the rows keyed by keyProperty.</returns>
		///<exception cref="DataMapperException">If a transaction is not in progress, or the database throws an exception.</exception>
		public IDictionary QueryForDictionary(string statementName, object parameterObject, string keyProperty, string valueProperty)
		{
			return QueryForMap( statementName, parameterObject, keyProperty, valueProperty);
		}

		/// <summary>
		///  Executes the SQL and retuns all rows selected in a map that is keyed on the property named
		///  in the keyProperty parameter.  The value at each key will be the entire result object.
		/// </summary>
		/// <param name="statementName">The name of the sql statement to execute.</param>
		/// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
		/// <param name="keyProperty">The property of the result object to be used as the key.</param>
		/// <returns>A IDictionary (Hashtable) of object containing the rows keyed by keyProperty.</returns>
		public IDictionary QueryForMap(string statementName, object parameterObject, string keyProperty)
		{
			return QueryForMap(statementName, parameterObject, keyProperty, null);
		}

		/// <summary>
		/// Executes the SQL and retuns all rows selected in a map that is keyed on the property named
		/// in the keyProperty parameter.  The value at each key will be the value of the property specified
		/// in the valueProperty parameter.  If valueProperty is null, the entire result object will be entered.
		/// </summary>
		/// <param name="statementName">The name of the sql statement to execute.</param>
		/// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
		/// <param name="keyProperty">The property of the result object to be used as the key.</param>
		/// <param name="valueProperty">The property of the result object to be used as the value (or null)</param>
		/// <returns>A IDictionary (Hashtable) of object containing the rows keyed by keyProperty.</returns>
		///<exception cref="DataMapperException">If a transaction is not in progress, or the database throws an exception.</exception>
		public IDictionary QueryForMap(string statementName, object parameterObject, string keyProperty, string valueProperty)
		{
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForMap(session, parameterObject, keyProperty, valueProperty);
            });
        }
		
		#endregion

		#region QueryForList

		/// <summary>
		/// Executes a Sql SELECT statement that returns data to populate
		/// a number of result objects.
		/// <p/>
		///  The parameter object is generally used to supply the input
		/// data for the WHERE clause parameter(s) of the SELECT statement.
		/// </summary>
		/// <param name="statementName">The name of the sql statement to execute.</param>
		/// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
		/// <returns>A List of result objects.</returns>
		public IList QueryForList(string statementName, object parameterObject)
		{
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForList(session, parameterObject);
            });
        }
		
		/// <summary>
		/// Executes the SQL and retuns all rows selected.
		/// <p/>
		///  The parameter object is generally used to supply the input
		/// data for the WHERE clause parameter(s) of the SELECT statement.
		/// </summary>
		/// <param name="statementName">The name of the sql statement to execute.</param>
		/// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
		/// <param name="skipResults">The number of rows to skip over.</param>
		/// <param name="maxResults">The maximum number of rows to return.</param>
		/// <returns>A List of result objects.</returns>
		public IList QueryForList(string statementName, object parameterObject, int skipResults, int maxResults)	
		{
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForList(session, parameterObject, skipResults, maxResults);
            });
        }

		
		/// <summary>
		/// Executes a Sql SELECT statement that returns data to populate
		/// a number of result objects.
		/// <p/>
		///  The parameter object is generally used to supply the input
		/// data for the WHERE clause parameter(s) of the SELECT statement.
		/// </summary>
		/// <param name="statementName">The name of the sql statement to execute.</param>
		/// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
		/// <param name="resultObject">An Ilist object used to hold the objects.</param>
		/// <returns>A List of result objects.</returns>
		public void QueryForList(string statementName, object parameterObject, IList resultObject)
		{
            if (resultObject == null)
            {
                throw new DataMapperException("resultObject parameter must be instantiated before being passed to SqlMapper.QueryForList");
            }

            RunWithLocalSessionSupport<object>(session =>
            {
				var statement = GetMappedStatement(statementName);
				statement.ExecuteQueryForList(session, parameterObject, resultObject);
			});
        }
		
		#endregion
        
        #region QueryForList .NET 2.0
	    
	            /// <summary>
        /// Executes the SQL and retuns all rows selected in a map that is keyed on the property named
        /// in the keyProperty parameter.  The value at each key will be the value of the property specified
        /// in the valueProperty parameter.  If valueProperty is null, the entire result object will be entered.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="keyProperty">The property of the result object to be used as the key.</param>
        /// <param name="valueProperty">The property of the result object to be used as the value (or null)</param>
        /// <returns>A IDictionary of object containing the rows keyed by keyProperty.</returns>
        ///<exception cref="DataMapperException">If a transaction is not in progress, or the database throws an exception.</exception>
        public IDictionary<TK, TV> QueryForDictionary<TK, TV>(string statementName, object parameterObject, string keyProperty, string valueProperty)
        {
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForDictionary<TK, TV>(session, parameterObject, keyProperty, valueProperty);
            });
        }
	    
	    /// <summary>
        ///  Executes the SQL and retuns all rows selected in a map that is keyed on the property named
        ///  in the keyProperty parameter.  The value at each key will be the entire result object.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="keyProperty">The property of the result object to be used as the key.</param>
        /// <returns>A IDictionary of object containing the rows keyed by keyProperty.</returns>
        public IDictionary<TK, TV> QueryForDictionary<TK, TV>(string statementName, object parameterObject, string keyProperty)
        {
            return QueryForDictionary<TK, TV>(statementName, parameterObject, keyProperty, null);
        }

        /// <summary>
        /// Runs a query with a custom object that gets a chance to deal 
        /// with each row as it is processed.
        /// <p/>
        ///  The parameter object is generally used to supply the input
        /// data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="keyProperty">The property of the result object to be used as the key.</param>
        /// <param name="valueProperty">The property of the result object to be used as the value (or null)</param>
        /// <param name="rowDelegate">A delegate called once per row in the QueryForDictionary method></param>
        /// <returns>A IDictionary (Hashtable) of object containing the rows keyed by keyProperty.</returns>
        ///<exception cref="DataMapperException">If a transaction is not in progress, or the database throws an exception.</exception>
        public IDictionary<TK, TV> QueryForDictionary<TK, TV>(string statementName, object parameterObject, string keyProperty, string valueProperty, DictionaryRowDelegate<TK, TV> rowDelegate)
        {
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForDictionary(session, parameterObject, keyProperty, valueProperty, rowDelegate);
            });
        }
	    
	    
        /// <summary>
        /// Executes a Sql SELECT statement that returns data to populate
        /// a number of result objects.
        /// <p/>
        ///  The parameter object is generally used to supply the input
        /// data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <returns>A List of result objects.</returns>
        public IList<T> QueryForList<T>(string statementName, object parameterObject)
        {
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForList<T>(session, parameterObject);
            });
        }

        /// <summary>
        /// Executes the SQL and retuns all rows selected.
        /// <p/>
        ///  The parameter object is generally used to supply the input
        /// data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="skipResults">The number of rows to skip over.</param>
        /// <param name="maxResults">The maximum number of rows to return.</param>
        /// <returns>A List of result objects.</returns>
        public IList<T> QueryForList<T>(string statementName, object parameterObject, int skipResults, int maxResults)
        {
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForList<T>(session, parameterObject, skipResults, maxResults);
            });
        }


        /// <summary>
        /// Executes a Sql SELECT statement that returns data to populate
        /// a number of result objects.
        /// <p/>
        ///  The parameter object is generally used to supply the input
        /// data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="resultObject">An Ilist object used to hold the objects.</param>
        public void QueryForList<T>(string statementName, object parameterObject, IList<T> resultObject)
        {
            if (resultObject == null)
            {
                throw new DataMapperException("resultObject parameter must be instantiated before being passed to SqlMapper.QueryForList");
            }

            RunWithLocalSessionSupport<object>(session =>
            {
                var statement = GetMappedStatement(statementName);
                statement.ExecuteQueryForList(session, parameterObject, resultObject);
            });
        }
        #endregion

		#region QueryWithRowDelegate

		/// <summary>
		/// Runs a query for list with a custom object that gets a chance to deal 
		/// with each row as it is processed.
		/// <p/>
		///  The parameter object is generally used to supply the input
		/// data for the WHERE clause parameter(s) of the SELECT statement.
		/// </summary>
		/// <param name="statementName">The name of the sql statement to execute.</param>
		/// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
		/// <param name="rowDelegate"></param>
		/// <returns>A List of result objects.</returns>
		public IList QueryWithRowDelegate(string statementName, object parameterObject, RowDelegate rowDelegate)
		{
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForRowDelegate(session, parameterObject, rowDelegate);
            });
		}

        /// <summary>
        /// Runs a query for list with a custom object that gets a chance to deal 
        /// with each row as it is processed.
        /// <p/>
        ///  The parameter object is generally used to supply the input
        /// data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="rowDelegate"></param>
        /// <returns>A List of result objects.</returns>
        public IList<T> QueryWithRowDelegate<T>(string statementName, object parameterObject, RowDelegate<T> rowDelegate)
        {
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForRowDelegate(session, parameterObject, rowDelegate);
            });
        }

		/// <summary>
		/// Runs a query with a custom object that gets a chance to deal 
		/// with each row as it is processed.
		/// <p/>
		///  The parameter object is generally used to supply the input
		/// data for the WHERE clause parameter(s) of the SELECT statement.
		/// </summary>
		/// <param name="statementName">The name of the sql statement to execute.</param>
		/// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
		/// <param name="keyProperty">The property of the result object to be used as the key.</param>
		/// <param name="valueProperty">The property of the result object to be used as the value (or null)</param>
		/// <param name="rowDelegate"></param>
		/// <returns>A IDictionary (Hashtable) of object containing the rows keyed by keyProperty.</returns>
		///<exception cref="DataMapperException">If a transaction is not in progress, or the database throws an exception.</exception>
		public IDictionary QueryForMapWithRowDelegate(string statementName, object parameterObject, string keyProperty, string valueProperty, DictionaryRowDelegate rowDelegate)
		{
            return GetWithLocalSessionSupport(session =>
            {
                IMappedStatement statement = GetMappedStatement(statementName);
                return statement.ExecuteQueryForMapWithRowDelegate(session, parameterObject, keyProperty, valueProperty, rowDelegate);
            });
        }
		
		#endregion

		#region Query Insert, Update, Delete

		/// <summary>
		/// Executes a Sql INSERT statement.
		/// Insert is a bit different from other update methods, as it
		/// provides facilities for returning the primary key of the
		/// newly inserted row (rather than the effected rows).  This
		/// functionality is of course optional.
		/// <p/>
		/// The parameter object is generally used to supply the input
		/// data for the INSERT values.
		/// </summary>
		/// <param name="statementName">The name of the statement to execute.</param>
		/// <param name="parameterObject">The parameter object.</param>
		/// <returns> The primary key of the newly inserted row.  
		/// This might be automatically generated by the RDBMS, 
		/// or selected from a sequence table or other source.
		/// </returns>
		public object Insert(string statementName, object parameterObject)
		{
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteInsert(session, parameterObject);
            });
        }

		/// <summary>
		/// Executes a Sql UPDATE statement.
		/// Update can also be used for any other update statement type,
		/// such as inserts and deletes.  Update returns the number of
		/// rows effected.
		/// <p/>
		/// The parameter object is generally used to supply the input
		/// data for the UPDATE values as well as the WHERE clause parameter(s).
		/// </summary>
		/// <param name="statementName">The name of the statement to execute.</param>
		/// <param name="parameterObject">The parameter object.</param>
		/// <returns>The number of rows effected.</returns>
		public int Update(string statementName, object parameterObject)
		{
            return GetWithLocalSessionSupport(session =>
            {
                var statement = GetMappedStatement(statementName);
                return statement.ExecuteUpdate(session, parameterObject);
            });
        }

		/// <summary>
		///  Executes a Sql DELETE statement.
		///  Delete returns the number of rows effected.
		/// </summary>
		/// <param name="statementName">The name of the statement to execute.</param>
		/// <param name="parameterObject">The parameter object.</param>
		/// <returns>The number of rows effected.</returns>
		public int Delete(string statementName, object parameterObject)
		{
            return GetWithLocalSessionSupport(session =>
            {
                IMappedStatement statement = GetMappedStatement(statementName);
                return statement.ExecuteUpdate(session, parameterObject);
            });
        }

		#endregion

		#region Get/Add ParemeterMap, ResultMap, MappedStatement, TypeAlias, DataSource, CacheModel

		/// <summary>
		/// Gets a MappedStatement by name
		/// </summary>
		/// <param name="id"> The id of the statement</param>
		/// <returns> The MappedStatement</returns>
		public IMappedStatement GetMappedStatement(string id) 
		{
			if (_mappedStatements.Contains(id) == false) 
			{
				throw new DataMapperException("This SQL map does not contain a MappedStatement named " + id);
			}
			return (IMappedStatement) _mappedStatements[id];
		}

		/// <summary>
		/// Adds a (named) MappedStatement.
		/// </summary>
		/// <param name="key"> The key name</param>
		/// <param name="mappedStatement">The statement to add</param>
		public void AddMappedStatement(string key, IMappedStatement mappedStatement) 
		{
			if (_mappedStatements.Contains(key)) 
			{
				throw new DataMapperException("This SQL map already contains a MappedStatement named " + mappedStatement.Id);
			}
			_mappedStatements.Add(key, mappedStatement);
		}

		/// <summary>
		/// The MappedStatements collection
		/// </summary>
        public HybridDictionary MappedStatements => _mappedStatements;

        /// <summary>
		/// Get a ParameterMap by name
		/// </summary>
		/// <param name="name">The name of the ParameterMap</param>
		/// <returns>The ParameterMap</returns>
		public ParameterMap GetParameterMap(string name) 
		{
			if (!_parameterMaps.Contains(name)) 
			{
				throw new DataMapperException("This SQL map does not contain an ParameterMap named " + name + ".  ");
			}
			return (ParameterMap) _parameterMaps[name];
		}

		/// <summary>
		/// Adds a (named) ParameterMap.
		/// </summary>
		/// <param name="parameterMap">the ParameterMap to add</param>
        public void AddParameterMap(ParameterMap parameterMap) 
		{
			if (_parameterMaps.Contains(parameterMap.Id)) 
			{
				throw new DataMapperException("This SQL map already contains an ParameterMap named " + parameterMap.Id);
			}
			_parameterMaps.Add(parameterMap.Id, parameterMap);
		}

		/// <summary>
		/// Gets a ResultMap by name
		/// </summary>
		/// <param name="name">The name of the result map</param>
		/// <returns>The ResultMap</returns>
        public IResultMap GetResultMap(string name) 
		{
			if (_resultMaps.Contains(name) == false) 
			{
				throw new DataMapperException("This SQL map does not contain an ResultMap named " + name);
			}
			return (ResultMap) _resultMaps[name];
		}

		/// <summary>
		/// Adds a (named) ResultMap
		/// </summary>
		/// <param name="resultMap">The ResultMap to add</param>
        public void AddResultMap(IResultMap resultMap) 
		{
			if (_resultMaps.Contains(resultMap.Id)) 
			{
				throw new DataMapperException("This SQL map already contains an ResultMap named " + resultMap.Id);
			}
			_resultMaps.Add(resultMap.Id, resultMap);
		}

		/// <summary>
		/// The ParameterMap collection
		/// </summary>
		public HybridDictionary ParameterMaps => _parameterMaps;

        /// <summary>
		/// The ResultMap collection
		/// </summary>
        public HybridDictionary ResultMaps => _resultMaps;

        /// <summary>
		/// The DataSource
		/// </summary>
        public IDataSource DataSource { get; set; }
		
		#endregion


        /// <summary>
        /// Creates a new SqlMapSession that will be used to query the data source.
        /// </summary>
        /// <returns>A new session</returns>
        public ISqlMapSession CreateSqlMapSession()
		{
			ISqlMapSession session = new SqlMapSession(this, _loggerFactory.CreateLogger<SqlMapSession>());
		    session.CreateConnection();

            return session;
		}


        /// <summary>
        /// Creates the SQL map session.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>A new session</returns>
        public ISqlMapSession CreateSqlMapSession(string connectionString)
        {
            ISqlMapSession session = new SqlMapSession(this, _loggerFactory.CreateLogger<SqlMapSession>());
            session.CreateConnection(connectionString);

            return session;
        }

		#endregion
	}
}
