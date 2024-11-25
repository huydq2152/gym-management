# Setting and run project

## Backend .net project

### Implemented
- CQRS pattern and mediaror pattern using MediatR
- Splitting by feature (vertical slice architecture)
- Result pattern
- Repository pattern
- Unit of work pattern
- Rich domain model (domain driven design)
- Strongly type enums
- Handle error in both domain layer and application layer and convert to presentation layer
- Implement Model validation, Application Cross-Cutting concerns, validating business rules in the domain layer using Fluent validation and MediatR pipeline behavior
- Domain events pattern with orchestration approach
- Eventual Consistency and Transactional Consistency
- Unit test using xunit

### Add Migrations:

1. cd to GymManagement.Api project
2. create migrations folder

   ```
   dotnet ef migrations add \<migration-name\> -p ..\GymManagement.Infrastructure\ -s ..\GymManagement.Api\ -c GymManagementDbContext
   ```

3. create database

   this project use sqlserver, change connection in appsetting.json file to your database and use this cmd to create database arcoding to migrations

   ```
   dotnet ef database update
   ```

### Run project

1. cd to GymManagement.Api project
2. run project

   ```
   dotnet run
   ```

   or use run project feature in your IDE
