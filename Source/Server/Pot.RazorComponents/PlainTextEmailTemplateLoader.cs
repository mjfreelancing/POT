namespace Pot.RazorComponents;

public static class PlainTextEmailTemplateLoader
{
    public static readonly string ChangePassword = "Emails.ChangePassword.ChangePasswordEmail.text";
    public static readonly string Signup = "Emails.Signup.SignupEmail.text";

    public static string Populate(string templateName, IDictionary<string, object?> replacements)
    {
        var template = LoadTemplate(templateName);

        foreach (var (key, value) in replacements)
        {
            template = template.Replace($"{{{key}}}", value!.ToString());
        }

        return template;
    }

    private static string LoadTemplate(string templateName)
    {
        var assembly = typeof(PlainTextEmailTemplateLoader).Assembly;
        var resourceName = $"Pot.RazorComponents.{templateName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded resource not found: {resourceName}");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
