using APS.Policys;
using APS.Data;
using APS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static APS.Policys.IsModeratorRequirment;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure PostgreSQL
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
    builder.Configuration.GetConnectionString("DefaultConnection");

// If using Railway's PostgreSQL, convert the connection string
if (connectionString != null && connectionString.StartsWith("postgres://"))
{
    connectionString = connectionString.Replace("postgres://", "Host=")
        .Replace("@", ";Username=")
        .Replace(":", ";Password=")
        .Replace("/", ";Database=");
}

builder.Services.AddDbContext<APSContext>(options =>
    options.UseNpgsql(connectionString));

// Add Identity
builder.Services.AddIdentity<User, Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<APSContext>()
    .AddDefaultTokenProviders();

// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = Microsoft.AspNetCore.Identity.IdentityConstants.ExternalScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme;
});

// Add authorization with custom policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.Requirements.Add(new IsAdminRequirement()));
    options.AddPolicy("RequireModerator", policy => policy.Requirements.Add(new IsModeratorRequirement()));
    options.AddPolicy("RequireAdminOrModerator", policy => policy.Requirements.Add(new IsAdminOrModeratorRequirement()));
});

// Register the authorization handler
builder.Services.AddScoped<IAuthorizationHandler, IsAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, IsModeratorRequirment.IsModeratorHandler>();
builder.Services.AddScoped<IAuthorizationHandler, IsAdminOrModeratorHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configure for GitHub Pages
app.UsePathBase("/APS");
app.UseRouting();

// Serve static files
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<APSContext>();
        context.Database.Migrate();
        SeedAdminUserAsync(services).GetAwaiter().GetResult();
        SeedRolesAsync(services).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();

// Funcție async pentru seed admin
async Task SeedAdminUserAsync(IServiceProvider services)
{
    var userManager = services.GetRequiredService<UserManager<User>>();
    var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "123@gmail.com";
    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Calin14!";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Calin",
            LastName = "Stefan",
            IsAdmin = true,
            IsActive = true,
            EmailConfirmed = true,
            City = "Bucuresti",
            Phone = "0700000000",
            JournalistType = "Admin",
            Publication = "Admin",
            ProfilePictureUrl = string.Empty,
            Biography = string.Empty,
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        await userManager.CreateAsync(adminUser, adminPassword);
    }
}

async Task SeedRolesAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
    string[] roles = { "Admin", "Moderator" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(role));
    }
}
