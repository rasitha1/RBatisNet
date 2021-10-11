# Releases

## 6.0.0 
* Switch to `Microsoft.Extensions.Logging` and get rid of custom logging implementation
* Add async support
* Support code-based configuration of `SqlMap.config` file
  * Properties
  * Settings
  * Providers
  * Database/Datasource
  * Aliases
  * Type handlers
  * Sql Maps

  ## Async support
  ```csharp

  var customers = await _mapper.QueryForListAsync<Customer>("GetAllCustomers", null);

  ```

  ## Code-based Configuration
  ```csharp

  services.AddSqlMapper(options => {
		//Configuration.GetSection("Database").Bind(options);
	})
	.UseSqlServer()
	.AddEmbeddedSqlMaps(typeof(CustomerDao).Assembly, "SqlMaps.*.xml")
	.AddTypeAlias(typeof(Customer).Assembly, "MyApp.Models.*")
	.AddTypeHandler<string, AnsiStringTypeHandler>()
	.AddTypeHandler<bool, OuiNonBoolTypeHandler>("Varchar");

  ```



## 5.1.0

* Provide a few extension methods to easily register a type that depends on a named `ISqlMapper` instance.

```csharp

// default instance
_services.AddSqlMapper(options => {...});

// named instances
_services.AddSqlMapper("foo", options => {...});
_services.AddSqlMapper("bar", options => {...});

// types that depend on a named instance
_services.AddSingletonWithNamedMapper<ISomeDao, SomeDao>("foo");
_services.AddSingletonWithNamedMapper<ISomeOtherDao, SomeOtherDao>("bar");

// same extensions exist for Transient and Scoped
```


## 5.0.0
* Switched to .NET 5 (no longer supports .net framework)
* Removed Caching (Breaking Change)
* Added `dynamic` support
* Improve build pipelines & code coverage

## 4.0.1
* SourceLink support and symbol packages  #6 (Thanks [JTOne123](https://github.com/JTOne123))

## 4.0.0
* Removed **SqlBatis.DataAccess** project (breaking change)
* Removed **SqlBatis.DataMapper.Logging.Log4Net** project (breaking change)
* Merged **SqlBatis.Common** into **SqlBatis.DataMapper** to be in a single assembly (breaking change)
  - Must manually remove SqlBatis.Common package when upgrading 
* Azure DevOps pipelines
* Removed deprecated method: `QueryForPaginatedList`
* Testing against `net472;netcoreapp2.1;netcoreapp3.0`
* Added NetStandardLogger and NetStandardLoggerAdapter to work with Microsoft.Extensions.Logging
  - `LogManager.Adapter = new NetStandardLoggerAdapter(Provider.GetRequiredService<ILoggerFactory>());`
* Added `SqlBatis.Schemas` NuGet Package to include `providers.xsd`, `sqlmap.xsd` and `sqlmapconfig.xsd` files for VS intellisense


### 3.1.0
* Add support for registering named instances of ISqlMapper and getting them via ISqlMapperFactory

### 3.0.1
* Add support for registering and getting an ISqlMapper through DI


### 3.0.0
* Renamed from iBatisNet to SqlBatis
* Added NOTICE file
* BF: Correctly handle removal of sessions in `AsyncLocalSessionStore` 
* Updating NOTICE file to include original iBatisNet Notice.txt text
* Including LICENSE.txt and NOTICE.txt file in NuGet packages

___
Delist 2.x packages published to nuget.org under IBatisNet.*
Publishing new packages under SqlBatis name
___


### 2.1.0
* Merged changes from [rev 709676](http://archive.apache.org/dist/ibatis/source/ibatis.net/) which picked up a number fixes 
* Including `SqlBatis.DataMapper.Logging.Log4Net` .Net Framework assembly as well. 
* Getting rid of rest of the .net framework version.

### 2.0.0
* Migrated [rev 513437](http://archive.apache.org/dist/ibatis/source/ibatis.net/) if IBatisNet to .Net Standard
* Decoupled Dynamic Proxy by dynamically loading `ILazyFactory` implementation via a new `settings` 
attribute called `lazyFactoryType` and splitting `ProxyGeneratorFactory` and `CachedProxyGenerator` to
a separate project called IBatisNet.DynamicProxy. You only need this if you have any lazy-loaded results. Refactored to use latest version of `Castle.Core` package
* Removed Transactions logic from `System.EnterpriseServices`
* Introduced `AsyncLocalSessionStore` and removed all other `ISessionStore` implementations (DataMapper & DataAccess)
* Removed auto instantiating of `ILoggerFactoryAdapter` using `ConfigurationManager` and defauled to `NoOpLoggerFA`. You must set `LogManager.Adapter` in your startup
* Updating assembly versions and package versions to 2.0 (original .net framework version was 1.6.2)
