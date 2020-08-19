# Sqlite.Database.Management
A lightweight SQLite specific ORM and management library.
This library is primarily a self-learning project, but is licensed under the [MIT License](https://github.com/JustinWilkinson/Sqlite.Database.Management/blob/master/LICENSE) if you'd like to utilise it.

## Project Structure:
### Sqlite.Database.Management
The main class library containing classes and methods to assist with management of either individual SQLite databases, or collections of databases.
There are also methods to assist with data retrieval and conversion to code objects.

### Sqlite.Database.Management.Test
An xUnit test project with tests for Sqlite.Database.Management.

## Usage:
### Overview:
* Sqlite.Database.Management is a basic ORM that can map code objects to SQLite Database tables.
* It supports both File Databases and In Memory databases (shared and unshared).
* It can be used to create and manage collections of databases with the same schema as well as to perform queries on the data present.
* There are also easy to use extension methods which can perform CRUD operations on certain types.

To use, simply install the Nuget package and add the below using statement.
```C#
using Sqlite.Database.Management
```

### Creating a Database:
#### File Databases:
The below sample shows you how to create a database from your connection string.
```C#
var database = new Database("Data Source=MyDatabase.sqlite;");
database.Tables.Add(new Table("Demo") 
{ 
    Columns = new List<Column>
    {
        new Column("StringProperty"),
        new Column("IntProperty", ColumnType.Integer) { Nullable = false },
        new Column("BoolProperty", ColumnType.Integer) { Nullable = false, CheckExpression = "IN (0, 1)" }
    }
});
database.Create(); // This method creates the database and any specified prior to calling this method.
```
The `database.Delete();` method can be used to delete the database once you are done with it.


#### In Memory:
##### Non-Shared:
* By default SQLite In Memory databases only support a single connection, and the database is deleted as soon as this connection closes.
* Sqlite.Database.Management can manage this for you, by opening the connection, and holding it open until you dispose of the database yourself.
The below sample shows you how to create a database with a table "Demo" from scratch.
```C#
var database = new InMemoryDatabase();
database.Tables.Add(new Table("Demo") 
{ 
    Columns = new List<Column>
    {
        new Column("StringProperty"),
        new Column("IntProperty", ColumnType.Integer) { Nullable = false },
        new Column("BoolProperty", ColumnType.Integer) { Nullable = false, CheckExpression = "IN (0, 1)" }
    }
});
database.Create(); // This method creates the database and any specified prior to calling this method.

// Do some work with your database here.

// Clean up - note that InMemoryDatabases implement IDisposable, so you can also simply use a using statement.
database.Dispose();
```

##### Shared:
To create a shared database which supports multiple connections, simply provide a name for your In Memory database.
```C#
var database = new InMemoryDatabase("Shared"); // Specifying a name here allows for multiple connections to the In Memory Database.
database.Tables.Add(new Table("Demo") 
{ 
    Columns = new List<Column>
    {
        new Column("StringProperty"),
        new Column("IntProperty", ColumnType.Integer) { Nullable = false },
        new Column("BoolProperty", ColumnType.Integer) { Nullable = false, CheckExpression = "IN (0, 1)" }
    }
});
database.Create(); // This method creates the database and any specified prior to calling this method.
```

### Tables from Objects:
* The above examples require you to specify the table structure yourselves, but Sqlite.Database.Management can also create this structure for you.
* To do so, we use the generic `ObjectMapper<T>` class.
Suppose you have the following class:
```C#
public class Demo
{
    public string StringProperty { get; set; }

    public int IntProperty { get; set; }

    public bool BoolProperty { get; set; }
}
```
Then Sqlite.Database.Management can create a table based on the structure of your class. The following will create the same table structure as seen in the above examples.
```C#
var database = new Database("Data Source=MyDatabase.sqlite;");
database.Tables.Add(ObjectMapper<Demo>.Table);
database.Create();
```

##### Primary Keys:
* Note that Sqlite.Database.Management will automatically detect and create Primary Key constraints for columns named Id or {TypeName}Id if no primary key column is specified by the user.
* Composite primary keys are not yet supported.
* A primary key is required for the `Update` extension method in order to uniquely identify the record to update.

### Extension Methods:
As stated above, there are a number of extension methods that allow for extremely easy CRUD operations on relatively simple types.
To use these, you will need to add the following statement to your file `using Sqlite.Database.Management.Extensions`.
```C#
var database = new Database("Data Source=MyDatabase.sqlite;");
var table = ObjectMapper<Demo>.Table;
table.PrimaryKey = "IntProperty" // This is required for the Update Method.
database.Tables.Add(table);
database.Create();

var demo = new Demo { IntProperty = 1, StringProperty = "Hi", BoolProperty = false };
// Insert a new record.
database.Insert("Hi");

// Update an existing record (note that this method requires a PrimaryKey to be specified).
demo.StringProperty = "Hello";
database.Update(demo);

// Select Records:
var fromDatabase = database.Select<Demo>(); // Returns a lazily evaluated IEnumerable of Demo objects.

// Delete a record.
database.Delete(demo);
```

### Database Collections:
* Sqlite.Database.Management also supports managing collections of databases.
```C#
var connectionStrings = new List<string> { "Data Source=Database1.sqlite;", "Data Source=Database2.sqlite;" , "Data Source=Database3.sqlite;"  };
var tables = new List<Table> { ObjectMapper<Demo>.Table };
var databases = new DatabaseCollection(connectionStrings, tables); // This will create all three databases and the specified tables in them.

// Each database can then be accessed by its connection string.
var firstDatabase = databases["Data Source=Database1.sqlite;"];
```