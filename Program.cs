using APS.Data;
using APS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<APSContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Seed admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedAdminUserAsync(services).GetAwaiter().GetResult();
}

app.Run();

// Funcție async pentru seed admin
async Task SeedAdminUserAsync(IServiceProvider services)
{
    var userManager = services.GetRequiredService<UserManager<User>>();
    var adminEmail = "123@gmail.com";
    var adminPassword = "Calin14!";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "Admin",
            IsAdmin = true,
            IsActive = true, // Doar adminul seed e activ automat
            EmailConfirmed = true,
            City = "Bucuresti",
            Phone = "0700000000",
            JournalistType = "Admin",
            Publication = "Admin",
            ProfilePictureUrl = string.Empty,
            Biography = string.Empty,
            DateOfBirth = new DateTime(1990, 1, 1) // Adding default date of birth
        };
        await userManager.CreateAsync(adminUser, adminPassword);
    }
}
