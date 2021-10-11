using System.Collections.Generic;
using SqlBatis.DataMapper.Configuration.ParameterMapping;
using SqlBatis.DataMapper.Configuration.ResultMapping;
using SqlBatis.DataMapper.Utilities.Objects;

namespace SqlBatis.DataMapper.DataExchange
{
    /// <summary>
    /// DataExchange implementation for IDictionary{string,object} objects
    /// </summary>
    public sealed class GenericDictionaryDataExchange : BaseDataExchange
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataExchangeFactory"></param>
        public GenericDictionaryDataExchange(DataExchangeFactory dataExchangeFactory) : base(dataExchangeFactory)
        {
        }

        #region IDataExchange Members

        /// <summary>
        /// Gets the data to be set into a IDataParameter.
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="parameterObject"></param>
        public override object GetData(ParameterProperty mapping, object parameterObject)
        {
            return ObjectProbe.GetMemberValue(parameterObject, mapping.PropertyName,
                DataExchangeFactory.AccessorFactory);
        }

        /// <summary>
        /// Sets the value to the result property.
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="target"></param>
        /// <param name="dataBaseValue"></param>
        public override void SetData(ref object target, ResultProperty mapping, object dataBaseValue)
        {
            ((IDictionary<string,object>)target).Add(mapping.PropertyName, dataBaseValue);
        }

        /// <summary>
        /// Sets the value to the parameter property.
        /// </summary>
        /// <remarks>Use to set value on output parameter</remarks>
        /// <param name="mapping"></param>
        /// <param name="target"></param>
        /// <param name="dataBaseValue"></param>
        public override void SetData(ref object target, ParameterProperty mapping, object dataBaseValue)
        {
            ObjectProbe.SetMemberValue(target, mapping.PropertyName, dataBaseValue,
                DataExchangeFactory.ObjectFactory,
                DataExchangeFactory.AccessorFactory);
        }

        #endregion
    }
}