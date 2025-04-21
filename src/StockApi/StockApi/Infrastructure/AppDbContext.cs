using Microsoft.EntityFrameworkCore;
using StockApi.Domain.Entities;

namespace StockApi.Infrastructure
{
    public class AppDbContext : DbContext
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public DbSet<Product> Products { get; set; }
    }
}
