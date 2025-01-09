using GymManagement.Api;
using GymManagement.Api.Extensions;
using GymManagement.Application;
using GymManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddConfigurationSettings(builder.Configuration)
    .AddPresentation()
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .ConfigureMassTransit();

var app = builder.Build();
app.AddInfrastructureMiddleware();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();