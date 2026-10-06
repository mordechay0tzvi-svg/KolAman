using Models;
using Microsoft.EntityFrameworkCore;
namespace DataContext;
public class Context : DbContext
{
    public Context(DbContextOptions<Context> options): base(options){}
    public DbSet<Alert> SouthAlerts => Set<Alert>();
    public DbSet<Alert> NorthAlerts => Set<Alert>();
    public DbSet<Alert> CenterAlerts => Set<Alert>();
    public DbSet<Alert> OverseasAlerts => Set<Alert>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Alert>().HasKey(c => c.alert_id);
    }
}