using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Configuration;
using System.Linq;
using NUnit.Framework;

using SqlBatis.DataMapper.Exceptions;

using SqlBatis.DataMapper.Test;
using SqlBatis.DataMapper.Test.Domain;

namespace SqlBatis.DataMapper.Test.NUnit.SqlMapTests
{
	/// <summary>
	/// Summary description for TransactionTest.
	/// </summary>
	[TestFixture] 
	public class ThreadTest: BaseTest
	{
		
		private static int _numberOfThreads = 10;
		private ManualResetEvent  _startEvent = new ManualResetEvent(false);
		private ManualResetEvent  _stopEvent = new ManualResetEvent(false);

		#region SetUp & TearDown

		/// <summary>
		/// SetUp 
		/// </summary>
		[SetUp] 
		public void Init() 
		{
			InitScript( sqlMap.DataSource, ScriptDirectory + "account-init.sql" );
		}

		/// <summary>
		/// TearDown
		/// </summary>
		[TearDown] 
		public void Dispose()
		{ /* ... */ } 

		#endregion

		#region Thread test

		[Test]
		public void TestCommonUsageMultiThread()
		{
			const int threadCount = 10;
            var errors = new ConcurrentBag<Exception>();

			Thread[] threads = new Thread[threadCount];
			
			for(int i = 0; i < threadCount; i++)
			{
				threads[i] = new Thread(() =>
				{
                    Run(ExecuteMethodUntilSignal, errors);
				});
				threads[i].Start();
			}

			_startEvent.Set();

			Thread.CurrentThread.Join(1 * 2000);

			_stopEvent.Set();

		    if (errors.Any())
		    {
		        throw new AggregateException(errors);
		    }
        }

		public void ExecuteMethodUntilSignal()
		{
			_startEvent.WaitOne(int.MaxValue, false);

			while (!_stopEvent.WaitOne(1, false))
			{
				Assert.IsFalse(sqlMap.IsSessionStarted);

				Console.WriteLine("Begin Thread : " + Thread.CurrentThread.GetHashCode());

				Account account = (Account) sqlMap.QueryForObject("GetAccountViaColumnIndex", 1);
				
				Assert.IsFalse(sqlMap.IsSessionStarted);
				
				Assert.AreEqual(1, account.Id, "account.Id");
				Assert.AreEqual("Joe", account.FirstName, "account.FirstName");
				Assert.AreEqual("Dalton", account.LastName, "account.LastName");

				Console.WriteLine("End Thread : " + Thread.CurrentThread.GetHashCode());
			}
		}

	    private static void Run(Action action, ConcurrentBag<Exception> errors)
	    {
	        try
	        {
	            action();
	        }
	        catch (Exception e)
	        {
                errors.Add(e);
	        }
        }

		/// <summary>
		/// Test BeginTransaction, CommitTransaction
		/// </summary>
		[Test] 
		public void TestThread() 
		{
			Account account = NewAccount6();
            var errors = new ConcurrentBag<Exception>();

			try 
			{
				Thread[] threads = new Thread[_numberOfThreads];

				AccessTest accessTest = new AccessTest();

				for (int i = 0; i < _numberOfThreads; i++) 
				{
					Thread thread = new Thread(() =>
					{
					    Run(accessTest.GetAccount, errors);
					});
					threads[i] = thread;
				}
				for (int i = 0; i < _numberOfThreads; i++) 
				{
					threads[i].Start();
				}
			} 
			finally 
			{
			}

		    if (errors.Any())
		    {
		        throw new AggregateException(errors);
		    }

		}

		#endregion

		/// <summary>
		/// Summary description for AccessTest.
		/// </summary>
		private class AccessTest
		{
		
			/// <summary>
			/// Get an account
			/// </summary>
			public void GetAccount()
			{
				Assert.IsFalse(sqlMap.IsSessionStarted);
				
				Account account = (Account) sqlMap.QueryForObject("GetAccountViaColumnIndex", 1);
				
				Assert.IsFalse(sqlMap.IsSessionStarted);
				
				Assert.AreEqual(1, account.Id, "account.Id");
				Assert.AreEqual("Joe", account.FirstName, "account.FirstName");
				Assert.AreEqual("Dalton", account.LastName, "account.LastName");

			}
		}	
	}


}
