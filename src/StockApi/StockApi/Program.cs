using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using StockApi.Grpc.Protos;
using StockApi.Grpc.Services;
using StockApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnectionString")));

// GPRC Service Register
builder.Services.AddGrpc();

var app = builder.Build();

#region Create DataBase and Migration
var scoped = app.Services.CreateScope();
var context = scoped.ServiceProvider.GetService<AppDbContext>();
//context?.Database.EnsureCreated();
context?.Database.EnsureCreated();
context?.Database.Migrate();
#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGrpcService<CrudProductServiceImpl>();
app.MapControllers();

app.Run();