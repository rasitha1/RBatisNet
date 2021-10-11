using SqlBatis.DataMapper.Exceptions;
using SqlBatis.DataMapper.Test.Domain;
using SqlBatis.DataMapper.Utilities.Objects;
using NUnit.Framework;
using System;
using Microsoft.Extensions.Logging.Abstractions;


namespace SqlBatis.DataMapper.Test.NUnit.CommonTests.Utilities
{

    [TestFixture] 
	public class ObjectFactoryTest
    {
        private IObjectFactory objectFactory;
		[SetUp]
        public void Setup()
        {
            objectFactory = new ObjectFactory(NullLoggerFactory.Instance, NullLogger<ObjectFactory>.Instance);
		}
		[Test]
        //[ExpectedException(typeof(ProbeException))]
		public void AbstractConstructor()
		{
			IFactory factory = objectFactory.CreateFactory(typeof (AbstractDocument), Type.EmptyTypes );

		    Assert.That(() => factory.CreateInstance(null), Throws.TypeOf<ProbeException>());
        }
    	
		[Test]
		public void DevivedClassConstructor()
		{
			IFactory factory = objectFactory.CreateFactory(typeof (Book), Type.EmptyTypes );

			Assert.IsNotNull(factory);
		}
    	
		[Test]
		//[ExpectedException(typeof(ProbeException))]
		public void PrivateConstructor()
		{
			Assert.That(() => objectFactory.CreateFactory(typeof(PrivateOrder), Type.EmptyTypes), Throws.TypeOf<ProbeException>());
		}

        [Test]
        //[ExpectedException(typeof(ProbeException))]
        public void NoMatchConstructor()
        {
            Assert.That(() => objectFactory.CreateFactory(typeof(ItemBis), Type.EmptyTypes), Throws.TypeOf<ProbeException>());
        }

		[Test]
		//[ExpectedException(typeof(ProbeException))]
		public void ProtectedConstructor()
		{
		    Assert.That(() => objectFactory.CreateFactory(typeof(ProtectedItem), Type.EmptyTypes), Throws.TypeOf<ProbeException>());

		}

		[Test]
		public void ClassWithMultipleConstructor()
		{
			Type[] types = {typeof(string)};
			IFactory factory0 = objectFactory.CreateFactory(typeof (Account), types );

			object[] parameters = {"gilles"};
			object obj0 = factory0.CreateInstance(parameters);

			Assert.IsTrue(obj0 is Account);
			Account account = (Account)obj0;
			Assert.AreEqual("gilles", account.Test);

			IFactory factory1 = objectFactory.CreateFactory(typeof (Account), Type.EmptyTypes );

			object obj1 = factory1.CreateInstance(parameters);

			Assert.IsTrue(obj1 is Account);
		}

		[Test]
		public void StringConstructor()
		{
			Type[] types = {typeof(string)};
			IFactory factory = objectFactory.CreateFactory(typeof (Account), types );

			object[] parameters = {"gilles"};
			object obj = factory.CreateInstance(parameters);

			Assert.IsTrue(obj is Account);
			Account account = (Account)obj;
			Assert.AreEqual("gilles", account.Test);
		}

		[Test]
		public void MultipleParamConstructor1()
		{
			Type[] types = {typeof(string)};
			IFactory factory = objectFactory.CreateFactory(typeof (Account), types );

			object[] parameters = new object[1];
			parameters[0] = null;
			object obj = factory.CreateInstance(parameters);

			Assert.IsTrue(obj is Account);
			Account account = (Account)obj;
			Assert.AreEqual(null, account.Test);
		}

		[Test]
		public void IntConstructor()
		{
			Type[] types = {typeof(int)};
			IFactory factory = objectFactory.CreateFactory(typeof (Account), types );

			object[] parameters = new object[1];
			parameters[0] = -55;
			object obj = factory.CreateInstance(parameters);

			Assert.IsTrue(obj is Account);
			Account account = (Account)obj;
			Assert.AreEqual( -55, account.Id);
		}

		[Test]
		public void EnumConstructorEnum()
		{
			Type[] types = {typeof(Days)};
			IFactory factory = objectFactory.CreateFactory(typeof (Account), types );

			object[] parameters = new object[1];
			parameters[0] = Days.Sun;
			object obj = factory.CreateInstance(parameters);

			Assert.IsTrue(obj is Account);
			Account account = (Account)obj;
			Assert.AreEqual( Days.Sun, account.Days);
		}

