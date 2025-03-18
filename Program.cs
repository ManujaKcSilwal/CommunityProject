using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using cmty_prjt.Data;
using cmty_prjt.Areas.Identity.Data;
using cmty_prjt.Data.Scaffold;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("cmty_prjtDBContextConnection") ?? throw new InvalidOperationException("Connection string 'cmty_prjtDBContextConnection' not found.");

// Register services
builder.Services.AddDbContext<cmty_prjtDBContext>(options => 
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<ClgeventmgmtContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<cmty_prjtUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<cmty_prjtDBContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.Configure<IdentityOptions>(options =>
{     
    options.Password.RequireUppercase = false;
});

var app = builder.Build();

// Ensure admin role exists
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<cmty_prjtUser>>();

    // Create Admin role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Add area route before default route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
