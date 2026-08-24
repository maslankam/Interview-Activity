# RateLimiterService

This is a small gRPC server implementing a fixed-window rate limiter.

Build and run (requires .NET SDK):

```bash
dotnet restore
dotnet build
dotnet run --project src/RateLimiterService
```

The service listens on port 5000 and exposes the `RateLimiter` gRPC service defined in `ratelimiter.proto`.
