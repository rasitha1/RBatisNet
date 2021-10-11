#region Apache Notice
/*****************************************************************************
 * $Header: $
 * $Revision: 476843 $
 * $Date: 2006-11-19 17:07:45 +0100 (dim., 19 nov. 2006) $
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


using System.Collections;
using SqlBatis.DataMapper.Commands;
using SqlBatis.DataMapper.Exceptions;
using SqlBatis.DataMapper.Configuration.Statements;
using SqlBatis.DataMapper.MappedStatements.ResultStrategy;

namespace SqlBatis.DataMapper.MappedStatements
{
	/// <summary>
	/// Summary description for DeleteMappedStatement.
	/// </summary>
    public sealed class DeleteMappedStatement : MappedStatement
	{
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sqlMap">An SqlMap</param>
        /// <param name="statement">An SQL statement</param>
        /// <param name="resultFactory"></param>
        internal DeleteMappedStatement(ISqlMapper sqlMap, IStatement statement, PreparedCommandFactory commandFactory, ResultStrategyFactory resultFactory)
            : base(sqlMap, statement, commandFactory, resultFactory)
		{ }

		#region ExecuteQueryForMap

		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <param name="parameterObject"></param>
		/// <param name="keyProperty"></param>
		/// <param name="valueProperty"></param>
		/// <returns></returns>
		public override IDictionary ExecuteQueryForMap(ISqlMapSession session, object parameterObject, string keyProperty, string valueProperty )
		{
			throw new DataMapperException("Delete statements cannot be executed as a query for map.");
		}

		#endregion

		#region ExecuteInsert

		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <param name="parameterObject"></param>
		/// <returns></returns>
        public override object ExecuteInsert(ISqlMapSession session, object parameterObject)
		{
			throw new DataMapperException("Delete statements cannot be executed as a query insert.");
		}

		#endregion

		#region ExecuteQueryForList

		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <param name="parameterObject"></param>
		/// <param name="resultObject"></param>
        public override void ExecuteQueryForList(ISqlMapSession session, object parameterObject, IList resultObject)
		{
			throw new DataMapperException("Delete statements cannot be executed as a query for list.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <param name="parameterObject"></param>
		/// <param name="skipResults"></param>
		/// <param name="maxResults"></param>
		/// <returns></returns>
        public override IList ExecuteQueryForList(ISqlMapSession session, object parameterObject, int skipResults, int maxResults)
		{
			throw new DataMapperException("Delete statements cannot be executed as a query for list.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <param name="parameterObject"></param>
		/// <returns></returns>
        public override IList ExecuteQueryForList(ISqlMapSession session, object parameterObject)
		{
			throw new DataMapperException("Delete statements cannot be executed as a query for list.");
		}

		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <param name="parameterObject"></param>
		/// <param name="rowDelegate"></param>
		/// <returns></returns>
        public override IList ExecuteQueryForRowDelegate(ISqlMapSession session, object parameterObject, RowDelegate rowDelegate)
		{
			throw new DataMapperException("Delete statements cannot be executed as a query for row delegate.");
		}

		
		#region ExecuteForObject

		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <param name="parameterObject"></param>
		/// <returns></returns>
        public override object ExecuteQueryForObject(ISqlMapSession session, object parameterObject)
		{
			throw new DataMapperException("Delete statements cannot be executed as a query for object.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <param name="parameterObject"></param>
		/// <param name="resultObject"></param>
		/// <returns></returns>
        public override object ExecuteQueryForObject(ISqlMapSession session, object parameterObject, object resultObject)
		{
			throw new DataMapperException("Delete statements cannot be executed as a query for object.");
		}

		#endregion
	}
}
