
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SqlBatis.DataMapper.Configuration;

using NUnit.Framework;

// DataSource definition
using SqlBatis.DataMapper.Utilities; // ScriptRunner definition
// SqlMap API
using SqlBatis.DataMapper.Test.Domain;
using System.Collections.Specialized;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using SqlBatis.DataMapper.Commands;
using SqlBatis.DataMapper.Configuration.ParameterMapping;
using SqlBatis.DataMapper.Configuration.Serializers;
using SqlBatis.DataMapper.MappedStatements.ResultStrategy;
using SqlBatis.DataMapper.Scope;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;


namespace SqlBatis.DataMapper.Test.NUnit.SqlMapTests
{
    public delegate string KeyConvert(string key);

    /// <summary>
    /// Summary description for BaseTest.
    /// </summary>
    [TestFixture]
    public abstract class BaseTest
    {
        /// <summary>
        /// The sqlMap
        /// </summary>
        protected static ISqlMapper sqlMap = null;
        protected static string ScriptDirectory { get; }

        protected static KeyConvert ConvertKey = null;

        static BaseTest()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(DefaultConfigurationStrings);
            Configuration = builder.Build();

            //LogManager.Adapter = new ConsoleOutLoggerFA(new NameValueCollection());

            ScriptDirectory = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(TestContext.CurrentContext.TestDirectory, ".."), ".."), ".."), "Scripts"),
                                  Configuration["database"]) + Path.DirectorySeparatorChar;

        }

        protected void InitSqlMap()
        {
            //DateTime start = DateTime.Now;
            var scope = new ConfigurationScope();
            var resultsStrategy = new ResultClassStrategy(NullLogger<ResultClassStrategy>.Instance);

            DomSqlMapBuilder builder = new DomSqlMapBuilder(NullLogger<DomSqlMapBuilder>.Instance, NullLoggerFactory.Instance,
                scope, new InlineParameterMapParser(), new PreparedCommandFactory(NullLoggerFactory.Instance), new ResultStrategyFactory(resultsStrategy));
            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection2, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int?");
            ChildSetupProperties(properties);
            builder.Properties = properties;

            sqlMap = builder.Configure("sqlmap" + "_" + Configuration["database"] + "_"
                                               + Configuration["providerType"] + ".config");

            //string loadTime = DateTime.Now.Subtract(start).ToString();
            //Console.WriteLine("Loading configuration time :"+loadTime);
        }

        static IReadOnlyDictionary<string, string> DefaultConfigurationStrings { get; } =
            new Dictionary<string, string>()
            {
                ["database"] = "MSSQL",
                ["providerType"] = "SqlClient"
            };

        protected static IConfiguration Configuration { get; }


        protected virtual void ChildSetupProperties(NameValueCollection nvc)
        {
            nvc["useStatementNamespaces"] = "false";
        }
        /// <summary>
        /// Initialize an sqlMap
        /// </summary>
        [OneTimeSetUp]
        protected virtual void SetUpFixture()
        {
            //DateTime start = DateTime.Now;

            var scope = new ConfigurationScope();
            var resultsStrategy = new ResultClassStrategy(NullLogger<ResultClassStrategy>.Instance);

            DomSqlMapBuilder builder = new DomSqlMapBuilder(NullLogger<DomSqlMapBuilder>.Instance, NullLoggerFactory.Instance,
                scope, new InlineParameterMapParser(), new PreparedCommandFactory(NullLoggerFactory.Instance), new ResultStrategyFactory(resultsStrategy));

            NameValueCollection properties = new NameValueCollection();
            properties.Add("collection2Namespace", "SqlBatis.DataMapper.Test.Domain.LineItemCollection2, SqlBatis.DataMapper.Test");
            properties.Add("nullableInt", "int?");
            ChildSetupProperties(properties);
            builder.Properties = properties;

            string fileName = "sqlmap" + "_" + Configuration["database"] + "_" + Configuration["providerType"] + ".config";
            try
            {
                sqlMap = builder.Configure(fileName);
            }
            catch (Exception ex)

            {
                Exception e = ex;
                while (e != null)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace.ToString());
                    e = e.InnerException;

                }
                throw;
            }

            if (sqlMap.DataSource.DbProvider.Name.IndexOf("PostgreSql") >= 0)
            {
                BaseTest.ConvertKey = new KeyConvert(Lower);
            }
            else if (sqlMap.DataSource.DbProvider.Name.IndexOf("oracle") >= 0)
            {
                BaseTest.ConvertKey = new KeyConvert(Upper);
            }
            else
            {
                BaseTest.ConvertKey = new KeyConvert(Normal);
            }

            //			string loadTime = DateTime.Now.Subtract(start).ToString();
            //			Console.WriteLine("Loading configuration time :"+loadTime);
        }

        /// <summary>
        /// Dispose the SqlMap
        /// </summary>
        [OneTimeTearDown]
        protected virtual void TearDownFixture()
        {
            sqlMap = null;
        }

        protected static string Normal(string key)
        {
            return key;
        }

        protected static string Upper(string key)
        {
            return key.ToUpper();
        }

        protected static string Lower(string key)
        {
            return key.ToLower();
        }

        /// <summary>
        /// Configure the SqlMap
        /// </summary>
        /// <remarks>
        /// Must verify ConfigureHandler signature.
        /// </remarks>
        /// <param name="obj">
        /// The reconfigured sqlMap.
        /// </param>
        protected static void Configure(object obj)
        {
            sqlMap = null;//(SqlMapper) obj;
        }

        /// <summary>
        /// Run a sql batch for the datasource.
        /// </summary>
        /// <param name="datasource">The datasource.</param>
        /// <param name="script">The sql batch</param>
        protected static void InitScript(IDataSource datasource, string script)
        {
            InitScript(datasource, script, true);
        }

        /// <summary>
        /// Run a sql batch for the datasource.
        /// </summary>
        /// <param name="datasource">The datasource.</param>
        /// <param name="script">The sql batch</param>
        /// <param name="doParse">parse out the statements in the sql script file.</param>
        protected static void InitScript(IDataSource datasource, string script, bool doParse)
        {
            ScriptRunner runner = new ScriptRunner();

            runner.RunScript(datasource, script, doParse);
        }

        /// <summary>
        /// Create a new account with id = 6
        /// </summary>
        /// <returns>An account</returns>
        protected Account NewAccount6()
        {
            Account account = new Account();
            account.Id = 6;
            account.FirstName = "Calamity";
            account.LastName = "Jane";
            account.EmailAddress = "no_email@provided.com";
            return account;
        }

        /// <summary>
        /// Verify that the input account is equal to the account(id=1).
        /// </summary>
        /// <param name="account">An account object</param>
        protected void AssertGilles(Account account)
        {
            Assert.AreEqual(5, account.Id, "account.Id");
            Assert.AreEqual("Gilles", account.FirstName, "account.FirstName");
            Assert.AreEqual("Bayon", account.LastName, "account.LastName");
            Assert.AreEqual("gilles.bayon@nospam.org", account.EmailAddress, "account.EmailAddress");
        }

        /// <summary>
        /// Verify that the input account is equal to the account(id=1).
        /// </summary>
        /// <param name="account">An account object</param>
        protected void AssertAccount1(Account account)
        {
            Assert.AreEqual(1, account.Id, "account.Id");
            Assert.AreEqual("Joe", account.FirstName, "account.FirstName");
            Assert.AreEqual("Dalton", account.LastName, "account.LastName");
            Assert.AreEqual("Joe.Dalton@somewhere.com", account.EmailAddress, "account.EmailAddress");
        }

        /// <summary>
        /// Verify that the input account is equal to the account(id=1).
        /// </summary>
        /// <param name="account">An account as hashtable</param>
        protected void AssertAccount1AsHashtable(Hashtable account)
        {
            Assert.AreEqual(1, (int)account["Id"], "account.Id");
            Assert.AreEqual("Joe", (string)account["FirstName"], "account.FirstName");
            Assert.AreEqual("Dalton", (string)account["LastName"], "account.LastName");
            Assert.AreEqual("Joe.Dalton@somewhere.com", (string)account["EmailAddress"], "account.EmailAddress");
        }

        /// <summary>
        /// Verify that the input account is equal to the account(id=1).
        /// </summary>
        /// <param name="account">An account as hashtable</param>
        protected void AssertAccount1AsHashtableForResultClass(Hashtable account)
        {
            Assert.AreEqual(1, (int)account[BaseTest.ConvertKey("Id")], "account.Id");
            Assert.AreEqual("Joe", (string)account[BaseTest.ConvertKey("FirstName")], "account.FirstName");
            Assert.AreEqual("Dalton", (string)account[BaseTest.ConvertKey("LastName")], "account.LastName");
            Assert.AreEqual("Joe.Dalton@somewhere.com", (string)account[BaseTest.ConvertKey("EmailAddress")], "account.EmailAddress");
        }

        /// <summary>
        /// Verify that the input account is equal to the account(id=6).
        /// </summary>
        /// <param name="account">An account object</param>
        protected void AssertAccount6(Account account)
        {
            Assert.AreEqual(6, account.Id, "account.Id");
            Assert.AreEqual("Calamity", account.FirstName, "account.FirstName");
            Assert.AreEqual("Jane", account.LastName, "account.LastName");
            Assert.IsNull(account.EmailAddress, "account.EmailAddress");
        }

        /// <summary>
        /// Verify that the input order is equal to the order(id=1).
        /// </summary>
        /// <param name="order">An order object.</param>
        protected void AssertOrder1(Order order)
        {
            DateTime date = new DateTime(2003, 2, 15, 8, 15, 00);

            Assert.AreEqual(1, order.Id, "order.Id");
            Assert.AreEqual(date.ToString(), order.Date.ToString(), "order.Date");
            Assert.AreEqual("VISA", order.CardType, "order.CardType");
            Assert.AreEqual("999999999999", order.CardNumber, "order.CardNumber");
            Assert.AreEqual("05/03", order.CardExpiry, "order.CardExpiry");
            Assert.AreEqual("11 This Street", order.Street, "order.Street");
            Assert.AreEqual("Victoria", order.City, "order.City");
            Assert.AreEqual("BC", order.Province, "order.Id");
            Assert.AreEqual("C4B 4F4", order.PostalCode, "order.PostalCode");
        }

        /// <summary>
        /// Verify that the input order is equal to the order(id=1).
        /// </summary>
        /// <param name="order">An order as hashtable.</param>
        protected void AssertOrder1AsHashtable(Hashtable order)
        {
            DateTime date = new DateTime(2003, 2, 15, 8, 15, 00);

            Assert.AreEqual(1, (int)order["Id"], "order.Id");
            Assert.AreEqual(date.ToString(), ((DateTime)order["Date"]).ToString(), "order.Date");
            Assert.AreEqual("VISA", (string)order["CardType"], "order.CardType");
            Assert.AreEqual("999999999999", (string)order["CardNumber"], "order.CardNumber");
            Assert.AreEqual("05/03", (string)order["CardExpiry"], "order.CardExpiry");
            Assert.AreEqual("11 This Street", (string)order["Street"], "order.Street");
            Assert.AreEqual("Victoria", (string)order["City"], "order.City");
            Assert.AreEqual("BC", (string)order["Province"], "order.Id");
            Assert.AreEqual("C4B 4F4", (string)order["PostalCode"], "order.PostalCode");
        }
    }
}
