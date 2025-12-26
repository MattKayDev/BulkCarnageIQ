using BulkCarnageIQ.Web.Components;
using BulkCarnageIQ.Web.Components.Account;
using BulkCarnageIQ.Core.Identity;
using BulkCarnageIQ.Infrastructure.Persistence;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BulkCarnageIQ.Core.Contracts;
using BulkCarnageIQ.Infrastructure.Repositories;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// If the connection string uses a relative SQLite file path (e.g. "Data Source=app.db"),
// make it absolute so the provider can open the file correctly in different working dirs.
if (connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
{
    var ds = connectionString.Substring("Data Source=".Length).Trim();
    if (!Path.IsPathRooted(ds))
    {
         var fullPath = Path.Combine(builder.Environment.ContentRootPath, ds);
         connectionString = $"Data Source={fullPath}";
     }
}

builder.Services.AddDbContext<AppDbContext>(options =>
     options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// DI:
builder.Services.AddScoped<IMealEntryService, MealEntryService>();
builder.Services.AddScoped<IFoodItemService, FoodItemService>();
builder.Services.AddScoped<IGroceryListService, GroceryListService>();
builder.Services.AddScoped<IWeightLogService, WeightLogService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IGroupFoodService, GroupFoodService>();
builder.Services.AddScoped<IEngineService, EngineService>();

var app = builder.Build();


// Ensure database is created / migrations applied at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Fallback for SQLite if migrations contain provider-specific SQL
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
        logger.LogWarning(ex, "Migrate() failed; falling back to EnsureCreated().");
        try
        {
            var db = services.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }
        catch (Exception inner)
        {
            logger.LogError(inner, "Database initialization failed.");
            throw;
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
