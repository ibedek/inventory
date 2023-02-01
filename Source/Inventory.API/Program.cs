using Inventory.API;
using Inventory.API.Services;
using Inventory.Application;
using Inventory.Application.Interfaces;
using Inventory.Application.Services;
using Inventory.Persistence;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLoggingInfrastructure();
builder.Services.AddSwaggerInfrastructure(null);
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ICurrentUser, CurrentUserService>();
builder.Services.AddTransient<IDateTime, MachineDateTime>();

builder.Services.AddInventoryPersistence(builder.Configuration);

builder.Services.AddInventoryApplication();

var app = builder.Build();

app.UseSwaggerInfrastructure(app.Environment);
app.UseRouting();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();