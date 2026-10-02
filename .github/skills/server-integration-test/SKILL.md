---
name: server-integration-test
description: Create or update hosted ASP.NET Core API integration tests that boot the app host and assert HTTP contract behavior (status codes, headers, ProblemDetails) through a real HttpClient.
---

# Server Integration Test

Use this when a test must boot the host, or exercise middleware, auth, routing or real `HttpClient` requests. If it does not, use the `dotnet-unit-test` skill instead.

Conventions are in `.github/instructions/aspnetcore-integration-tests.instructions.md` and `.github/instructions/dotnet-tests.instructions.md`.

1. Place tests under `Source/Server/Pot.AspNetCore.Integration.Tests/`, organized by feature.
2. Reuse the host fixtures in `Host/` (`*WebApplicationFactory.cs`) and helpers in `Host/Extensions`.
3. Assert status code first, then critical contract fields, headers and error shape.
4. Run targeted first, from `Source/Server`:
   `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --filter "FullyQualifiedName~<FixtureOrTestName>"`
5. Then broaden: `dotnet test Pot.AspNetCore.Integration.Tests/Pot.AspNetCore.Integration.Tests.csproj --nologo --verbosity minimal`.