		[Test]
		public void ClassConstructor()
		{
			Type[] types = {typeof(Property)};
			IFactory factory = objectFactory.CreateFactory(typeof (Account), types );

			object[] parameters = new object[1];
			Property prop = new Property();
			prop.String = "Gilles";
			parameters[0] = prop;
			object obj = factory.CreateInstance(parameters);

			Assert.IsTrue(obj is Account);
			Account account = (Account)obj;
			Assert.IsNotNull(account.Property);
			Assert.AreEqual( "Gilles", account.Property.String);
		}

		[Test]
		public void DateTimeConstructor()
		{
			Type[] types = {typeof(DateTime)};
			IFactory factory = objectFactory.CreateFactory(typeof (Account), types );

			object[] parameters = new object[1];
			DateTime date = DateTime.Now;
			parameters[0] = date;
			object obj = factory.CreateInstance(parameters);

			Assert.IsTrue(obj is Account);
			Account account = (Account)obj;
			Assert.AreEqual( date, account.Date);
		}

        [Test]
        public void ArrayParamConstructor()
        {
            Type[] types = { typeof(int[]) };
            IFactory factory = objectFactory.CreateFactory(typeof(Account), types);

            object[] parameters = new object[1];

            int[] ids = new int[2];
            ids[0] = 1;
            ids[1] = 2;

            parameters[0] = ids;
            object obj = factory.CreateInstance(parameters);

            Assert.IsTrue(obj is Account);
            Account account = (Account)obj;

            Assert.AreEqual(2, account.Ids.Length);
            Assert.AreEqual(1, account.Ids[0]);
            Assert.AreEqual(2, account.Ids[1]);
        }

		[Test]
		public void MultipleParamConstructor0()
		{
			Type[] types = {typeof(string), typeof(Property)};
			IFactory factory = objectFactory.CreateFactory(typeof (Account), types );

			object[] parameters = new object[2];
			Property prop = new Property();
			prop.String = "Gilles";
			parameters[0] = "Héloïse";
			parameters[1] = prop;
			object obj = factory.CreateInstance(parameters);

			Assert.IsTrue(obj is Account);
			Account account = (Account)obj;
			Assert.AreEqual("Héloïse", account.FirstName);
			Assert.IsNotNull(account.Property);
			Assert.AreEqual( "Gilles", account.Property.String);
		}



		[Test]
		public void DynamicFactoryCreatesTypes()
		{
			IFactory factory = objectFactory.CreateFactory(typeof (Account), Type.EmptyTypes);
			object obj = factory.CreateInstance(null);
			Assert.IsTrue(obj is Account);

			factory = objectFactory.CreateFactory(typeof (Account), Type.EmptyTypes);
			obj = factory.CreateInstance(Type.EmptyTypes);
			Assert.IsTrue(obj is Account);

			factory = objectFactory.CreateFactory(typeof (Simple), Type.EmptyTypes);
			obj = factory.CreateInstance(Type.EmptyTypes);
			Assert.IsTrue(obj is Simple);
		}

		[Test]
		public void CreateInstanceWithDifferentFactories()
		{
			const int TEST_ITERATIONS = 1000000;
			IFactory factory = null;

			#region new
			factory = new NewAccountFactory();

			// create an instance so that Activators can
			// cache the type/constructor/whatever
			factory.CreateInstance(Type.EmptyTypes);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Timer timer = new Timer();
			timer.Start();
			for (int i = 0; i < TEST_ITERATIONS; i++)
			{
				factory.CreateInstance(Type.EmptyTypes);
			}
			timer.Stop();
			double newFactoryResult = 1000000 * (timer.Duration / (double)TEST_ITERATIONS);
			#endregion

            #region Emit
			factory = new EmitObjectFactory(NullLoggerFactory.Instance).CreateFactory(typeof(Account), Type.EmptyTypes);

			// create an instance so that Activators can
			// cache the type/constructor/whatever
			factory.CreateInstance(Type.EmptyTypes);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			timer.Start();
			for (int i = 0; i < TEST_ITERATIONS; i++)
			{
				factory.CreateInstance(Type.EmptyTypes);
			}
			timer.Stop();
			double emitFactoryResult = 1000000 * (timer.Duration / (double)TEST_ITERATIONS);
			#endregion

			// Print results
			Console.WriteLine(
				"Create " + TEST_ITERATIONS.ToString() + " objects via factory :"
				+ "\nNew : \t\t\t" + newFactoryResult.ToString("F3")
				+ "\nEmit IL : \t\t\t" + emitFactoryResult.ToString("F3") + " Ratio : " + ((emitFactoryResult / newFactoryResult)).ToString("F3"));
		}

		internal class NewAccountFactory : IFactory
		{
			public object CreateInstance(object[] parameters)
			{
				return new Account();
			}

		}

	}
}
