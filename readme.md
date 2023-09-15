# Sample .Net Core Minimal API Web App with Sqlite
This is a .net core minimal API web app that uses System.Data.Sqlite;

It has a list of people who have a firstName and lastName (and an ID in the database).

# Building
dotnet build, or open the .csproj and build it.

# Running
Run the resultant EXE file.

# UI
A web UI can be found at http://localhost:5000/index.html.

# Sample Curl Commands
Note these are quoted for bash.  (On Windows, you can run them in Git Bash, MobaXTerm, or Cygwin).

## Add some people
```
curl -X POST -H "Content-Type: application/json" -d '{"LastName":"Doe","FirstName":"John"}' http://localhost:5000/people
curl -X POST -H "Content-Type: application/json" -d '{"LastName":"Smith","FirstName":"Jane"}' http://localhost:5000/people
```

## Add batch of people
```
curl -X POST -H "Content-Type: application/json" -d '[
    {"FirstName":"Alice","LastName":"Johnson"},
    {"FirstName":"Bob","LastName":"Smith"},
    {"FirstName":"Charlie","LastName":"Brown"}
]' http://localhost:5000/people
```

## Get a specific person
```
curl http://localhost:5000/people/1
```

## Get all people
```
curl http://localhost:5000/people
```