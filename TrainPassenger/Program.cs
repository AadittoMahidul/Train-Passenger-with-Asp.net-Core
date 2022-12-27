using Microsoft.EntityFrameworkCore;
using TrainPassenger.HostedService;
using TrainPassenger.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TrainDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("db")));
builder.Services.AddHostedService<DbSeederHostedService>();
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
var app = builder.Build();

app.UseStaticFiles();
app.MapDefaultControllerRoute();

app.Run();