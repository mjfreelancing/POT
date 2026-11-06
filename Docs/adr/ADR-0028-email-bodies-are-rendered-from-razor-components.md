---
id: ADR-0028
title: Email bodies are rendered from Razor components
status: accepted
date: 2025-10-18
area: backend
---

# ADR-0028 (backfilled): Email bodies are rendered from Razor components

**Summary:** In the context of transactional email sent as both HTML and plain text, facing the need to
combine styled markup with dynamic content from each message, we decided to render the HTML body from a
Razor component and the plain-text body from a template with placeholders, to achieve styled, data-driven
HTML written as markup and rendered by the framework, accepting that values reach both templates by name
at run time rather than through a typed contract.

## Context and Problem Statement

The system supports both plain-text and HTML email, so every message is sent with both bodies. The HTML
body needs styling and dynamic content — a name, a code, a site — filled from the message. Razor components were a natural fit for that: they combine markup and styling with
code, so dynamic content is written where it appears, and the framework renders them.

Each message has two templates in `Pot.RazorComponents`: a Razor component for the HTML body, rendered to
a string outside any web request, and a plain-text file with `{Property}` placeholders for the text body.

## Decision Drivers

- Both an HTML and a plain-text body for every message.
- HTML that combines styling with content from the message, written as markup rather than built in code.
- No templating engine beyond what the framework provides.

## Considered Options

1. A Razor component for the HTML body, and a plain-text template with placeholders.
2. Bodies built as strings in code.
3. A third-party templating engine for both bodies.

## Decision Outcome

Chosen option: 1. The subject is a fixed string per email type, the HTML body is rendered from the type's
Razor component by `RazorComponentRenderer`, and the plain-text body is the type's text template with its
placeholders replaced. Both are filled from the message's properties by name at send time.

### Consequences

- Good, because the HTML layout and its styling are written as markup, with the dynamic content beside
  them, and rendered by the framework.
- Good, because a message renders outside a web request, so the worker that sends it needs no HTTP
  context.
- Bad, because the values reach the templates by property name at run time, not through a typed contract:
  a value the component does not declare fails the render, and the message is then logged and dropped,
  while a placeholder the text template names but the values lack is sent as written. Nothing checks this
  at build time.
- Bad, because each message has two templates, written in two mechanisms, that must say the same thing;
  nothing checks that they do.

### Confirmation

- Test: `EmailGreetingFixture` renders several of the HTML and plain-text templates with the values the
  sender passes.
- Gap: no test renders every email type, so a type whose values and templates disagree is found when it
  is sent.
- Review: a new email type adds both a Razor component and a plain-text template in `Pot.RazorComponents`.

## Pros and Cons of the Options

### Razor component and text template (chosen)

- Good: styled, dynamic HTML as markup, rendered by the framework.
- Bad: values bound by name at run time, and two templates per message.

### Bodies built in code

- Good: values are typed, and nothing is resolved at run time.
- Bad: markup and styling assembled in strings, which is hard to read and to change.

### Third-party templating engine

- Good: one mechanism for both bodies.
- Bad: another dependency and template language, with no typing over the values either.

## More Information

- How a message is queued and sent is [ADR-0027](ADR-0027-email-is-queued-in-process.md).
