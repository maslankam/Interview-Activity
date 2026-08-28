using System;

namespace RateLimiterService.Data.Models;

public record RateLimitEntry
{
    public int Id { get; set; }
    public string resource { get; set; } = string.Empty;
    public int max_requests { get; set; }
    public int window_seconds { get; set; }
}
