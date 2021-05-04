using Microsoft.Extensions.DependencyInjection;

namespace SqlBatis.DataMapper.DependencyInjection
{
    /// <summary>
    /// Extensions methods for adding <see cref="IDataTableBuilder{T}"/> into the DI pipeline
    /// </summary>
    public static class DataTableBuilderServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a <see cref="IDataTableBuilderFactory"/> that can be used for working with <see cref="IDataTableBuilder{T}"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDataTableBuilder(this IServiceCollection services)
        {
            services.AddSingleton<IDataTableBuilderFactory, DataTableBuilderFactory>();
            return services;
        }
    }
}