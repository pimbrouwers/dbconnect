# DbConnect.NET
[![Build Status](https://travis-ci.org/pimbrouwers/DbConnect.svg?branch=master)](https://travis-ci.org/pimbrouwers/DbConnect/)

DbConnect is a tiny, performant abstraction encapsulating ADO.NET. The intention of the project, is to make accessing a SQL Server database much simpler, and less verbose (i.e. cinch-ier). 

You can think of DbConnect as useful set of extension methods for `SqlConnection`, `SqlCommand`, `SqlDataReader` and `SqlBulkCopy` with a fluent api for building both `SqlCommands` and `SqlBulkCopy` routines. 

DbConnect is compiled against .NET Standard 1.6, which provides support for both both **.NET Core >= 1.0** and **.NET Framework 4.6.1**. This project was heavily if not entirely inspired by my mentor [@joelvarty](https://github.com/joelvarty).

DbConnect leverages Marc Gravell's amazing FastMember for object construction, via a helper method which converts `SqlDataReader` to any instantiable object. `DataReader`'s are used throughout the stack to reduce the overall memory footprint as much as humanly possible.

## Getting Started
Install using nuget:
`Install-Package Cinch.DbConnect`

By default DbConnect assumes you're calling Stored Procedures (because why wouldn't you? am I right?!). But this is entirely configurable at runtime. At it's core DbConnect offers four execution pathways:
1. Command (no results)
2. Query (single result set, buffers into memory)
3. SqlDataReader (useful for multiple result sets)
4. Bulk Copy

Helper methods for 1-3 are available, and a fluent interface (`SqlCommandBuilder` or `SqlBulkCopyBuilder`) is available for more complex configurations. 
### Helpers
These helper methods expose the:

- `string query`
- `object parameters` (optional)
- `CommandType commandType` (optional)- 

#### Command (no results)

```c#
using(var db = new SqlConnection("your connection string")){
    db.Execute("dbo.someSproc", new { param1 = "yayAParam" }); //stored procedure
    db.Execute("select top 100 from dbo.SomeTable where someCol = @param1", new { param1 = "yayAParam" }, CommandType.Text); //inline
}

//async
using(var db = new SqlConnection("your connection string")){
    await db.ExecuteAsync("dbo.someSproc", new { param1 = "yayAParam" }); //stored procedure
    await db.ExecuteAsync("select top 100 from dbo.SomeTable", commandType: CommandType.Text); //inline
}
```

#### Queries (single result set)

This will buffer the datareader into memory (use with caution on large result sets). Under the hood [FastMember](https://github.com/mgravell/fast-member) is being used for conversion.

```c#
using(var db = new SqlConnection("your connection string")){
    var someObjects = db.Execute<SomeObject>("dbo.someSproc", new { param1 = "yayAParam" });
}

//async
using(var db = new SqlConnection("your connection string")){
    var someObjects = await db.ExecuteAsync<SomeObject>("dbo.someSproc", new { param1 = "yayAParam" });
}
```

#### Reader

This approach is useful for dealing with multiple result sets, or if you need to work with an unbuffered data reader (usually only needed when the record count or data volume is high).

> Note that `Reader(...)` returns an instance of `DbReader` which is simply an object to encapsulate the underlying `SqlCommand` and `SqlDataReader`. It's purpose is to provide a clean interface and handle disposal, allowing for use within a `using() { ... }` statement.

> Note that `Read<T>()` and `ReadAsync<T>()` are optional, you are entirely free to use and manipulate the reader as needed.

```c#
using(var db = new SqlConnection("your connection string")){
    using(var rd = db.Reader("dbo.someSproc", new { param1 = "yayAParam" })){
        var someObjects = rd.Read<SomeObject>();
        var someOtherObjects = rd.Read<SomeOtherObject>();
    }
}

//async
using(var db = new SqlConnection("your connection string")){
    using(var rd = await db.ReaderAsync("dbo.someSproc", new { param1 = "yayAParam" })){
        var someObjects = await rd.ReadAsync<SomeObject>();
        var someOtherObjects = await rd.ReadAsync<SomeOtherObject>();
    }
}
```

### Bulk Insert

Useful for copying large amounts of data from one database to another, or within the same database. 

```c#
IEnumerable<SomeObject> srcData = {your source data};
using(var db = new SqlConnection("your connection string")) {
    db.Bulk<SomeObject>(srcData, "dbo.SomeTable");
}

//async
using(var db = new SqlConnection("your connection string")) {
    await db.BulkAsync<SomeObject>(srcData, "dbo.SomeTable");
}
```

## SqlCommandBuilder

`SqlCommandBuilder` implements the builder pattern, by exposing a fluent api, allowing the creation of configurable `SqlCommand`'s. A fully expressed example:

```c#
var cmd = new SqlCommandBuilder().CreateCommand("dbo.someProc")
                                 .SetCommandType(CommandType.StoredProcedure) //optional
                                 .SetCommandTimeout(30) //optional
                                 .WithParameters(new { param1 = "yayAParam" }) //optional
                                 .WithDbParams(dbParams) //optional (DbParams outlined below)
                                 .UsingTransaction(transaction) //optional
```

`Execute`, `Execute<T>`, and `Reader` (including there `async` counterparts) can all accept `SqlCommandBuilder` as there first method parameter.

```c#
 public void Execute(SqlCommandBuilder cmdBuilder, Action<SqlCommand> afterExecution = null) { ... }
 public async Task ExecuteAsync(SqlCommandBuilder cmdBuilder, Action<SqlCommand> afterExecution = null) { ... }
 
 public IEnumerable<T> Execute<T>(SqlCommandBuilder cmdBuilder) { ... }
 public async Task<IEnumerable<T>> ExecuteAsync<T>(SqlCommandBuilder cmdBuilder) { ... }
 
 public SqlDataReader Reader(SqlCommandBuilder cmdBuilder) { ... }
 public async Task<SqlDataReader> ReaderAsync(SqlCommandBuilder cmdBuilder) { ... }
```

## SqlBulkCopyBuilder

Similar to `SqlCommandBuilder` the purpose of `SqlBulkCopyBuilder` is to expose a fluent api to construct bulk copy operations. A fully expressed example:

```c#
var bcp = new SqlBulkCopyBuilder().CreateBcp("dbo.SomeTable")
                                  .SetBatchSize(5000) //optional
                                  .SetTimeout(30) //optional
                                  .WithOptions(SqlBulkCopyOptions.Default) //optional
                                  .UsingTransaction(transaction) //optional
```

Both `Bulk` and `BulkAsync` accept `SqlBulkCopyBuilder` as there first method parameter.

```c#
public void Bulk<T>(SqlBulkCopyBuilder bcpBuilder, IEnumerable<T> srcData, IEnumerable<string> ignoreCols = null) { ... }
public async Task BulkAsync<T>(SqlBulkCopyBuilder bcpBuilder, IEnumerable<T> srcData, IEnumerable<string> ignoreCols = null) { ... }
```

## DbParams

Most scenarios can probably be covered using an anonymous type and the `parameters` parameter of the `Execute`, `Execute<T>`, and `Reader` methods. For scenarios requiring further control, for example output parameters, the custom collection `DbParams` is provided. 

```c#
var dbParams = new DbParams();

dbParams.Add("param1", "yayAParam");
dbParams.Add("param1, "yayAParam". SqlDbType.NVarChar); //optionally specify SqlDbType

dbParams.AddOutput("outputParam1", SqlDbType.NVarChar); 
dbParams.AddOutput("outputParam1", SqlDbType.NVarChar, 30); //optonally specify length 
```

### Accessing Output Parameters

Accessing output parameters is done in "callback" (`Action<SqlCommand`) fashion. 

> Note that this functionality is only exposed in the `Execute` and `ExecuteAsync` methods.

```c#
using(var db = new SqlConnection("your connection string")){
    string outputParam1 = string.Empty;
    var dbParams = new DbParams();
    dbParams.Add("param1", "yayAParam");
    dbParams.AddOutput("outputParam1", SqlDbType.NVarChar); 
    
    var cmd = new SqlCommandBuilder().CreateCommand("dbo.someSproc")
                                     .WithDbParams(dbParams);
                                     
    db.Execute(cmd, (cmd) => {
        outputParam1 = cmd.GetOutputValue<string>("outputParam1");
    });
}
```

## Transactions

DbConnect executions have full support for transactions via the fluent api and exposes two extension methods to kick them off: `BeginTransaction` and `BeginTransactionAsync`. 

```c#
using(var db = new SqlConnection("your connection string")){
    var trans = db.BeginTransaction();
    
    var cmd = new SqlCommandBuilder().CreateCommand("insert into dbo.SomeTable(someCol) values (1)")
                                     .SetCommandType(CommandType.Text)
                                     .UsingTransaction(trans);
    try {
        db.Execute(cmd);    
    }
    catch (Exception ex)    
    {
        //log exception
        trans.Rollback();
    }
    finally {
        trans.Commit();
    }
}

///async
using(var db = new SqlConnection("your connection string")){
    var trans = await db.BeginTransactionAsync();
    
    var cmd = new SqlCommandBuilder().CreateCommand("insert into dbo.SomeTable(someCol) values (1)")
                                     .SetCommandType(CommandType.Text)
                                     .UsingTransaction(trans);
    try {
        await db.ExecuteAsync(cmd);    
    }
    catch (Exception ex)    
    {
        //log exception
        trans.Rollback();
    }
    finally {
        trans.Commit();
    }
}
```

Built with ♥ by [Pim Brouwers](https://github.com/pimbrouwers) in Toronto, ON. Licensed under [MIT](https://github.com/pimbrouwers/DbConnect/blob/master/LICENSE).
