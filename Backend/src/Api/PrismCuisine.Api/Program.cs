using PrismCuisine.BuildingBlocks.Infrastructure;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Identity.Infrastructure;
using PrismCuisine.Modules.Identity.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Infrastructure;
using PrismCuisine.Modules.Purchasing.Infrastructure;
using PrismCuisine.Modules.SalesOrder.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddBuildingBlocksInfrastructure(builder.Configuration);

builder.Services
    .AddIdentityModule(builder.Configuration)
    .AddInventoryModule()
    .AddPurchasingModule()
    .AddSalesOrderModule();

var app = builder.Build();

await EnsureDatabaseAsync(app);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseIdentityPermissions();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task EnsureDatabaseAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<PrismCuisineDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var identitySeeder = scope.ServiceProvider.GetRequiredService<IIdentityDataSeeder>();
    await identitySeeder.SeedAsync();
}
