# DbConnect.NET

DbConnect is a tiny, performant abstraction encapsulating the .NET `SqlCommand`. The intention of the project, is to make accessing a SQL Server database much simpler, and less verbose. It supports both **.NET CORE** and **.NET Framework**. This project was heavily if not entirely inspired by my mentor @joelvarty.

DbConnect leverages Marc Gravell's amazing FastMember for object construction, via a helper method which converts `SqlDataReader` to any instantiable object. `DataReader`'s are used throughout the stack to reduce the overall memory footprint as much as humanly possible, and the project is `async` compliant throughout the stack.

## Usage
By default DbConnect assumes you're calling Stored Procedures (because why wouldn't you? am I right?!). But this is entirely configurable at construction.

Basic usage is as follows:

```c#
using(var db = new DbConnect("your connection string", "dbo.SomeProcedure")){
    await db.ExecuteNonQuery();
}
```

And that's it!

### Filling Objects
```c#
using(var db = new DbConnect("your connection string", "dbo.SomeProcedure")){
    return await db.FillObject<SomeObject>();
}
```

### Filling Lists
```c#
using(var db = new DbConnect("your connection string", "dbo.SomeProcedure")){
    return await db.FillList<SomeObject>();
}
```

### Executing Scalar Queries
```c#
using(var db = new DbConnect("your connection string", "dbo.SomeProcedure")){
    return await db.ExecuteScalarCast<int>();
}
```

### Multiple Result Sets
```c#
using(var db = new DbConnect("your connection string", "dbo.SomeProcedure")){
    using(var rd = db.FillSqlDataReader()){
        while (rd.HasRows){
            while (rd.Read()){
                var someObject = rd.ConvertToObject<SomeObject>();
            }
            
            rd.NextResult();
        }
    }
}
```
