#region Apache Notice

/*****************************************************************************
 * $Header: $
 * $Revision: 398108 $
 * $Date: 2006-04-29 11:39:42 +0200 (sam., 29 avr. 2006) $
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
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using SqlBatis.DataMapper.Exceptions;

#endregion

namespace IBatisNet.DynamicProxy
{
    /// <summary>
    ///     An ProxyGenerator with cache that uses the Castle.DynamicProxy library.
    /// </summary>
    public class CachedProxyGenerator : ProxyGenerator
    {
        // key = mapped type
        // value = proxy type
        private readonly IDictionary _cachedProxyTypes;

        /// <summary>
        ///     Constructor
        /// </summary>
        public CachedProxyGenerator()
        {
            _cachedProxyTypes = new HybridDictionary();
        }

        public override object CreateInterfaceProxyWithTarget(Type interfaceToProxy, Type[] additionalInterfacesToProxy,
            object target,
            ProxyGenerationOptions options, params IInterceptor[] interceptors)
        {
            return base.CreateInterfaceProxyWithTarget(interfaceToProxy, additionalInterfacesToProxy, target, options,
                interceptors);
        }

        public override object CreateClassProxy(Type classToProxy, Type[] additionalInterfacesToProxy,
            ProxyGenerationOptions options,
            object[] constructorArguments, params IInterceptor[] interceptors)
        {
            try
            {
                if (classToProxy == null)
                {
                    throw new ArgumentNullException(nameof(classToProxy));
                }

                if (options == null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                if (!classToProxy.GetTypeInfo().IsClass)
                {
                    throw new ArgumentException("'classToProxy' must be a class", nameof(classToProxy));
                }

                CheckNotGenericTypeDefinition(classToProxy, nameof(classToProxy));
                CheckNotGenericTypeDefinitions(additionalInterfacesToProxy, nameof(additionalInterfacesToProxy));

                Type proxyType;


                lock (_cachedProxyTypes.SyncRoot)
                {
                    proxyType = _cachedProxyTypes[classToProxy] as Type;

                    if (proxyType == null)
                    {
                        proxyType = CreateClassProxyType(classToProxy, additionalInterfacesToProxy, options);
                        _cachedProxyTypes[classToProxy] = proxyType;
                    }
                }

                var proxyArguments = BuildArgumentListForClassProxy(options, interceptors);
                if (constructorArguments != null && constructorArguments.Length != 0)
                {
                    proxyArguments.AddRange(constructorArguments);
                }

                return CreateClassProxyInstance(proxyType, proxyArguments, classToProxy, constructorArguments);

            }
            catch (Exception e)
            {
                throw new IBatisNetException("Castle Proxy Generator failed", e);
            }

        }

        public override object CreateClassProxyWithTarget(Type classToProxy, Type[] additionalInterfacesToProxy,
            object target,
            ProxyGenerationOptions options, object[] constructorArguments, params IInterceptor[] interceptors)
        {
            if (classToProxy == null)
            {
                throw new ArgumentNullException(nameof(classToProxy));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!classToProxy.GetTypeInfo().IsClass)
            {
                throw new ArgumentException("'classToProxy' must be a class", nameof(classToProxy));
            }

            CheckNotGenericTypeDefinition(classToProxy, nameof(classToProxy));
            CheckNotGenericTypeDefinitions(additionalInterfacesToProxy, nameof(additionalInterfacesToProxy));

            Type proxyType;
            var targetType = target.GetType();


            lock (_cachedProxyTypes.SyncRoot)
            {
                proxyType = _cachedProxyTypes[targetType] as Type;

                if (proxyType == null)
                {
                    proxyType = CreateClassProxyTypeWithTarget(classToProxy, additionalInterfacesToProxy, options);
                    _cachedProxyTypes[targetType] = proxyType;
                }
            }

            var proxyArguments = BuildArgumentListForClassProxyWithTarget(target, options, interceptors);
            if (constructorArguments != null && constructorArguments.Length != 0)
            {
                proxyArguments.AddRange(constructorArguments);
            }

            return CreateClassProxyInstance(proxyType, proxyArguments, classToProxy, constructorArguments);
        }

        public override object CreateInterfaceProxyWithTargetInterface(Type interfaceToProxy,
            Type[] additionalInterfacesToProxy, object target,
            ProxyGenerationOptions options, params IInterceptor[] interceptors)
        {
            return base.CreateInterfaceProxyWithTargetInterface(interfaceToProxy, additionalInterfacesToProxy, target,
                options, interceptors);
        }

        public override object CreateInterfaceProxyWithoutTarget(Type interfaceToProxy,
            Type[] additionalInterfacesToProxy,
            ProxyGenerationOptions options, params IInterceptor[] interceptors)
        {
            return base.CreateInterfaceProxyWithoutTarget(interfaceToProxy, additionalInterfacesToProxy, options,
                interceptors);
        }           
    }
}