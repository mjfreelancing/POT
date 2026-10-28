using System.Text.Json;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

/// <summary>
/// Reads the operations the generated contract documents, which is the inventory the authorization matrix is
/// generated from.
/// </summary>
/// <remarks>
/// The contract is read from the running host, so the inventory is the document this build generates rather than a
/// copy kept beside the tests.
/// </remarks>
public static class DocumentedOperations
{
    private const string ContractPath = "/openapi/v1.json";

    /// <summary>
    /// Reads every operation the contract documents.
    /// </summary>
    /// <param name="client">A client for the host whose contract is being read.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The documented operations, ordered so a run reports them the same way every time.</returns>
    public static async Task<IReadOnlyList<DocumentedOperation>> ReadAsync(HttpClient client, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(ContractPath, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"The contract could not be read from {ContractPath}: {(int)response.StatusCode} {response.StatusCode} {content}");
        }

        using var document = JsonDocument.Parse(content);

        var operations = new List<DocumentedOperation>();

        foreach (var path in document.RootElement.GetProperty("paths").EnumerateObject())
        {
            foreach (var operation in path.Value.EnumerateObject())
            {
                if (!IsHttpMethod(operation.Name))
                {
                    continue;
                }

                var operationElement = operation.Value;
                var carriesRequestBody = operationElement.TryGetProperty("requestBody", out _);
                var endpointOperation = EndpointOperation.Create(operation.Name, path.Name);

                operations.Add(new DocumentedOperation(endpointOperation, carriesRequestBody));
            }
        }

        return
        [
            .. operations
                .OrderBy(documented => documented.Operation.Path, StringComparer.Ordinal)
                .ThenBy(documented => documented.Operation.Method, StringComparer.Ordinal)
        ];
    }

    private static bool IsHttpMethod(string name)
    {
        return name is "get" or "post" or "put" or "delete" or "patch";
    }
}
