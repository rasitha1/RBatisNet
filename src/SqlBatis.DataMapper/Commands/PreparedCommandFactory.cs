
#region Apache Notice
/*****************************************************************************
 * $Header: $
 * $Revision: 512878 $
 * $Date: 2007-02-28 18:57:11 +0100 (mer., 28 févr. 2007) $
 * 
 * iBATIS.NET Data Mapper
 * Copyright (C) 2005 - Gilles Bayon
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



#endregion

using Microsoft.Extensions.Logging;

namespace SqlBatis.DataMapper.Commands
{
	/// <summary>
	/// Summary description for PreparedCommandFactory.
	/// </summary>
    public sealed class PreparedCommandFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public PreparedCommandFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
		/// <summary>
		/// Get an IPreparedCommand.
		/// </summary>
		/// <returns></returns>
		public IPreparedCommand GetPreparedCommand(bool isEmbedStatementParams)
        {
            IPreparedCommand preparedCommand = new DefaultPreparedCommand(_loggerFactory.CreateLogger<DefaultPreparedCommand>());

            return preparedCommand;
        }

	}
}
