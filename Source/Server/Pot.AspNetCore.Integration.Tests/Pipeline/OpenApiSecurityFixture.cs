using Pot.AspNetCore.Integration.Tests.Host;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

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
