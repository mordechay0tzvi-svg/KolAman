using MySqlContext;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
ServerVersion? serverVersion =  ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<Context>(options => options.UseMySql(connectionString,serverVersion));

var app = builder.Build();

using (var scope = app.Services.CreateScope())  
{
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    while (true)
    {
        var alert = context.CenterAlerts.FirstOrDefault(a => a.status == "WAITING");
        if (alert!.priority == "LOW")
        {
            alert.status = "CANCELLED";
            context.SaveChanges();
            continue;
        }

        alert.status = "INPROGRESS";
        context.SaveChanges();

        var AlertInProgress = Task.Run(async () =>
        {
            Thread.Sleep(101);
            alert.status = "DONE";
            await context.SaveChangesAsync();
        });
        await Task.WhenAll(AlertInProgress);
    }
}