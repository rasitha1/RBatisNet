#region Apache Notice
/*****************************************************************************
 * $Revision: 374175 $
 * $LastChangedDate: 2006-02-19 12:37:22 +0100 (Sun, 19 Feb 2006) $
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

using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace SqlBatis.DataMapper.Utilities.Objects
{
	/// <summary>
	/// A factory to create objects 
	/// </summary>
	public class ObjectFactory : IObjectFactory
	{
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ObjectFactory> _logger;
        private IObjectFactory _objectFactory = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="allowCodeGeneration"></param>
        /// <param name="loggerFactory"></param>
        public ObjectFactory(ILoggerFactory loggerFactory, ILogger<ObjectFactory> logger)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            // Detect runtime environment and create the appropriate factory
            if (Environment.Version.Major >= 2)
            {
                _objectFactory = new DelegateObjectFactory(_loggerFactory.CreateLogger<DelegateObjectFactory>());
            }
            else
            {
                _objectFactory = new EmitObjectFactory(_loggerFactory);
            }
        }

        #region IObjectFactory members

		/// <summary>
		/// Create a new factory instance for a given type
		/// </summary>
		/// <param name="typeToCreate">The type instance to build</param>
		/// <param name="types">The types of the constructor arguments</param>
		/// <returns>Returns a new instance factory</returns>
		public IFactory CreateFactory(Type typeToCreate, Type[] types)
		{
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                return new FactoryLogAdapter(typeToCreate, types, _objectFactory.CreateFactory(typeToCreate, types), _logger);
            }
		    else
            {
                return _objectFactory.CreateFactory(typeToCreate, types);
            }
		}

		#endregion
	}
}
