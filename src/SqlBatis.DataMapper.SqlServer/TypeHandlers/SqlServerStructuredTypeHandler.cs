using System.Data;
using System.Data.SqlClient;
using SqlBatis.DataMapper.TypeHandlers;

namespace SqlBatis.DataMapper.SqlServer.TypeHandlers
{
    /// <summary>
    /// Type handler that can be used with a <see cref="SqlTableParameter"/>
    /// </summary>
    public class SqlServerStructuredTypeHandler : ITypeHandlerCallback
    {

        /// <summary>
        /// Return the same value from the current getter.
        /// </summary>
        /// <param name="getter"></param>
        /// <returns></returns>
        public object GetResult(IResultGetter getter)
        {
            return getter.Value;
        }

        /// <summary>
        /// Return the nullable equivalent as null.
        /// </summary>
        public object NullValue
        {
            get { return null; }
        }

        /// <summary>
        /// Set the SqlDbType to Structured and set the setter's value to the parameter value.
        /// </summary>
        /// <param name="setter"></param>
        /// <param name="parameter"></param>
        public void SetParameter(IParameterSetter setter, object parameter)
        {
            var structuredParameter = (SqlTableParameter)parameter;
            var sqlParameter = (SqlParameter)setter.DataParameter;
            sqlParameter.SqlDbType = SqlDbType.Structured;
            sqlParameter.TypeName = structuredParameter.TypeName;
            setter.Value = structuredParameter.Data;
        }

        /// <summary>
        /// Return the parsed value of this type from a string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public object ValueOf(string s)
        {
            return s;
        }
    }
}