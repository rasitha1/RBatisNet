using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SqlBatis.DataMapper
{
    internal class DataTableBuilder<T> : IDataTableBuilder<T>
    {
        private readonly List<BuilderPropertyInfo> _properties = new List<BuilderPropertyInfo>();

        private class BuilderPropertyInfo
        {
            public PropertyInfo Property { get; set; }
            public Func<T, object> GetMethod { get; set; }
        }

        private static Func<T, object> CreateGetter(MemberInfo memberInfo)
        {
            var parameter = Expression.Parameter(typeof(T), "source");
            var memberAccess = Expression.MakeMemberAccess(parameter, memberInfo);
            var convert = Expression.Convert(memberAccess, typeof(object));
            var lambda = Expression.Lambda<Func<T, object>>(convert, parameter);
            return lambda.Compile();
        }
        public DataTable Build(IEnumerable<T> items)
        {
            var table = new DataTable();
            foreach (var prop in _properties)
            {
                table.Columns.Add(prop.Property.Name, prop.Property.PropertyType);
            }

            foreach (var item in items)
            {
                table.Rows.Add(_properties.Select(i => i.GetMethod(item)).ToArray());
            }

            return table;
        }

        public IDataTableBuilder<T> Add<TProp>(Expression<Func<T, TProp>> expression)
        {
            var prop = (PropertyInfo)((MemberExpression)expression.Body).Member;
            _properties.Add(new BuilderPropertyInfo
            {
                Property = prop,
                GetMethod = CreateGetter(prop)
            });
            return this;
        }
    }
}