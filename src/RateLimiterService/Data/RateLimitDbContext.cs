using Microsoft.EntityFrameworkCore;
using RateLimiterService.Data.Models;

namespace RateLimiterService.Data;

public class RateLimitDbContext : DbContext
{
    public RateLimitDbContext(DbContextOptions<RateLimitDbContext> options)
        : base(options)
    {
    }

    public DbSet<RateLimitEntry> RateLimits { get; set; } = null!;
}
