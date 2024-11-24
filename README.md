# Setting and run project

## Backend .net project

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
