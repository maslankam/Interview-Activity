using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using RateLimiterService.Data;
using RateLimiterService.Data.Models;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using RateLimiter.Grpc;

namespace RateLimiterService.Services
{
    public class RateLimiterService : global::RateLimiter.Grpc.RateLimiter.RateLimiterBase
    {
        private readonly RateLimitDbContext _db;
        private readonly State.RateLimiter _rateLimiter;

        public RateLimiterService(RateLimitDbContext db, State.RateLimiter rateLimiter)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
        }

        public override async Task<ConfigureResourceResponse> ConfigureResource(ConfigureResourceRequest request, ServerCallContext context)
        {
            if (request is null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));

            if (string.IsNullOrWhiteSpace(request.Resource))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource name cannot be empty"));

            if (request.MaxRequests <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MaxRequests must be greater than 0"));

            if (request.WindowSeconds <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "WindowSeconds must be greater than 0"));

            var existingEntry = await _db.RateLimits.SingleOrDefaultAsync(r => r.resource == request.Resource);

            if (existingEntry is null)
            {
                _db.RateLimits.Add(new RateLimitEntry
                {
                    resource = request.Resource,
                    max_requests = request.MaxRequests,
                    window_seconds = request.WindowSeconds
                });
            }
            else
            {
                existingEntry.max_requests = request.MaxRequests;
                existingEntry.window_seconds = request.WindowSeconds;
            }

            await _db.SaveChangesAsync();

            await _rateLimiter.UpdateRateLimitAsync(request.Resource);

            return new ConfigureResourceResponse { Success = true };
        }

        public override async Task<RateLimitResponse> CheckRateLimit(RateLimitRequest request, ServerCallContext context)
        {
            if (request is null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));

            if (string.IsNullOrWhiteSpace(request.Resource))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource cannot be empty"));

            if (string.IsNullOrWhiteSpace(request.ClientId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ClientId cannot be empty"));

            bool isAllowed = await _rateLimiter.IsRequestAllowedAsync(request.Resource, request.ClientId);  
            if(isAllowed)
            {
                return new RateLimitResponse { Allowed = true };
            }
            return new RateLimitResponse { Allowed = false };
        }

    }
}
