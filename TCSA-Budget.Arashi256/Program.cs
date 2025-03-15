using Microsoft.EntityFrameworkCore;
using TCSA_Budget.Arashi256.Interfaces;
using TCSA_Budget.Arashi256.Models; 
using TCSA_Budget.Arashi256.Repositories;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
// Register ApplicationDbContext with SQL Server.
builder.Services.AddDbContext<BudgetDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString") ?? throw new InvalidOperationException("Connection string not found.")));
// Register IRepository and Repository with the DI container.
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
// Build dat bitch.
var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transaction}/{action=Index}/{id?}");
// Fly, my pretty! Fly! 
app.Run();