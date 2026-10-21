using Pot.AspNetCore.Integration.Tests.Host;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

public class OpenApiSecurityFixture : IntegrationFixtureBase
{
    [Fact]
    public async Task Should_Describe_Standard_ProblemDetails_For_Api_Operations()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseContent);

        using var openApiDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(
            TestContext.Current.CancellationToken);

        openApiDocument.ShouldNotBeNull();

        var loginOperation = openApiDocument!.RootElement.GetProperty("paths")
            .GetProperty("/api/auth/login")
            .GetProperty("post");

        foreach (var statusCode in new[] { "400", "500" })
        {
            var problemDetailsContent = loginOperation.GetProperty("responses")
                .GetProperty(statusCode)
                .GetProperty("content")
                .GetProperty("application/problem+json");

            problemDetailsContent.GetProperty("schema")
                .GetProperty("$ref")
                .GetString()
                .ShouldBe("#/components/schemas/ProblemDetails");
        }
    }

    [Fact]
    public async Task Should_Include_Xml_Comments_In_OpenApi_Schema_Descriptions()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseContent);

        using var openApiDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(
            TestContext.Current.CancellationToken);

        openApiDocument.ShouldNotBeNull();

        var root = openApiDocument!.RootElement;
        var requestSchemaReference = root.GetProperty("paths")
            .GetProperty("/api/auth/login")
            .GetProperty("post")
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();

        requestSchemaReference.ShouldNotBeNull();

        var requestSchemaName = requestSchemaReference!.Split('/').Last();
        var requestSchema = root.GetProperty("components")
            .GetProperty("schemas")
            .GetProperty(requestSchemaName);

        requestSchema.GetProperty("description")
            .GetString()
            .ShouldBe("Carries the credentials and request context needed to sign in a user.");
    }

    [Fact]
    public async Task Should_Use_Distinct_Schemas_For_Feature_Specific_Requests_And_Responses()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseContent);

        using var openApiDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(
            TestContext.Current.CancellationToken);

        openApiDocument.ShouldNotBeNull();

        var root = openApiDocument!.RootElement;
        var loginOperation = root.GetProperty("paths")
            .GetProperty("/api/auth/login")
            .GetProperty("post");
        var createExpenseOperation = root.GetProperty("paths")
            .GetProperty("/api/expenses")
            .GetProperty("post");
        var getExpenseOperation = root.GetProperty("paths")
            .GetProperty("/api/expenses/{id}")
            .GetProperty("get");
        var getIncomeOperation = root.GetProperty("paths")
            .GetProperty("/api/incomes/{id}")
            .GetProperty("get");

        var loginRequestSchemaReference = GetRequestSchemaReference(loginOperation);
        var expenseRequestSchemaReference = GetRequestSchemaReference(createExpenseOperation);
        loginRequestSchemaReference.ShouldBe("#/components/schemas/Auth_Login_Request");
        expenseRequestSchemaReference.ShouldBe("#/components/schemas/Expenses_Create_Request");
        loginRequestSchemaReference.ShouldNotBe(expenseRequestSchemaReference);

        var loginResponseSchemaReference = GetResponseSchemaReference(loginOperation, "200");
        var expenseResponseSchemaReference = GetResponseSchemaReference(createExpenseOperation, "201");
        loginResponseSchemaReference.ShouldBe("#/components/schemas/Auth_Login_Response");
        expenseResponseSchemaReference.ShouldBe("#/components/schemas/Expenses_Create_Response");
        loginResponseSchemaReference.ShouldNotBe(expenseResponseSchemaReference);

        var schemas = root.GetProperty("components").GetProperty("schemas");
        schemas.TryGetProperty("Request", out _).ShouldBeFalse();
        schemas.TryGetProperty("Response", out _).ShouldBeFalse();

        var loginRequestSchema = schemas.GetProperty("Auth_Login_Request");
        loginRequestSchema.GetProperty("description")
            .GetString()
            .ShouldBe("Carries the credentials and request context needed to sign in a user.");
        loginRequestSchema.GetProperty("properties").TryGetProperty("username", out _).ShouldBeTrue();
        loginRequestSchema.GetProperty("properties").TryGetProperty("password", out _).ShouldBeTrue();

        var expenseRequestSchema = schemas.GetProperty("Expenses_Create_Request");
        expenseRequestSchema.GetProperty("description")
            .GetString()
            .ShouldBe("Carries the values needed to create an expense.");
        expenseRequestSchema.GetProperty("properties").TryGetProperty("description", out _).ShouldBeTrue();
        expenseRequestSchema.GetProperty("properties").TryGetProperty("accountRowId", out _).ShouldBeTrue();

        var loginResponseSchema = schemas.GetProperty("Auth_Login_Response");
        loginResponseSchema.GetProperty("description")
            .GetString()
            .ShouldBe("Describes the result of a user sign-in attempt.");
        loginResponseSchema.GetProperty("properties").TryGetProperty("status", out _).ShouldBeTrue();
        loginResponseSchema.GetProperty("properties").TryGetProperty("accessToken", out _).ShouldBeTrue();

        var expenseResponseSchema = schemas.GetProperty("Expenses_Create_Response");
        expenseResponseSchema.GetProperty("description")
            .GetString()
            .ShouldBe("Describes the result of creating an expense.");
        expenseResponseSchema.GetProperty("properties").TryGetProperty("rowId", out _).ShouldBeTrue();
        expenseResponseSchema.GetProperty("properties").TryGetProperty("etag", out _).ShouldBeTrue();
        expenseResponseSchema.GetProperty("properties").TryGetProperty("accrualStart", out _).ShouldBeTrue();

        var expenseDetailsResponseSchema = schemas.GetProperty("Expenses_Get_Response");
        var incomeDetailsResponseSchema = schemas.GetProperty("Incomes_Get_Response");
        var expenseAccountSchema = expenseDetailsResponseSchema.GetProperty("properties")
            .GetProperty("account");
        var incomeAccountSchema = incomeDetailsResponseSchema.GetProperty("properties")
            .GetProperty("account");
        var expenseAccountSchemaReference = GetSchemaReference(expenseAccountSchema);
        var incomeAccountSchemaReference = GetSchemaReference(incomeAccountSchema);
        expenseAccountSchemaReference.ShouldNotBeNull(expenseAccountSchema.ToString());
        incomeAccountSchemaReference.ShouldNotBeNull(incomeAccountSchema.ToString());

        GetResponseSchemaReference(getExpenseOperation, "200")
            .ShouldBe("#/components/schemas/Expenses_Get_Response");
        GetResponseSchemaReference(getIncomeOperation, "200")
            .ShouldBe("#/components/schemas/Incomes_Get_Response");
        expenseAccountSchemaReference.ShouldBe("#/components/schemas/Expenses_Get_AccountModel");
        incomeAccountSchemaReference.ShouldBe("#/components/schemas/Incomes_Get_AccountModel");
        expenseAccountSchemaReference.ShouldNotBe(incomeAccountSchemaReference);
    }

    [Fact]
    public async Task Should_Describe_ReadOnly_Collection_Contracts_As_OpenApi_Arrays()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseContent);

        using var openApiDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(
            TestContext.Current.CancellationToken);

        openApiDocument.ShouldNotBeNull();

        var schemas = openApiDocument!.RootElement.GetProperty("components").GetProperty("schemas");
        var updateRolesRequest = schemas.GetProperty("Users_UpdateRoles_Request");
        var accrualStatusResponse = schemas.GetProperty("Accruals_Status_Response");
        var projectionResponse = schemas.GetProperty("Projections_Get_Response");

        updateRolesRequest.GetProperty("properties").GetProperty("roleIds")
            .GetProperty("type").GetString().ShouldBe("array");
        accrualStatusResponse.GetProperty("properties").GetProperty("expenseRenewalsRequired")
            .GetProperty("type").GetString().ShouldBe("array");
        accrualStatusResponse.GetProperty("properties").GetProperty("incomeRenewalsRequired")
            .GetProperty("type").GetString().ShouldBe("array");
        projectionResponse.GetProperty("properties").GetProperty("accounts")
            .GetProperty("type").GetString().ShouldBe("array");
    }

    [Fact]
    public async Task Should_Declare_Bearer_Security_For_Protected_Api_Operations_Only()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseContent);

        using var openApiDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(
            TestContext.Current.CancellationToken);

        openApiDocument.ShouldNotBeNull();

        var root = openApiDocument!.RootElement;
        var bearerScheme = root.GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("Bearer");

        bearerScheme.GetProperty("type").GetString().ShouldBe("http");
        bearerScheme.GetProperty("scheme").GetString().ShouldBe("bearer");
        bearerScheme.GetProperty("bearerFormat").GetString().ShouldBe("JWT");

        var apiOperations = root.GetProperty("paths")
            .EnumerateObject()
            .Where(path => path.Name.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => path.Value.EnumerateObject()
                .Where(operation => IsHttpMethod(operation.Name))
                .Select(operation => (Path: path.Name, Operation: operation.Value)))
            .ToList();

        var protectedOperations = apiOperations
            .Where(apiOperation => !apiOperation.Path.StartsWith("/api/auth/", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var anonymousAuthOperations = apiOperations
            .Where(apiOperation => apiOperation.Path.StartsWith("/api/auth/", StringComparison.OrdinalIgnoreCase))
            .ToList();

        protectedOperations.ShouldNotBeEmpty();
        anonymousAuthOperations.ShouldNotBeEmpty();

        foreach (var apiOperation in protectedOperations)
        {
            var security = apiOperation.Operation.GetProperty("security");
            security.GetArrayLength().ShouldBe(1);
            security[0].GetProperty("Bearer").GetArrayLength().ShouldBe(0);
        }

        foreach (var apiOperation in anonymousAuthOperations)
        {
            apiOperation.Operation.TryGetProperty("security", out _)
                .ShouldBeFalse($"{apiOperation.Path} should remain anonymous in OpenAPI");
        }
    }

    [Fact]
    public async Task Should_Declare_The_Optional_CorrelationId_Header_On_Every_Api_Operation()
    {
        // CorrelationIdMiddleware accepts the header on every request without any endpoint declaring it, so only the
        // operation transformer puts it in the document. Constrained exactly as the middleware enforces it.
        using var client = CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseContent);

        using var openApiDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(
            TestContext.Current.CancellationToken);

        openApiDocument.ShouldNotBeNull();

        var apiOperations = openApiDocument!.RootElement.GetProperty("paths")
            .EnumerateObject()
            .Where(path => path.Name.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => path.Value.EnumerateObject()
                .Where(operation => IsHttpMethod(operation.Name))
                .Select(operation => (Path: path.Name, Operation: operation.Value)))
            .ToList();

        apiOperations.ShouldNotBeEmpty();

        foreach (var apiOperation in apiOperations)
        {
            var parameter = apiOperation.Operation.GetProperty("parameters")
                .EnumerateArray()
                .Where(item => item.GetProperty("name").GetString() == "X-Correlation-Id")
                .ToList()
                .ShouldHaveSingleItem($"{apiOperation.Path} should declare X-Correlation-Id once");

            parameter.GetProperty("in").GetString().ShouldBe("header");
            (parameter.TryGetProperty("required", out var required) && required.GetBoolean()).ShouldBeFalse();

            var schema = parameter.GetProperty("schema");
            schema.GetProperty("maxLength").GetInt32().ShouldBe(128);
            schema.GetProperty("pattern").GetString().ShouldBe("^[A-Za-z0-9_.:-]+$");

            // Echoed on every response, and on a success the header is the only place the id travels.
            foreach (var operationResponse in apiOperation.Operation.GetProperty("responses").EnumerateObject())
            {
                var documented = operationResponse.Value.TryGetProperty("headers", out var headers) &&
                    headers.TryGetProperty("X-Correlation-Id", out _);

                documented.ShouldBeTrue($"{apiOperation.Path} {operationResponse.Name} should declare the X-Correlation-Id response header");
            }
        }
    }

    [Fact]
    public async Task Should_Describe_The_Common_ProblemDetails_Members_As_Required_Strings()
    {
        // Every error body carries them, but they travel in the Extensions dictionary the generator cannot see, so
        // only the schema transformer puts them in the document.
        using var client = CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseContent);

        using var openApiDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(
            TestContext.Current.CancellationToken);

        openApiDocument.ShouldNotBeNull();

        var problemDetailsSchema = openApiDocument!.RootElement.GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("ProblemDetails");

        var required = problemDetailsSchema.GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToList();

        foreach (var member in new[] { "correlationId", "traceId", "instance" })
        {
            required.ShouldContain(member);

            // A plain string, not string-or-null: the members are never null, and a nullable type would make a
            // generated client handle a null that never arrives.
            problemDetailsSchema.GetProperty("properties")
                .GetProperty(member)
                .GetProperty("type")
                .GetString()
                .ShouldBe("string", member);
        }
    }

    [Fact]
    public async Task Should_Key_Every_Schema_Component_With_Characters_OpenApi_Allows()
    {
        // OpenAPI restricts component keys to this pattern. A key outside it - "Response[]" for an array that was
        // given a component of its own - is rejected by some generators and validators.
        using var client = CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseContent);

        using var openApiDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(
            TestContext.Current.CancellationToken);

        openApiDocument.ShouldNotBeNull();

        var invalidKeys = openApiDocument!.RootElement.GetProperty("components")
            .GetProperty("schemas")
            .EnumerateObject()
            .Select(schema => schema.Name)
            .Where(name => !Regex.IsMatch(name, "^[a-zA-Z0-9.\\-_]+$"))
            .ToList();

        invalidKeys.ShouldBeEmpty();
    }

    protected override ApiWebApplicationFactory CreateApiWebApplicationFactory(string dbHost, int dbPort)
    {
        return new DevelopmentApiWebApplicationFactory(dbHost, dbPort);
    }

    private static bool IsHttpMethod(string operationName)
    {
        return operationName is "get" or "post" or "put" or "delete" or "patch" or "options" or "head" or "trace";
    }

    private static string GetRequestSchemaReference(JsonElement operation)
    {
        return operation.GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString()!;
    }

    private static string GetResponseSchemaReference(JsonElement operation, string statusCode)
    {
        return operation.GetProperty("responses")
            .GetProperty(statusCode)
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString()!;
    }

    private static string? GetSchemaReference(JsonElement schema)
    {
        if (schema.TryGetProperty("$ref", out var reference))
        {
            return reference.GetString();
        }

        foreach (var alternativeType in new[] { "anyOf", "oneOf", "allOf" })
        {
            if (!schema.TryGetProperty(alternativeType, out var alternatives))
            {
                continue;
            }

            foreach (var alternative in alternatives.EnumerateArray())
            {
                var alternativeReference = GetSchemaReference(alternative);

                if (alternativeReference is not null)
                {
                    return alternativeReference;
                }
            }
        }

        return null;
    }
}
