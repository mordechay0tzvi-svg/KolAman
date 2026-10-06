using Models;
using Microsoft.EntityFrameworkCore;
namespace MySqlContext;
public class Context : DbContext
{
    public Context(DbContextOptions<Context> options): base(options){}
    public DbSet<Alert> SouthAlerts => Set<Alert>();
    public DbSet<Alert> NorthAlerts => Set<Alert>();
    public DbSet<Alert> CenterAlerts => Set<Alert>();
    public DbSet<Alert> OverseasAlerts => Set<Alert>();
}