# Hangfire Api
  A program to perform background processing in .NET and .NET Core applications, as well as schedule task !


## Required packages 
> Open terminal, navigate to your project location te type the following commands !

    > dotnet add package Hangfire
    > dotnet add package Hangfire.MySql
    > dotnet add package Owin
    > dotnet add package Microsoft.Owin
    > dotnet add package NewTonSoft.Json
    > dotnet add package MySql.Data.MySqlClient;
  


## AppSettings
> don't forget to set database connection !

    {
      "Logging": {
        "LogLevel": {
          "Default": "Warning"
        }
      },
      "AllowedHosts": "*",
      "ConnectionStrings" : {
        "localhost" : "Server=localhost;User ID=SA;password=Luna1005;Database=hangfire",
        "DefaultConnection" : "Server=?;User ID=SA;password=?;Database=hangfire",
        "MySql" : {
          "gitRepo" : "Server=localhost;Uid=?;PWD=?;Database=gitRepo",
          "gitRepo1" : "Server=localhost;User ID=?;Password=?;Database=gitRepo"
        },
        "Github" : {
          "url" : "https://api.github.com"
        }
      }
    }


# Notice 

Make sure that your database table has the same name as your model, ass well as the colums, if you created a table with diffrent columns name use the [JsonProperty] option in your class as i did !

## Startup class

> change "WickedSs" to your username in github !

    response = await client.GetAsync("/users/WickedSs/repos");
  
That All,
if you want to get more data, you can change the Model class 

