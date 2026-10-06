using Models;
using Microsoft.EntityFrameworkCore;
namespace Context;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options){}
    public DbSet<Alert> Alerts => Set<Alert>();
}