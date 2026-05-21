# Introduction 
Quizzes API

# Getting Started
1.	In package manager console set the Project of the respective dbContext Repository as a default project (TradeX.Repository)

# How to Add Migrations [sql server]
Package manager console:

*** set TradeX.Repository as Default project in Package Manager Console   

    add-migration 'TradeX_Init' -Context 'TradeXDbContext' -OutputDir 'Migrations\TradeX\SqlServer' -startupProject 'TradeX.Repository.Executor'
    
# How to update database [sql server]

Generate migrations using sql server dbcontext design time options.

*** set TradeX.Repository as Default project in Package Manager Console   

    Update-Database -args '"Server=localhost;Initial Catalog=TradeX-dev;Connection Timeout=30;Integrated Security=True;Encrypt=False"' -Context 'TradeXDbContext' -startupProject 'TradeX.Repository.Executor'

    Update-Database -args '"Server=sql-TradeX-prod.database.windows.net;Database=db-TradeX;User Id=TradeXadmin;Password=Beavis111_;TrustServerCertificate=False;Encrypt=True;"' -Context 'TradeXDbContext' -startupProject 'TradeX.Repository.Executor'

# How to run azure blob storage emulator
cmd prompt as admin

cd C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator
azurite.exe --skipApiVersionCheck

    # run microsoft azure storage explorer

# Swagger UI
Api routes follow REST principles. Using the swagger interface is straightforward.

# How to Get the access token
Visual Studio Code
npm install -g serve
cd PATH_DO_index.html-a (root api solutiona)
serve .
browser: localhost:3000
Login


# Solution Structure
This is an example of Clean Architecture CQRS (Command Query Responsibility Segregation) solution.
Thin clients (AzureFunction API project, Test project) call the main Application logic using the MediatR pattern.
Main business logic is modular and testabile.

# Solution Projects

## _Apps
Folder for the clients or our aplication. 
- .net api


## Application
Main application logic.
Handles exceptions with a global handler.
Measures processing time of each request.
Validates requests using fluent validation rules.
Executes request handlers.
Does db CRUD operations.
Returns data to clients using standard output format, ensuring each response follows the same structure (good for frontend).

## Repository
Implemented Indexes, unique and non unique.
Disabled onDeleteCascade option.
There is a Seed.sql file in the Sql folder. After updating the database, you can run the file from MS SQL Server Management studio.

## Tests
Project: TradeX.Tests projeTradeX. 
CQRS solution design pattern enables testing of the application logic oblivious to the client using it. Tests should only test the Application projeTradeX.
They contain tests for scenarios described in the project documentation.

# Code Examples
Examples of typical scenarios for various standard operations.

## Paging data retrieval
Namespace: TradeX.API.Test
Class: TradeX.API.Test
Url parameter:
[QueryOpenApiParameter("page", "{"index":"0","size":"20"}", false, typeof(string))]
Example: {"size":"10","index":"0"}

## Sorting data
Namespace: TradeX.API.Test
Class: TradeX.API.Test
Url parameter:
[QueryOpenApiParameter("sort", "[{\"fieldName\":\"fieldName\",\"direction\":\"asc|desc\"}}]", false, typeof(string))]
Example: [{"fieldName":"updatedAt","direction":"desc"}]
* it is possible to sort by more than one fieldName in given order

## Filtering data
Namespace: TradeX.API.Test
Class: TradeX.API.Test
Url parameter:
[QueryOpenApiParameter("filter", "[{"fieldName":"fieldName","filter":[{"op":"gt|lt|gte|lte|eq|startsWith|contains","value":"some_value"}]}]", false, typeof(string))]
Example: [{"fieldName":"name","filter":[{"op":"startsWith","value":"value"}]}]
* it is possible to filter by more than one field, logical AND is applied

# Best Practices
    * Clean architecture: CQRS solution design pattern is used. Easily testabile business logic, multiple clients are supported (function app or in the future a standard api, or a console app, etc). *
    * Authentication: JWT. Short lived access tokens are returned in the response so the frontend can store them in memory. Long lived refresh tokens are return as HttpOnly cookies.*
    * Thin clients: It is easy to migrate the entire app to another platform. Azure functions can be replaced by windows services if situation dictates. All logic is placed inside the Application projeTradeX. *
    * Project: abstractions separated from implementations. Rule, abstraction projects don't have any project dependencies *
    * Swagger documentation: all parameters are clearly visible in the swagger UI page. Authentication parameter is also shown, but is not used in the solution. *
    * REST principles: all endpoints follow REST naming principles.*
    * Version control: all endpoints contain a version id in their url (v1). An endpoint of a newer version can easily be created. If possible the old one should not be deleted. *
    * Standard outputs: All endpoints return standardized output. Exceptions, validation errors and successful results are returned in this format (BaseOutput<T>). *
    * Request validation: CQRS feature file contains the request class, models for validation, a request handler and private methods. For brevity and ease of access all of this is placed in the same file. *
    * Global exception handling *
    * Data Integrity: Database contains foreign keys and onCascade delete is turned off. If parent entity needs to be delete, first every child needs to be deleted.*
    * Data Paging: Data retrieval has default sorting followed with the db index for speed and resource saving. *
    * Data Filtering: Data retrieval supports paging and filtering on the db level. This saves bandwith and speeds up the application. *
    * Data Sorting: Data retrieval supports sorting on the db level. Handled in a generic way. Sorting is by default possible on every column endpoint returns. *
    * Db indexes: some unique for integrity reasons and some for speedy data retrieval *
    * Unit test project: tests are create for business use Encounters, no need to test plumbing code. *
    * Data seeding - Seed.sql script is provided in the solution Sql folder. Can be run from MS SQL Mangement Studio *
    * Solution items: important external files are placed in the Solution item folder. *
