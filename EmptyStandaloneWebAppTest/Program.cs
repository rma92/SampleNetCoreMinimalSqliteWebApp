using System.Collections.Generic;
using System.Data.SQLite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//use static files serves files out of wwwroot if there's a request that doesn't match a defined endpoint.
app.UseStaticFiles();

//List<Person> people = new List<Person>();
var connectionString = "Data Source=mydatabase.db";
using var connection = new SQLiteConnection(connectionString);
connection.Open();

using var command = connection.CreateCommand();
command.CommandText = @"
    CREATE TABLE IF NOT EXISTS People (
        ID INTEGER PRIMARY KEY AUTOINCREMENT,
        FIRST TEXT,
        LAST TEXT
    )";
command.ExecuteNonQuery();


//app.MapGet("/", () => "Hello World!");

app.MapGet("/people", (HttpContext context) =>
{
    var peopleList = new List<Person>();
    
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT ID, FIRST, LAST FROM People";

    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        var person = new Person(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
        peopleList.Add(person);
    }

    context.Response.WriteAsJsonAsync(peopleList);
});

app.MapGet("/people/{id}", (HttpContext context, int id) =>
{
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT ID, FIRST, LAST FROM People WHERE ID = @Id";
    command.Parameters.AddWithValue("@Id", id);

    using var reader = command.ExecuteReader();
    if (reader.Read())
    {
        var person = new Person(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)); 
        context.Response.WriteAsJsonAsync(person);
    }
    else
    {
        context.Response.StatusCode = 404; // Not Found
    }
});

app.MapPost("/people", (HttpContext context, Person person) =>
{
    using var command = connection.CreateCommand();
    command.CommandText = "INSERT INTO People (FIRST, LAST) VALUES (@FirstName, @LastName)";
    command.Parameters.AddWithValue("@FirstName", person.FirstName);
    command.Parameters.AddWithValue("@LastName", person.LastName);
    command.ExecuteNonQuery();
    context.Response.StatusCode = 201; // Created
});

app.MapPost("/people", async (HttpContext context) =>
{
    try
    {
        var person = await context.Request.ReadFromJsonAsync<Person>();
        if (person != null)
        {
            using var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO People (FIRST, LAST) VALUES (@FirstName, @LastName)";
            insertCommand.Parameters.AddWithValue("@FirstName", person.FirstName);
            insertCommand.Parameters.AddWithValue("@LastName", person.LastName);
            insertCommand.ExecuteNonQuery();
            context.Response.StatusCode = 201; // Created
        }
        else
        {
            context.Response.StatusCode = 400; // Bad Request
        }
    }
    catch (Exception)
    {
        context.Response.StatusCode = 500; // Internal Server Error
    }
});

app.MapPost("/people/batch", async (HttpContext context) =>
{
    try
    {
        var peopleToAdd = await context.Request.ReadFromJsonAsync<List<Person>>();
        if (peopleToAdd != null && peopleToAdd.Any())
        {
            using var transaction = connection.BeginTransaction();

            foreach (var person in peopleToAdd)
            {
                using var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = "INSERT INTO People (FIRST, LAST) VALUES (@FirstName, @LastName)";
                insertCommand.Parameters.AddWithValue("@FirstName", person.FirstName);
                insertCommand.Parameters.AddWithValue("@LastName", person.LastName);
                insertCommand.ExecuteNonQuery();
            }

            transaction.Commit();
            context.Response.StatusCode = 201; // Created
        }
        else
        {
            context.Response.StatusCode = 400; // Bad Request
        }
    }
    catch (Exception)
    {
        context.Response.StatusCode = 500; // Internal Server Error
    }
});

// Rest of your code

app.Run();

public record Person
{
    public int Id { get; init; } // ID property

    public string FirstName { get; init; }
    public string LastName { get; init; }

    // Constructor without the ID
    public Person(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    // Constructor with the ID
    public Person(int id, string firstName, string lastName)
        : this(firstName, lastName) // Call the constructor without ID
    {
        Id = id;
    }
}
