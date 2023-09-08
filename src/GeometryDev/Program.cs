using Geomertry.Models;
using Geometry.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000",
                                              "http://www.contoso.com")
                          .AllowCredentials()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                      });
});

builder.Services.AddPooledDbContextFactory<LTAContext>(options => 
options.UseSqlServer(builder.Configuration.GetConnectionString("LTAContext"), x => x.UseNetTopologySuite().MigrationsAssembly("GeometryDev"))
);

builder.Services.AddScoped<LTADbContextFactory>();
builder.Services.AddScoped(
    sp => sp.GetRequiredService<LTADbContextFactory>().CreateDbContext());

//
var conn = ConnectionMultiplexer.Connect("localhost:6379");
Console.WriteLine($"{conn.IsConnected}");
var db = conn.GetDatabase();
builder.Services.AddSingleton<IDatabase>(db);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
