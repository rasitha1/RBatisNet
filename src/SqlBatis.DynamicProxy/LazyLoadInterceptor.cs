﻿#region Apache Notice

/*****************************************************************************
 * $Revision: 374175 $
 * $LastChangedDate: 2006-04-25 19:40:27 +0200 (mar., 25 avr. 2006) $
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

using System;
using System.Collections;
using System.Reflection;
using Castle.DynamicProxy;
using SqlBatis.DataMapper.Utilities.Objects.Members;
using SqlBatis.DataMapper;
using SqlBatis.DataMapper.MappedStatements;

#endregion

namespace IBatisNet.DynamicProxy
{
    /// <summary>
    ///     Default implementation of the interceptor reponsible of load the lazy element
    ///     Could load collections and single objects
    /// </summary>
    [Serializable]
    internal class LazyLoadInterceptor : IInterceptor
    {
        #region IInterceptor member

        /// <summary>
        ///     Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <param name="arguments">The target arguments.</param>
        /// <returns></returns>
        public void Intercept(IInvocation invocation)
        {
            //if (_logger.IsDebugEnabled) _logger.Debug("Proxyfying call to " + invocation.Method.Name);

            lock (_loadLock)
            {
                if ((_loaded == false) && (!_passthroughMethods.Contains(invocation.Method.Name)))
                {
                    //if (_logger.IsDebugEnabled) _logger.Debug("Proxyfying call, query statement " + _statementName);

                    //Perform load
                    if (typeof(IList).IsAssignableFrom(_setAccessor.MemberType))
                        _lazyLoadedItem = _sqlMap.QueryForList(_statementName, _param);
                    else
                        _lazyLoadedItem = _sqlMap.QueryForObject(_statementName, _param);

                    _loaded = true;
                    _setAccessor.Set(_target, _lazyLoadedItem);
                }
            }

            object returnValue = invocation.Method.Invoke(_lazyLoadedItem, invocation.Arguments);

            //if (_logger.IsDebugEnabled) _logger.Debug("End of proxyfied call to " + invocation.Method.Name);

            invocation.ReturnValue = returnValue;
        }

        #endregion

        #region Fields

        private readonly object _param;
        private readonly object _target;
        private readonly ISetAccessor _setAccessor;
        private readonly ISqlMapper _sqlMap;
        private readonly string _statementName = string.Empty;
        private bool _loaded;

        private object _lazyLoadedItem;

        //private IList _innerList = null;
        private readonly object _loadLock = new object();
        private static readonly ArrayList _passthroughMethods = new ArrayList();

        #endregion

        #region  Constructor (s) / Destructor

        /// <summary>
        ///     Static Constructor for a lazy list loader
        /// </summary>
        static LazyLoadInterceptor()
        {
            _passthroughMethods.Add("GetType");
        }

        /// <summary>
        ///     Constructor for a lazy list loader
        /// </summary>
        /// <param name="mappedSatement">The mapped statement used to build the list</param>
        /// <param name="param">The parameter object used to build the list</param>
        /// <param name="setAccessor">The proxified member accessor.</param>
        /// <param name="target">The target object which contains the property proxydied.</param>
        internal LazyLoadInterceptor(IMappedStatement mappedSatement, object param,
            object target, ISetAccessor setAccessor)
        {
            _param = param;
            _statementName = mappedSatement.Id;
            _sqlMap = mappedSatement.SqlMap;
            _target = target;
            _setAccessor = setAccessor;
        }

        #endregion
    }
}