using Models;
using Microsoft.EntityFrameworkCore;
namespace MySqlContext;
public class Context : DbContext
{
    public Context(DbContextOptions<Context> options): base(options){}
    public DbSet<Alert> Alerts => Set<Alert>();
}