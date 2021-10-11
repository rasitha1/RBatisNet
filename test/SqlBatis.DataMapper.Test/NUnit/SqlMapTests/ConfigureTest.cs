using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using SqlBatis.DataMapper.Utilities;
using SqlBatis.DataMapper; // SqlMap API
using SqlBatis.DataMapper.Configuration;
using SqlBatis.DataMapper.SessionStore;
using SqlBatis.DataMapper.Test.Domain;
using NUnit.Framework;
using System.Collections.Specialized;
using Microsoft.Extensions.Logging.Abstractions;
using SqlBatis.DataMapper.Commands;
using SqlBatis.DataMapper.Configuration.ParameterMapping;
using SqlBatis.DataMapper.Configuration.Serializers;
using SqlBatis.DataMapper.MappedStatements.ResultStrategy;
using SqlBatis.DataMapper.Scope;

namespace SqlBatis.DataMapper.Test.NUnit.SqlMapTests
{
	/// <summary>
	/// Description résumée de ConfigureTest.
	/// </summary>
	[TestFixture] 
	public class ConfigureTest : BaseTest 
	{
		private string _fileName = string.Empty;
        private DomSqlMapBuilder builder;
		#region SetUp

		/// <summary>
		/// SetUp
		/// </summary>
		[SetUp] 
		public void Init() 
		{
            _fileName = "sqlmap" + "_" + Configuration["database"] + "_" + Configuration["providerType"] + ".config";

            var scope = new ConfigurationScope();
            var resultsStrategy = new ResultClassStrategy(NullLogger<ResultClassStrategy>.Instance);

            builder = new DomSqlMapBuilder(NullLogger<DomSqlMapBuilder>.Instance, NullLoggerFactory.Instance,
                scope, new InlineParameterMapParser(), new PreparedCommandFactory(NullLoggerFactory.Instance), new ResultStrategyFactory(resultsStrategy));


		}
		#endregion

		/// <summary>
		/// Test AsyncLocalSessionStore
		/// </summary>
		[Test]
        public void AsyncLocalSessionStoreTest()
	    {
            sqlMap.SessionStore = new AsyncLocalSessionStore(sqlMap.Id);
	        
            Account account = sqlMap.QueryForObject("SelectWithProperty", null) as Account;
            AssertAccount1(account);
	    }
	    

		#region Relatives Path tests

		/// <summary>
		/// Test Configure via relative path
		/// </summary>
		[Test] 
		public void TestConfigureRelativePath()
		{

			NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(_fileName);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test ConfigureAndWatch via relative path
		/// </summary>
		[Test] 
		public void TestConfigureAndWatchRelativePath()
		{

            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(_fileName);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test Configure via relative path
		/// </summary>
		[Test] 
		public void TestConfigureRelativePathViaBuilder()
		{
            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(_fileName);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test ConfigureAndWatch via relative path
		/// </summary>
		[Test] 
		public void TestConfigureAndWatchRelativePathViaBuilder()
		{
            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(_fileName);

			Assert.IsNotNull(mapper);
		}
		#endregion 

		#region Absolute Paths

		/// <summary>
		/// Test Configure via absolute path
		/// </summary>
		[Test] 
		public void TestConfigureAbsolutePath()
		{
			_fileName = Resources.BaseDirectory+Path.DirectorySeparatorChar+_fileName;

            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(_fileName);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test Configure via absolute path
		/// </summary>
		[Test] 
		public void TestConfigureAbsolutePathViaBuilder()
		{
			_fileName = Resources.BaseDirectory+Path.DirectorySeparatorChar+_fileName;

            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(_fileName);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test Configure via absolute path with file suffix
		/// </summary>
		[Test] 
		public void TestConfigureAbsolutePathWithFileSuffix()
		{
			_fileName = "file://"+Resources.BaseDirectory+Path.DirectorySeparatorChar+_fileName;

            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(_fileName);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test Configure via absolute path with file suffix
		/// </summary>
		[Test] 
		public void TestConfigureAbsolutePathWithFileSuffixViaBuilder()
		{
			_fileName = "file://"+Resources.BaseDirectory+Path.DirectorySeparatorChar+_fileName;

            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(_fileName);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test Configure via absolute path via FileIfno
		/// </summary>
		[Test] 
		public void TestConfigureAbsolutePathViaFileInfo()
		{
			_fileName = Resources.BaseDirectory+Path.DirectorySeparatorChar+_fileName;
			FileInfo fileInfo = new FileInfo(_fileName);

            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(fileInfo);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test Configure via absolute path via Uri
		/// </summary>
		[Test] 
		public void TestConfigureAbsolutePathViaUri()
		{
			_fileName = Resources.BaseDirectory+Path.DirectorySeparatorChar+_fileName;
			Uri uri = new Uri(_fileName);

            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(uri);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test Configure via absolute path
		/// </summary>
		[Test] 
		public void TestConfigureAndWatchAbsolutePath()
		{
			_fileName = Resources.BaseDirectory+Path.DirectorySeparatorChar+_fileName;
            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(_fileName);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test Configure via absolute path
		/// </summary>
		[Test] 
		public void TestConfigureAndWatchAbsolutePathViaBuilder()
		{
			_fileName = Resources.BaseDirectory+Path.DirectorySeparatorChar+_fileName;

            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(_fileName);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test Configure via absolute path 
		/// </summary>
		[Test] 
		public void TestConfigureAndWatchAbsolutePathWithFileSuffix()
		{
			_fileName = "file://"+Resources.BaseDirectory+Path.DirectorySeparatorChar+_fileName;
            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(_fileName);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test Configure via absolute path 
		/// </summary>
		[Test] 
		public void TestConfigureAndWatchAbsolutePathWithFileSuffixViaBuilder()
		{
			_fileName = "file://"+Resources.BaseDirectory+Path.DirectorySeparatorChar+_fileName;

            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(_fileName);

			Assert.IsNotNull(mapper);
		}

		/// <summary>
		/// Test Configure via absolute path via FileInfo
		/// </summary>
		[Test] 
		public void TestConfigureAndWatchAbsolutePathViaFileInfo()
		{
			_fileName = Resources.BaseDirectory+Path.DirectorySeparatorChar+_fileName;
			FileInfo fileInfo = new FileInfo(_fileName);

            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(fileInfo);

			Assert.IsNotNull(mapper);
		}
		#endregion 

		#region Stream / Embedded 

		/// <summary>
		/// Test Configure via Stream/embedded
		/// </summary>
		[Test] 
		public void TestConfigureViaStream()
		{
			// embeddedResource = "bin.Debug.SqlMap_MSSQL_SqlClient.config, SqlBatis.DataMapper.Test";
			
            Assembly assembly = Assembly.Load("SqlBatis.DataMapper.Test");
			Stream stream = assembly.GetManifestResourceStream("SqlBatis.DataMapper.Test.SqlMap_MSSQL_SqlClient.config");

            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int");
		    ChildSetupProperties(properties);
            builder.Properties = properties;

            ISqlMapper mapper = builder.Configure(stream);

			Assert.IsNotNull(mapper);
		}
		#endregion 

	}
}
