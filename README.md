- Building solution: dotnet build
- Running program: dotnet run
- Quick smoke tests with `grpcurl` 
- Configure a resource:
```bash
grpcurl -plaintext -import-path . -proto ratelimiter.proto \
  -d '{"resource":"api/users","max_requests":3,"window_seconds":60}' \
  localhost:5000 RateLimiter.ConfigureResource
```
- Check rate limit (repeat to hit the limit):
```bash
grpcurl -plaintext -import-path . -proto ratelimiter.proto \
  -d '{"client_id":"user1","resource":"api/users"}' \
  localhost:5000 RateLimiter.CheckRateLimit
```

2. Decisions and trade-offs:
I decided to use singleton pattern for the limiter and concurrent dictionary to keep timers. Production grade system could use redis or other quick base.
Service uses scoped lifetime to handle multiple requests at once.
Setup for each api is stored in db, I used in memory for simplocity of prototype implementation.
3. ConfigureResourceResponse is not sending data with success status = 0 instead empty response. 
Service requests need more input validation. Validation if inserted api name exist. Pruning inactive users to keep low usage of memory. Setup method is not proteted itself for flooding with requests.
Adding unit tests and integration tests.
4. For simplicity I assume no authentication and TLS encryption.
5. Updating setting requires lookup of whole collection of counters. It could be implemented more efficient if frequent changes are required. 
