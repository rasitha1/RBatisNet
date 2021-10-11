using Microsoft.Extensions.DependencyInjection;

namespace SqlBatis.DataMapper.DependencyInjection
{
    /// <summary>
    /// Provides a way to configure a <see cref="ISqlMapper"/> further
    /// </summary>
    public interface ISqlMapperBuilder
    {
        /// <summary>
        /// A reference to <see cref="IServiceCollection"/>
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Name of the <see cref="ISqlMapper"/>
        /// </summary>
        string Name { get; }
    }
}