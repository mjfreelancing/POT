# Rule documents: server

Binding conventions for the .NET server. Each rule document in this folder is authoritative for the
rules it states; where a tool can check a rule, the tool's configuration wins and the document points at
it.

Each rule document in this folder:

- stays within about 800 lines and opens with a quick-reference list of its rules;
- tags every rule `[error]`, `[warning]`, `[test]` or `[review]`;
- marks every rule's provenance: "platform convention", "project decision" or "diverges from the
  platform convention";
- points at `.editorconfig`, the analyzer configuration or a test where a tool decides the rule, instead
  of restating its settings.

Start a new rule document from [rule-template.md](../rule-template.md).
