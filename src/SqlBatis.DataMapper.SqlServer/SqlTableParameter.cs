using System.Data;

namespace SqlBatis.DataMapper.SqlServer
{
    /// <summary>
    /// Specialized parameter for SQL Server Structured Data Type
    /// </summary>
    public class SqlTableParameter
    {
        /// <summary>
        /// Data table
        /// </summary>
        public DataTable Data { get; set; }

        /// <summary>
        /// Table type name
        /// </summary>
        public string TypeName { get; set; }
    }
}