using Microsoft.EntityFrameworkCore;
using Repositories;
using DataContext;
var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
ServerVersion? serverVersion =  ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<Context>(options => options.UseMySql(connectionString,serverVersion));

builder.Services.AddScoped<IRepository, Repository>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
