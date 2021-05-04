using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace SqlBatis.DataMapper
{
    /// <summary>
    /// Defines a builder for generating a <see cref="DataTable"/> to be used in a bulk insert parameter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataTableBuilder<T>
    {
        /// <summary>
        /// Builds a <see cref="DataTable"/> from the given list of <paramref name="items"/> using the columns registered with the builder.
        /// </summary>
        /// <param name="items">A record will be added to <see cref="DataTable"/> for each in <paramref name="items"/></param>
        /// <returns></returns>
        DataTable Build(IEnumerable<T> items);

        /// <summary>
        /// Adds a column using a property of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        IDataTableBuilder<T> Add<TProp>(Expression<Func<T, TProp>> expression);
    }
}