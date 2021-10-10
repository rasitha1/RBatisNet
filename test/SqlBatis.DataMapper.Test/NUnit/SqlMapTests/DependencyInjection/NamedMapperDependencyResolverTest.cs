﻿using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SqlBatis.DataMapper.DependencyInjection;

namespace SqlBatis.DataMapper.Test.NUnit.SqlMapTests.DependencyInjection
{
    [TestFixture]
    public class NamedMapperDependencyResolverTest : BaseTest
    {
        private string _fileName = string.Empty;
        private IServiceCollection _services;
        /// <summary>
        /// SetUp
        /// </summary>
        [SetUp]
        public void Init()
        {
            _fileName = "sqlmap" + "_" + Configuration["database"] + "_" + Configuration["providerType"] + ".config";
            _services = new ServiceCollection();

            // default
            _services.AddSqlMapper(options =>
            {
                options.Resource = _fileName;
                options.Parameters = new Dictionary<string, string>
                {
                    { "useStatementNamespaces", "true"},
                    {
                        "collection2Namespace",
                        "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test"
                    },
                    {"nullableInt", "int"}
                };

            });

            // named 1
            _services.AddSqlMapper("A", options =>
            {
                options.Resource = _fileName;
                options.Parameters = new Dictionary<string, string>
                {
                    { "useStatementNamespaces", "true"},
                    {
                        "collection2Namespace",
                        "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test"
                    },
                    {"nullableInt", "int"}
                };

            });

            // named 2
            _services.AddSqlMapper("B", options =>
            {
                options.Resource = _fileName;
                options.Parameters = new Dictionary<string, string>
                {
                    { "useStatementNamespaces", "true"},
                    {
                        "collection2Namespace",
                        "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test"
                    },
                    {"nullableInt", "int"}
                };

            });
        }

        [Test]
        public void DefaultInstanceShouldStillWork()
        {
            _services.AddSingleton<ISomeDao, SomeDao>();

            using var provider = _services.BuildServiceProvider();
            var dao = provider.GetRequiredService<ISomeDao>();
            Assert.That(dao, Is.Not.Null);
        }

        [TestCase("A")]
        [TestCase("B")]
        public void ShouldProvideSingletonBehaviorWithNamedInstances(string name)
        {
            _services.AddSingletonWithNamedMapper<ISomeDao, SomeDao>(name);

            using var provider = _services.BuildServiceProvider();
            var dao1 = provider.GetRequiredService<ISomeDao>();
            var dao2 = provider.GetRequiredService<ISomeDao>();

            Assert.That(dao1, Is.EqualTo(dao2));
        }

        [TestCase("A")]
        [TestCase("B")]
        public void ShouldProvideScopedBehaviorWithNamedInstances(string name)
        {
            _services.AddScopedWithNamedMapper<ISomeDao, SomeDao>(name);

            using var provider = _services.BuildServiceProvider();
            ISomeDao scope1Dao1;
            ISomeDao scope1Dao2;
            ISomeDao scope2Dao1;
            ISomeDao scope2Dao2;
            using (var scope1 = provider.CreateScope())
            {
                scope1Dao1 = scope1.ServiceProvider.GetRequiredService<ISomeDao>();
                scope1Dao2 = scope1.ServiceProvider.GetRequiredService<ISomeDao>();
            }
            using (var scope2 = provider.CreateScope())
            {
                scope2Dao1 = scope2.ServiceProvider.GetRequiredService<ISomeDao>();
                scope2Dao2 = scope2.ServiceProvider.GetRequiredService<ISomeDao>();
            }

            Assert.That(scope1Dao1, Is.EqualTo(scope1Dao2));
            Assert.That(scope2Dao1, Is.EqualTo(scope2Dao2));

            Assert.That(scope1Dao1, Is.Not.EqualTo(scope2Dao1));
            Assert.That(scope1Dao2, Is.Not.EqualTo(scope2Dao2));

        }

        [TestCase("A")]
        [TestCase("B")]
        public void ShouldProvideTransientBehaviorWithNamedInstances(string name)
        {
            _services.AddTransientWithNamedMapper<ISomeDao, SomeDao>(name);

            using var provider = _services.BuildServiceProvider();
            var dao1 = provider.GetRequiredService<ISomeDao>();
            var dao2 = provider.GetRequiredService<ISomeDao>();

            Assert.That(dao1, Is.Not.EqualTo(dao2));

            // but mappers are singletons
            Assert.That(dao1.Mapper, Is.EqualTo(dao2.Mapper));
        }

        [TestCase("A")]
        [TestCase("B")]
        public void CanGetNamedMapperBasedType(string name)
        {
            _services.AddSingletonWithNamedMapper<ISomeDao, SomeDao>(name);

            using var provider = _services.BuildServiceProvider();
            var dao = provider.GetRequiredService<ISomeDao>();

            Assert.That(dao.Mapper, Is.EqualTo(provider.GetRequiredService<ISqlMapperFactory>().GetMapper(name)));
        }

        [TestCase("A")]
        [TestCase("B")]
        public void CanInstantiateTypeWithAdditionalArgs(string name)
        {
            _services.AddSingletonWithNamedMapper<ISomeComplexDao, AdditionalArgDao>(name);
            _services.AddSingleton(Configuration);

            using var provider = _services.BuildServiceProvider();

            var dao = provider.GetRequiredService<ISomeComplexDao>();

            Assert.That(dao.Configuration, Is.EqualTo(Configuration));
        }

        internal interface ISomeDao
        {
            ISqlMapper Mapper { get; }
        }
        internal interface ISomeComplexDao
        {
            ISqlMapper Mapper { get; }
            IConfiguration Configuration { get; }
        }
        internal class SomeDao : ISomeDao
        {
            public SomeDao(ISqlMapper mapper)
            {
                Mapper = mapper;
            }
            public ISqlMapper Mapper { get; }
        }
        internal class AdditionalArgDao : ISomeComplexDao
        {
            public AdditionalArgDao(ISqlMapper mapper, IConfiguration configuration)
            {
                Mapper = mapper;
                Configuration = configuration;
            }
            public ISqlMapper Mapper { get; }
            public IConfiguration Configuration { get; }
        }
    }
}