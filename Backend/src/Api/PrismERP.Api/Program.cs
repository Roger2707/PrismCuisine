using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PrismERP.BuildingBlocks.Infrastructure;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Identity.Infrastructure;
using PrismERP.Modules.Identity.Infrastructure.Auth;
using PrismERP.Modules.Identity.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Infrastructure;
using PrismERP.Modules.Inventory.Infrastructure.Persistence;
using PrismERP.Modules.Purchasing.Infrastructure;
using PrismERP.Modules.Purchasing.Infrastructure.Persistence;
using PrismERP.Modules.SalesOrdering.Infrastructure;
using PrismERP.Modules.SalesOrdering.Infrastructure.Persistence;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

#region Scalar API Reference

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Put your JWT Access Token here (NOT INCLUDED Bearer at beginning)"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", scheme);

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>()
        });

        return Task.CompletedTask;
    });
});

#endregion

#region Authentcation & Authorization

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

if (jwt == null || string.IsNullOrEmpty(jwt.SigningKey))
    throw new InvalidOperationException("JwtOptions or SigningKey is not configed in appsettings.json!");

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = key
        };
    })
    .AddCookie("MyRefreshCookieScheme", options =>
    {
        options.Cookie.Name = "refreshToken"; // Cookie Name
        options.Cookie.HttpOnly = true;       // Prevent XSS
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        // Prevent Cookie auto redirect to /Account/Login if error
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

#endregion

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
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseIdentityAuthBlacklistUsers();
app.UseIdentityPermissions();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task EnsureDatabaseAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<PrismERPDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var identitySeeder = scope.ServiceProvider.GetRequiredService<IIdentityDataSeeder>();
    await identitySeeder.SeedAsync();

    var inventorySeeder = scope.ServiceProvider.GetRequiredService<IInventoryDataSeeder>();
    await inventorySeeder.SeedAsync();

    var purchasingSeeder = scope.ServiceProvider.GetRequiredService<IPurchasingDataSeeder>();
    await purchasingSeeder.SeedAsync();

    var salesOrderingSeeder = scope.ServiceProvider.GetRequiredService<ISalesOrderingDataSeeder>();
    await salesOrderingSeeder.SeedAsync();
}
