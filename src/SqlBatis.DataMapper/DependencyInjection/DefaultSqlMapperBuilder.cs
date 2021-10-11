using Microsoft.Extensions.DependencyInjection;

namespace SqlBatis.DataMapper.DependencyInjection
{
    internal class DefaultSqlMapperBuilder : ISqlMapperBuilder
    {
        internal DefaultSqlMapperBuilder(IServiceCollection services, string name)
        {
            Services = services;
            Name = name;
        }
        public IServiceCollection Services { get; }
        public string Name { get; }
    }
}