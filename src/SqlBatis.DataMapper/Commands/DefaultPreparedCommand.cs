
#region Apache Notice
/*****************************************************************************
 * $Header: $
 * $Revision: 591573 $
 * $Date: 2007-11-03 11:17:59 +0100 (sam., 03 nov. 2007) $
 * 
 * iBATIS.NET Data Mapper
 * Copyright (C) 2005 - Gilles Bayon
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

using System;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using Microsoft.Extensions.Logging;
using SqlBatis.DataMapper.Utilities.Objects;
using SqlBatis.DataMapper.Configuration.ParameterMapping;
using SqlBatis.DataMapper.Configuration.Statements;
using SqlBatis.DataMapper.Exceptions;
using SqlBatis.DataMapper.Scope;


namespace SqlBatis.DataMapper.Commands
{
	/// <summary>
	/// Summary description for DefaultPreparedCommand.
	/// </summary>
	internal class DefaultPreparedCommand : IPreparedCommand
	{
        private readonly ILogger<DefaultPreparedCommand> _logger;

        public DefaultPreparedCommand(ILogger<DefaultPreparedCommand> logger)
        {
            _logger = logger;
        }

		/// <summary>
		/// Create an IDbCommand for the SqlMapSession and the current SQL Statement
		/// and fill IDbCommand IDataParameter's with the parameterObject.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="session">The SqlMapSession</param>
		/// <param name="statement">The IStatement</param>
		/// <param name="parameterObject">
		/// The parameter object that will fill the sql parameter
		/// </param>
		/// <returns>An IDbCommand with all the IDataParameter filled.</returns>
		public void Create(RequestScope request, ISqlMapSession session, IStatement statement, object parameterObject )
		{
			// the IDbConnection & the IDbTransaction are assign in the CreateCommand 
            request.IDbCommand = new DbCommandDecorator(session.CreateCommand(statement.CommandType), request);
			
			request.IDbCommand.CommandText = request.PreparedStatement.PreparedSql;

			if (_logger.IsEnabled(LogLevel.Debug))
			{
				_logger.LogDebug("Statement Id: [{StatementId}] PreparedStatement : [{PreparedStatement}]", statement.Id, request.IDbCommand.CommandText);
			}

			ApplyParameterMap( session, request.IDbCommand, request, statement, parameterObject  );
		}


        /// <summary>
        /// Applies the parameter map.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="command">The command.</param>
        /// <param name="request">The request.</param>
        /// <param name="statement">The statement.</param>
        /// <param name="parameterObject">The parameter object.</param>
		protected virtual void ApplyParameterMap
			( ISqlMapSession session, IDbCommand command,
			RequestScope request, IStatement statement, object parameterObject )
		{
			StringCollection properties = request.PreparedStatement.DbParametersName;
            IDbDataParameter[] parameters = request.PreparedStatement.DbParameters;
            StringBuilder paramLogList = new StringBuilder(); // Log info
            StringBuilder typeLogList = new StringBuilder(); // Log info
            
			int count = properties.Count;

            for ( int i = 0; i < count; ++i )
			{
                IDbDataParameter sqlParameter = parameters[i];
                IDbDataParameter parameterCopy = command.CreateParameter();
				ParameterProperty property = request.ParameterMap.GetProperty(i);

                if (_logger.IsEnabled(LogLevel.Debug))
				{
                    paramLogList.Append(sqlParameter.ParameterName);
                    paramLogList.Append("=[");
                    typeLogList.Append(sqlParameter.ParameterName);
                    typeLogList.Append("=[");
				}

				if (command.CommandType == CommandType.StoredProcedure)
				{
					// A store procedure must always use a ParameterMap 
					// to indicate the mapping order of the properties to the columns
					if (request.ParameterMap == null) // Inline Parameters
					{
						throw new DataMapperException("A procedure statement tag must alway have a parameterMap attribute, which is not the case for the procedure '"+statement.Id+"'."); 
					}
					else // Parameters via ParameterMap
					{
						if (property.DirectionAttribute.Length == 0)
						{
							property.Direction = sqlParameter.Direction;
						}

						sqlParameter.Direction = property.Direction;					
					}
				}

                if (_logger.IsEnabled(LogLevel.Debug))
				{
                    paramLogList.Append(property.PropertyName);
                    paramLogList.Append(",");
				}

				request.ParameterMap.SetParameter(property, parameterCopy, parameterObject );

				parameterCopy.Direction = sqlParameter.Direction;

				// With a ParameterMap, we could specify the ParameterDbTypeProperty
				if (request.ParameterMap != null)
				{
                    if (property.DbType != null && property.DbType.Length > 0)
					{
						string dbTypePropertyName = session.DataSource.DbProvider.ParameterDbTypeProperty;
						object propertyValue = ObjectProbe.GetMemberValue(sqlParameter, dbTypePropertyName, request.DataExchangeFactory.AccessorFactory);
						ObjectProbe.SetMemberValue(parameterCopy, dbTypePropertyName, propertyValue, 
							request.DataExchangeFactory.ObjectFactory, request.DataExchangeFactory.AccessorFactory);
					}
					else
					{
						//parameterCopy.DbType = sqlParameter.DbType;
					}
				}
				else
				{
					//parameterCopy.DbType = sqlParameter.DbType;
				}

                if (_logger.IsEnabled(LogLevel.Debug))
				{
					if (parameterCopy.Value == DBNull.Value) 
					{
                        paramLogList.Append("null");
                        paramLogList.Append("], ");
                        typeLogList.Append("System.DBNull, null");
                        typeLogList.Append("], ");
					} 
					else 
					{

                        paramLogList.Append(parameterCopy.Value);
                        paramLogList.Append("], ");

						// sqlParameter.DbType could be null (as with Npgsql)
						// if PreparedStatementFactory did not find a dbType for the parameter in:
						// line 225: "if (property.DbType.Length >0)"
						// Use parameterCopy.DbType

						//typeLogList.Append( sqlParameter.DbType.ToString() );
                        typeLogList.Append(parameterCopy.DbType.ToString());
                        typeLogList.Append(", ");
                        typeLogList.Append(parameterCopy.Value?.GetType());
                        typeLogList.Append("], ");
					}
				}

				// JIRA-49 Fixes (size, precision, and scale)
				if (session.DataSource.DbProvider.SetDbParameterSize) 
				{
					if (sqlParameter.Size > 0) 
					{
						parameterCopy.Size = sqlParameter.Size;
					}
				}

				if (session.DataSource.DbProvider.SetDbParameterPrecision) 
				{
					parameterCopy.Precision = sqlParameter.Precision;
				}
				
				if (session.DataSource.DbProvider.SetDbParameterScale) 
				{
					parameterCopy.Scale = sqlParameter.Scale;
				}				

				parameterCopy.ParameterName = sqlParameter.ParameterName;

				command.Parameters.Add( parameterCopy );
			}

            if (_logger.IsEnabled(LogLevel.Debug) && properties.Count>0)
			{
				_logger.LogDebug("Statement Id: [{StatementId}] Parameters: [{Parameters}]", statement.Id, paramLogList.ToString(0, paramLogList.Length - 2));
                _logger.LogDebug("Statement Id: [{StatementId}] Types: [{Types}]", statement.Id, typeLogList.ToString(0, typeLogList.Length - 2));
			}
		}

	}
}
