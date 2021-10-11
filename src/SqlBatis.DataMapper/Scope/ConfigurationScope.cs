
#region Apache Notice
/*****************************************************************************
 * $Header: $
 * $Revision: 576082 $
 * $Date: 2007-09-16 14:04:01 +0200 (dim., 16 sept. 2007) $
 * 
 * iBATIS.NET Data Mapper
 * Copyright (C) 2004 - Gilles Bayon
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using SqlBatis.DataMapper.Exceptions;
using SqlBatis.DataMapper.Utilities;
using SqlBatis.DataMapper.Utilities.Objects;
using SqlBatis.DataMapper.Configuration;
using SqlBatis.DataMapper.DataExchange;
using SqlBatis.DataMapper.Proxy;
using SqlBatis.DataMapper.TypeHandlers;

#endregion

namespace SqlBatis.DataMapper.Scope
{
	/// <summary>
	/// The ConfigurationScope maintains the state of the build process.
	/// </summary>
	public class ConfigurationScope : IScope
	{
		/// <summary>
		/// Empty parameter map
		/// </summary>
        public const string EMPTY_PARAMETER_MAP = "iBATIS.Empty.ParameterMap";


		#region Fields
		
		private readonly ErrorContext _errorContext;
		private readonly HybridDictionary _providers = new HybridDictionary();
        private readonly HybridDictionary _sqlIncludes = new HybridDictionary();

		private readonly NameValueCollection _properties = new NameValueCollection();

		private XmlDocument _sqlMapConfigDocument;
		private XmlDocument _sqlMapDocument;
		private XmlNode _nodeContext;

		private bool _useConfigFileWatcher;
		private bool _useStatementNamespaces;
		private bool _useReflectionOptimizer = true;
		private bool _validateSqlMap;
		private bool _isCallFromDao;

        private ISqlMapper _sqlMapper;
		private string _sqlMapNamespace;
		private DataSource _dataSource;
		private bool _isXmlValid = true;
		private XmlNamespaceManager _nsmgr;

        #endregion
	
		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public ConfigurationScope()
		{
			_errorContext = new ErrorContext();

			_providers.Clear();
		}
		#endregion 

		#region Properties

        /// <summary>
        /// The list of sql fragment
        /// </summary>
        public HybridDictionary SqlIncludes
        {
            get { return _sqlIncludes; }
        }
	    
		/// <summary>
		/// XmlNamespaceManager
		/// </summary>
		public XmlNamespaceManager XmlNamespaceManager
		{
			set { _nsmgr = value; }
			get { return _nsmgr; }
		}
		
		/// <summary>
		/// Set if the parser should validate the sqlMap documents
		/// </summary>
		public bool ValidateSqlMap
		{
			set { _validateSqlMap = value; }
			get { return _validateSqlMap; }
		}

		/// <summary>
		/// Tells us if the xml configuration file validate the schema 
		/// </summary>
		public bool IsXmlValid
		{
			set { _isXmlValid = value; }
			get { return _isXmlValid; }
		}


		/// <summary>
		/// The current SqlMap namespace.
		/// </summary>
		public string SqlMapNamespace
		{
			set { _sqlMapNamespace = value; }
            get { return _sqlMapNamespace; }
		}

		/// <summary>
		/// The SqlMapper we are building.
		/// </summary>
		public ISqlMapper SqlMapper
		{
			set { _sqlMapper = value; }
			get { return _sqlMapper; }
		}


		/// <summary>
		/// A factory for DataExchange objects
		/// </summary>
		public DataExchangeFactory DataExchangeFactory
		{
			get { return _sqlMapper.DataExchangeFactory; }
		}

		/// <summary>
		/// The current context node we are analyzing
		/// </summary>
		public XmlNode NodeContext
		{
			set { _nodeContext = value; }
			get { return _nodeContext; }
		}

		/// <summary>
		/// The XML SqlMap config file
		/// </summary>
		public XmlDocument SqlMapConfigDocument
		{
			set { _sqlMapConfigDocument = value; }
			get { return _sqlMapConfigDocument; }
		}

		/// <summary>
		/// A XML SqlMap file
		/// </summary>
		public XmlDocument SqlMapDocument
		{
			set { _sqlMapDocument = value; }
			get { return _sqlMapDocument; }
		}

		/// <summary>
		/// Tell us if we use statements namespaces
		/// </summary>
		public bool UseStatementNamespaces
		{
			set { _useStatementNamespaces = value; }
			get { return _useStatementNamespaces; }
		}
		
		/// <summary>
		///  Get the request's error context
		/// </summary>
		public ErrorContext ErrorContext
		{
			get { return _errorContext; }
		}

		/// <summary>
		///  List of providers
		/// </summary>
		public HybridDictionary Providers
		{
			get { return _providers; }
		}

		/// <summary>
		///  List of global properties
		/// </summary>
		public NameValueCollection Properties
		{
			get { return _properties; }
		}


        /// <summary>
	    ///     Provides the <see cref="ILazyFactory" /> implementation
	    /// </summary>
	    public Type LazyFactoryType { get; set; }

        #endregion

        /// <summary>
        /// Register under Statement Name or Fully Qualified Statement Name
        /// </summary>
        /// <param name="id">An Identity</param>
        /// <returns>The new Identity</returns>
        public string ApplyNamespace(string id)
        {
            string newId = id;

            if (!string.IsNullOrEmpty(_sqlMapNamespace)
                && !string.IsNullOrEmpty(id) && id.IndexOf(".", StringComparison.Ordinal) < 0)
            {
                newId = _sqlMapNamespace + DomSqlMapBuilder.DOT + id;
            }
            return newId;
        }

        /// <summary>
        /// Resolves the type handler.
        /// </summary>
        /// <param name="clazz">The clazz.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="clrType">Type of the CLR.</param>
        /// <param name="dbType">Type of the db.</param>
        /// <param name="forSetter">if set to <c>true</c> [for setter].</param>
        /// <returns></returns>
		public ITypeHandler ResolveTypeHandler(Type clazz, string memberName, string clrType, string dbType, bool forSetter)
		{
			ITypeHandler handler;
			if (clazz==null)
			{
                handler = DataExchangeFactory.TypeHandlerFactory.GetUnkownTypeHandler();
			}
			else if (typeof(IDictionary).IsAssignableFrom(clazz) || typeof(IDictionary<string, object>).IsAssignableFrom(clazz)) 
			{
				// IDictionary
				if (string.IsNullOrEmpty(clrType)) 
				{
                    handler = DataExchangeFactory.TypeHandlerFactory.GetUnkownTypeHandler(); 
				} 
				else 
				{
					try 
					{
						Type type = TypeUtils.ResolveType(clrType);
                        handler = DataExchangeFactory.TypeHandlerFactory.GetTypeHandler(type, dbType);
					} 
					catch (Exception e) 
					{
                       throw new ConfigurationException("Error. Could not set TypeHandler.  Cause: " + e.Message, e);
                    }
				}
			}
            else if (DataExchangeFactory.TypeHandlerFactory.GetTypeHandler(clazz, dbType) != null) 
			{
				// Primitive
                handler = DataExchangeFactory.TypeHandlerFactory.GetTypeHandler(clazz, dbType);
			}
			else 
			{
				// .NET object
				if (string.IsNullOrEmpty(clrType)) 
				{
					Type type;
					if (forSetter)
					{
						type = ObjectProbe.GetMemberTypeForSetter(clazz, memberName);
					}
					else
					{
						type = ObjectProbe.GetMemberTypeForGetter(clazz, memberName);	
					}
                    handler = DataExchangeFactory.TypeHandlerFactory.GetTypeHandler(type, dbType);
				} 
				else 
				{
					try 
					{
						Type type = TypeUtils.ResolveType(clrType);
                        handler = DataExchangeFactory.TypeHandlerFactory.GetTypeHandler(type, dbType);
					} 
					catch (Exception e) 
					{
                       throw new ConfigurationException("Error. Could not set TypeHandler.  Cause: " + e.Message, e);
					}
				}
			}

			return handler;
		}

	}
}
