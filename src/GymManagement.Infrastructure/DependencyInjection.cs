using System.Text;
using GymManagement.Application.Common.Interfaces;
using GymManagement.Application.Common.Interfaces.CosmosDB;
using GymManagement.Domain.Common.Interfaces;
using GymManagement.Infrastructure.Admins.Persistence;
using GymManagement.Infrastructure.Admins.Persistence.CosmosDB;
using GymManagement.Infrastructure.Authentication.PasswordHasher;
using GymManagement.Infrastructure.Authentication.TokenGenerator;
using GymManagement.Infrastructure.Common.Persistence;
using GymManagement.Infrastructure.Common.Persistence.CosmosDb;
using GymManagement.Infrastructure.Common.Persistence.CosmosDb.Interfaces;
using GymManagement.Infrastructure.Gyms.Persistence;
using GymManagement.Infrastructure.Subscriptions.Persistence;
using GymManagement.Infrastructure.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GymManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddCosmosPersistence(configuration);
        services.AddAuthentication(configuration);
        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<GymManagementDbContext>(options =>
            options.UseSqlServer(connectionString,
                builder => builder.MigrationsAssembly(typeof(GymManagementDbContext).Assembly.FullName)));

        services.AddScoped<ISubscriptionsRepository, SubscriptionsRepository>();
        services.AddScoped<IGymsRepository, GymsRepository>();
        services.AddScoped<IAdminsRepository, AdminsRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<GymManagementDbContext>());
        
        return services;
    }
    
    private static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.Section, jwtSettings);

        services.AddSingleton(Options.Create(jwtSettings));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            });


        return services;
    }
    
    private static IServiceCollection AddCosmosPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        CosmosDbSettings cosmosDbConfig = configuration.GetSection("ConnectionStrings:CosmosDB").Get<CosmosDbSettings>();
        // register CosmosDB client and data repositories
        services.AddCosmosDb(cosmosDbConfig.EndpointUrl,
            cosmosDbConfig.PrimaryKey,
            cosmosDbConfig.DatabaseName,
            cosmosDbConfig.Containers);
        
        services.AddScoped<ICosmosDBRoomRepository, CosmosDbRoomRepository>();

        return services;
    }
    
    private static IServiceCollection AddCosmosDb(this IServiceCollection services,
        string endpointUrl,
        string primaryKey,
        string databaseName,
        List<ContainerInfo> containers)
    {
        Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(endpointUrl, primaryKey);
        CosmosDbContainerFactory cosmosDbClientFactory = new CosmosDbContainerFactory(client, databaseName, containers);

        // Microsoft recommends a singleton client instance to be used throughout the application
        // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.cosmosclient?view=azure-dotnet#definition
        // "CosmosClient is thread-safe. Its recommended to maintain a single instance of CosmosClient per lifetime of the application which enables efficient connection management and performance"
        services.AddSingleton<ICosmosDbContainerFactory>(cosmosDbClientFactory);

        return services;
    }
}