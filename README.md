# DbConnect.NET

DbConnect is a tiny, performant abstraction encapsulating ADO.NET. The intention of the project, is to make accessing a SQL Server database much simpler, and less verbose (i.e. cinch-ier). Referencing .NET Standard 1.4 means DbConnect supports both **.NET Core** and **.NET Framework**. This project was heavily if not entirely inspired by my mentor @joelvarty.

DbConnect leverages Marc Gravell's amazing FastMember for object construction, via a helper method which converts `SqlDataReader` to any instantiable object. `DataReader`'s are used throughout the stack to reduce the overall memory footprint as much as humanly possible.

## Getting Started
By default DbConnect assumes you're calling Stored Procedures (because why wouldn't you? am I right?!). But this is entirely at execution time.

### Execute Command (no results)

```c#
using(var db = new DbConnect("your connection string")){
    db.Execute("dbo.someSproc");
}
```

### Execute Query

```c#
using(var db = new DbConnect("your connection string")){
    var someObjects = db.Execute<SomeObject>("dbo.someSproc");
}
```

### Execute Reader

This approach is useful for dealing with multiple result sets, or you need to work with an unbuffered data reader (usually only needed when the record count or data volume is high). 

Note that `Read<T>()` is optional, you are entirely free to use and manipulate the reader as needed.

```c#
using(var db = new DbConnect("your connection string")){
    using(var rd = db.Reader("dbo.someSproc")){
        var someObjects = rd.Read<SomeObject>();
        var someOtherObjects = rd.Read<SomeOtherObject>();
    }
}
```

### Bulk Insert

Useful for copy large amounts of data from one database to another, or within the same database. 

Note that this is a dual connection operation.

```c#
using(var db = new DbConnect("your connection string"))
using(var bulkDb = new DbConnect("another connection string"))
using(var rd = db.Reader("dbo.someSproc")) {
    bulkDb.Bulk<SomeObject>(rd, "dbo.SomeTable");
}
```

## Configuration

### Setting Command Timeout

By default `SqlCommand` uses a 30 second timeout. If you need to change this for any reason, you can do so as follows:

```c#
...
db.Execute("dbo.someSproc", commandTimeout: 60);
...

...
var someObjects = db.Execute<SomeObject>("dbo.someSproc", commandTimeout: 60);
...

...
var rd = db.Reader("dbo.someSproc", commandTimeout: 60)
...
```

### Setting Command Type

By default DbConnect assumes you're executing a stored procedure. If you need to change this for any reason, you can do so as follows (available options found [here](https://msdn.microsoft.com/en-us/library/system.data.commandtype(v=vs.110).aspx)):

```c#
...
db.Execute("insert into dbo.SomeTable(someCol) values (1)", commandType: CommandType.Text);
...

...
var someObjects = db.Execute<SomeObject>("select * from dbo.SomeTable", commandType: CommandType.Text);
...

...
var rd = db.Reader("select * from dbo.SomeTable", commandType: CommandType.Text)
...
```

### Bulk Options

The `Bulk<T>()` command exposes the following configuration options:

```c#
//int bulkCopyTimeout (default 30)
...
db.Bulk<SomeObject>(rd, "dbo.SomeTable", bulkCopyTimeout: 60);
...

//int batchSize (default 5000)
...
db.Bulk<SomeObject>(rd, "dbo.SomeTable", batchSize: 500);
...

//SqlBulkCopyOptions sqlBulkCopyOptions (default SqlBulkCopyOptions.Default)
...
db.Bulk<SomeObject>(rd, "dbo.SomeTable", sqlBulkCopyOptions: SqlBulkCopyOptions.KeepNulls);
...

//IEnumerable<string> ignoreCols (columns to skip during mapping)
...
db.Bulk<SomeObject>(rd, "dbo.SomeTable", ignoreCols: new string[] { "SomeProperty" } );
...
```

## Transactions

DbConnect has full support for transaction. 

```c#
using(var db = new DbConnect("your connection string")){
    var trans = db.BeginTransaction();

    try {
        db.Execute("insert into dbo.SomeTable(someCol) values (1)", commandType: CommandType.Text, transaction: trans);    
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

Built with ♥ by [Pim Brouwers](https://github.com/pimbrouwers) in Toronto, ON. Licensed under [MIT](https://github.com/pimbrouwers/HydrogenCSS/blob/master/LICENSE).
