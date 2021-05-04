namespace SqlBatis.DataMapper
{
    /// <summary>
    /// A factory that can return a <see cref="IDataTableBuilder{T}"/>
    /// </summary>
    public interface IDataTableBuilderFactory
    {
        /// <summary>
        /// Creates a <see cref="IDataTableBuilder{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDataTableBuilder<T> CreateBuilder<T>();
    }
}