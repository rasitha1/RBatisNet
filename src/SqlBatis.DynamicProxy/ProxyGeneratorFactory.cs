#region Apache Notice

/*****************************************************************************
 * $Header: $
 * $Revision: 383115 $
 * $Date: 2006-03-04 15:21:51 +0100 (sam., 04 mars 2006) $
 * 
 * iBATIS.NET Data Mapper
 * Copyright (C) 2004-2005 - Apache Foundation
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
using Castle.DynamicProxy;

namespace IBatisNet.DynamicProxy
{
    /// <summary>
    ///     A Factory for getting the ProxyGenerator.
    /// </summary>
    public sealed class ProxyGeneratorFactory
    {
        private static readonly ProxyGenerator _generator = new CachedProxyGenerator();

        private ProxyGeneratorFactory()
        {
            // should not be created.	
        }

        /// <summary></summary>
        public static ProxyGenerator GetProxyGenerator()
        {
            //TODO: make this read from a configuration file!!!  At this point anybody
            // could substitute in their own IProxyGenerator and LazyInitializer.
            return _generator;
        }
    }
}