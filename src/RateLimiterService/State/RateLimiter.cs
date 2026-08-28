using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using RateLimiterService.Data;

namespace RateLimiterService.State
{
    public class RateLimiter
    {
        private readonly ConcurrentDictionary<string, RateLimitCounter> _counters = new();
        private readonly RateLimitDbContext _db;

        public RateLimiter(RateLimitDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Task<bool> IsRequestAllowedAsync(string resource, string clientId)
        {
            var key = $"{resource}:{clientId}";
            var now = DateTime.UtcNow;

            if (!_counters.TryGetValue(key, out var counter))
            {
                var rateLimit = _db.RateLimits.SingleOrDefault(r => r.resource == resource);
                var maxRequests = rateLimit?.max_requests ?? 10; // Default limit if not found
                var window = rateLimit?.window_seconds ?? 60; // Default window if not found   

                counter = new RateLimitCounter { WindowStart = now, RequestsCount = 0, RequestsLimit = maxRequests, Window = TimeSpan.FromSeconds(window) };

                _counters.TryAdd(key, counter);
            }

            lock (counter)
            {
                if (DateTime.UtcNow - counter.WindowStart > counter.Window)
                {
                    counter.WindowStart = now;
                    counter.RequestsCount = 1;
                    return Task.FromResult(true);
                }

                
                if (counter.RequestsCount < counter.RequestsLimit)
                {
                    counter.RequestsCount++;
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }


        public async Task UpdateRateLimitAsync(string resource)
        {
            var entry = await _db.RateLimits.SingleOrDefaultAsync(r => r.resource == resource);

            if (entry is null) return;

            foreach (var kvp in _counters
                         .Where(kvp => kvp.Key.StartsWith(entry.resource + ":", StringComparison.Ordinal)).ToList())
            {
                var counter = kvp.Value;
                lock (counter)
                {
                    counter.RequestsLimit = entry.max_requests;
                    counter.Window = TimeSpan.FromSeconds(entry.window_seconds);
                    counter.WindowStart = DateTime.UtcNow;
                    counter.RequestsCount = 0;
                }
            }
        }

        private class RateLimitCounter
        {
            public DateTime WindowStart;
            public int RequestsCount;
            public int RequestsLimit;
            public TimeSpan Window;
        }
    }
}