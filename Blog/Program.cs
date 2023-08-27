using Blog.Data;
using Blog.Data.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var provider = builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<IConfiguration>();

// For Entity Framework
builder.Services.AddDbContext<AppDbContext>(op => op.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<IRepository, Repository>();

// For Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(op =>
{
    op.Password.RequireNonAlphanumeric = false;
	op.Password.RequireUppercase = false;
    op.Password.RequireDigit = false;
    op.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Adding Authentication
builder.Services.AddAuthentication(op =>
{
    op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    op.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});

var app = builder.Build();

try
{
	var scope = app.Services.CreateScope();

	var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	var usrMngr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
	var roleMngr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

	ctx.Database.EnsureCreated();

	var adminRole = new IdentityRole("Admin");
	if (!ctx.Roles.Any())
	{
		// create a role
		roleMngr.CreateAsync(adminRole).GetAwaiter().GetResult();
	}

	if (!ctx.Users.Any(u => u.UserName == "admin"))
	{
		// create an admin
		var adminUser = new IdentityUser
		{
			UserName = "admin",
			Email = "admin@test.com"
		};
		var result = usrMngr.CreateAsync(adminUser, "password").GetAwaiter().GetResult();
		// add role to user
		usrMngr.AddToRoleAsync(adminUser, adminRole.Name).GetAwaiter().GetResult();
	}
}
catch (Exception e)
{
	Console.WriteLine(e.Message);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
