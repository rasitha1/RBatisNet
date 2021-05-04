namespace SqlBatis.DataMapper
{
    internal class DataTableBuilderFactory : IDataTableBuilderFactory
    {
        public IDataTableBuilder<T> CreateBuilder<T>()
        {
            return new DataTableBuilder<T>();
        }
    }
}