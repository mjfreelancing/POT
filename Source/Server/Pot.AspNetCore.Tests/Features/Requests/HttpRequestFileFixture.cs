using System.Text.Json;
using System.Text.RegularExpressions;
using Shouldly;

namespace Pot.AspNetCore.Tests.Features.Requests;

/// <summary>
/// Guards the request file and its environment files. They carry the documentation for the values the requests
/// need, and Visual Studio's REST editor silently mis-reads some shapes of them, so the rules they rely on are
/// asserted here rather than left to whoever edits the file next.
/// </summary>
public class HttpRequestFileFixture
{
    private static readonly Regex VariableReferencePattern = new(@"\{\{([^{}]+)\}\}", RegexOptions.Compiled);
    private static readonly Regex ResponseReferencePattern = new(@"\A[\w-]+\.(response|request)\.", RegexOptions.Compiled);
    private static readonly Regex FileVariablePattern = new(@"(?m)^\s*@\w+\s*=", RegexOptions.Compiled);
    private static readonly Regex MarkerOnlyCommentPattern = new(@"(?m)^(//|#)[ \t]*\r?$", RegexOptions.Compiled);
    private static readonly Regex NamedRequestPattern = new(@"(?m)^# @name (\w+)\s*$", RegexOptions.Compiled);

    private static readonly string ServerSourceDirectory = FindServerSourceDirectory();
    private static readonly string RequestFilePath = Path.Combine(ServerSourceDirectory, "Pot.AspNetCore", "Pot.AspNetCore.http");
    private static readonly string EnvironmentFilePath = Path.Combine(ServerSourceDirectory, "Pot.AspNetCore", "http-client.env.json");

    [Fact]
    public void Should_Define_The_Same_Variables_In_Every_Environment()
    {
        var environments = ReadEnvironments();

        // Visual Studio's REST editor does not fall back to a shared block here, so every environment
        // has to define every variable in full.
        environments.Keys.ShouldNotContain("$shared");
        environments.Count.ShouldBeGreaterThan(1);

        var reference = environments.First();

        foreach (var environment in environments)
        {
            environment.Value.ShouldBe(reference.Value, ignoreOrder: true,
                customMessage: $"Environment '{environment.Key}' must define the same variables as '{reference.Key}'.");
        }
    }

    [Fact]
    public void Should_Resolve_Every_Variable_Reference_In_Every_Environment()
    {
        var requestText = File.ReadAllText(RequestFilePath);
        var namedRequests = ReadNamedRequests(requestText);
        var environments = ReadEnvironments();

        foreach (var environment in environments)
        {
            var unresolved = new List<string>();

            foreach (Match match in VariableReferencePattern.Matches(requestText))
            {
                var expression = match.Groups[1].Value;

                // Built-in expressions such as $guid and $timestamp are supplied by the editor.
                if (expression.StartsWith('$'))
                {
                    continue;
                }

                if (ResponseReferencePattern.IsMatch(expression))
                {
                    if (!namedRequests.Contains(expression.Split('.')[0]))
                    {
                        unresolved.Add(expression);
                    }

                    continue;
                }

                if (!environment.Value.Contains(expression))
                {
                    unresolved.Add(expression);
                }
            }

            unresolved.ShouldBeEmpty($"Environment '{environment.Key}' does not define: {string.Join(", ", unresolved)}");
        }
    }

    [Fact]
    public void Should_Not_Declare_File_Variables()
    {
        var requestText = File.ReadAllText(RequestFilePath);

        // Values belong in the environment files so every request file resolves the same keys, and the
        // environment files are documented by the comment block inside the request file.
        FileVariablePattern.IsMatch(requestText)
            .ShouldBeFalse("the request file must read its values from the environment files, not from @ declarations");
    }

    [Fact]
    public void Should_Not_Contain_A_Comment_Line_That_Holds_Only_The_Marker()
    {
        var requestText = File.ReadAllText(RequestFilePath);

        var offendingLines = MarkerOnlyCommentPattern.Matches(requestText)
            .Select(match => 1 + requestText.Take(match.Index).Count(character => character == '\n'))
            .ToList();

        // Visual Studio's REST parser reports HTTP0004/HTTP0005 on the next request's "### title" line and
        // HTTP0006 on the request line, and the request then loses its "Send request" link.
        offendingLines.ShouldBeEmpty(
            $"comment lines holding only their marker break the next request: line(s) {string.Join(", ", offendingLines)}");
    }

    [Fact]
    public void Should_Name_The_Login_And_Refresh_Requests_Above_Their_Post_Lines()
    {
        var lines = File.ReadAllLines(RequestFilePath);
        var namedRequests = new[] { ("login", "/auth/login"), ("refresh", "/auth/refresh") };

        foreach (var (requestName, path) in namedRequests)
        {
            var nameIndex = Array.FindIndex(lines, line => line.TrimEnd() == $"# @name {requestName}");

            nameIndex.ShouldBeGreaterThanOrEqualTo(0,
                $"The '{requestName}' request must be named so later requests can read its response.");

            var requestLineIndex = nameIndex + 1;

            while (requestLineIndex < lines.Length && lines[requestLineIndex].Trim().Length == 0)
            {
                requestLineIndex++;
            }

            lines[requestLineIndex].ShouldStartWith("POST ");
            lines[requestLineIndex].ShouldContain(path);
        }
    }

    [Fact]
    public void Should_Use_The_Setting_Category_Literal_Instead_Of_A_Variable()
    {
        var requestText = File.ReadAllText(RequestFilePath);

        // The category is one of the API's enum names rather than a per-developer value.
        requestText.ShouldNotContain("{{SettingCategory}}", Case.Insensitive);
        requestText.ShouldContain("/settings/EmailBudgetReminder/");
    }

    private static Dictionary<string, HashSet<string>> ReadEnvironments()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(EnvironmentFilePath));

        var environments = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var environment in document.RootElement.EnumerateObject())
        {
            var variables = environment.Value.EnumerateObject()
                .Select(variable => variable.Name)
                .ToHashSet(StringComparer.Ordinal);

            environments[environment.Name] = variables;
        }

        return environments;
    }

    private static HashSet<string> ReadNamedRequests(string requestText)
    {
        return NamedRequestPattern.Matches(requestText)
            .Select(match => match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string FindServerSourceDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "pot.sln")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new InvalidOperationException(
                "The repository root holding pot.sln must be reachable from the test output directory.");
        }

        // The solution sits beside the server projects, so this folder is the server source directory.
        return directory.FullName;
    }
}
