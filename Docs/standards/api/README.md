# Rule documents: API contract

Binding conventions for the HTTP contract that both the server and the client must honour. The generated
contract at `contracts/openapi/v1.json` wins for shape; a rule document here wins for intent.

Each rule document in this folder:

- stays within about 800 lines and opens with a quick-reference list of its rules;
- tags every rule `[error]`, `[warning]`, `[test]` or `[review]`;
- marks every rule's provenance: "platform convention", "project decision" or "diverges from the
  platform convention";
- never restates the generated contract at `contracts/openapi/v1.json`; that document wins for shape,
  a rule document wins for intent.

A convention that is really a decision — the error body, concurrency, authorisation — stays in its own
ADR and is referenced here rather than restated.

Start a new rule document from [rule-template.md](../rule-template.md).
