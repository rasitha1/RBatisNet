#region Apache Notice

/*****************************************************************************
 * $Revision: 408164 $
 * $LastChangedDate: 2006-11-14 19:33:12 +0100 (mar., 14 nov. 2006) $
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
using System.Data;
using IBatisNet.DataMapper.Configuration.ResultMapping;

#endregion


namespace IBatisNet.DataMapper.TypeHandlers
{
    /// <summary>
    ///     SByteTypeHandler.
    /// </summary>
    public sealed class SByteTypeHandler : BaseTypeHandler
    {
        /// <summary>
        ///     Gets a value indicating whether this instance is simple type.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is simple type; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSimpleType => true;

        /// <summary>
        ///     The null value for this type
        /// </summary>
        /// <value></value>
        public override object NullValue =>
            throw new InvalidCastException("SByteTypeHandler, could not cast a null value in sbyte field.");

        /// <summary>
        ///     Gets a column value by the name
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public override object GetValueByName(ResultProperty mapping, IDataReader dataReader)
        {
            int index = dataReader.GetOrdinal(mapping.ColumnName);

            if (dataReader.IsDBNull(index))
                return DBNull.Value;
            return Convert.ToSByte(dataReader.GetValue(index));
        }

        /// <summary>
        ///     Gets a column value by the index
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public override object GetValueByIndex(ResultProperty mapping, IDataReader dataReader)
        {
            if (dataReader.IsDBNull(mapping.ColumnIndex))
                return DBNull.Value;
            return Convert.ToSByte(dataReader.GetValue(mapping.ColumnIndex));
        }

        /// <summary>
        ///     Converts the String to the type that this handler deals with
        /// </summary>
        /// <param name="type">the tyepe of the property (used only for enum conversion)</param>
        /// <param name="s">the String value</param>
        /// <returns>the converted value</returns>
        public override object ValueOf(Type type, string s)
        {
            return Convert.ToSByte(s);
        }

        /// <summary>
        ///     Retrieve ouput database value of an output parameter
        /// </summary>
        /// <param name="outputValue">ouput database value</param>
        /// <param name="parameterType">type used in EnumTypeHandler</param>
        /// <returns></returns>
        public override object GetDataBaseValue(object outputValue, Type parameterType)
        {
            return Convert.ToSByte(outputValue);
        }
    }
}