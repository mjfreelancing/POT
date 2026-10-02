---
description: "AllOverIt helper, validator and exception logging patterns"
applyTo: "Source/Server/**/*.cs"
---


# AllOverIt and logging patterns

Living document: add new patterns or corrections here. When it grows, split it into focused rule files.

These apply when `AllOverIt.Extensions` is available (via the `AllOverIt` NuGet package).

## Strings and null guards

- Use `value.IsNullOrEmpty()` instead of `string.IsNullOrEmpty(value)`.
- Use `value.IsNotNullOrEmpty()` instead of `!string.IsNullOrEmpty(value)` and `!string.IsNullOrWhiteSpace(value)`.
- Guard runtime-nullable arguments with `filePath.WhenNotNull();`, not a manual `ArgumentNullException` throw.
- Constructor parameters resolved by DI need no null guard: the container throws if a dependency cannot be resolved. Keep `WhenNotNull()` on a constructor parameter only when the class is also created by a factory that may pass `null`.

## Validators (`AllOverIt.Validation`)

- Inherit `ValidatorBase<T>`, not `AbstractValidator<T>`.
- Call `DisablePropertyNameSplitting()` in the static constructor.
- Use `IsNotEmpty()` for required strings, not `NotEmpty()`.

```csharp
internal sealed class MyValidator : ValidatorBase<MyModel>
{
    static MyValidator()
    {
        DisablePropertyNameSplitting();
    }

    public MyValidator()
    {
        RuleFor(model => model.Name).IsNotEmpty();
    }
}
```

## Dependent-state checks (`Throw<TException>`, namespace `AllOverIt.Assertion`)

Use `Throw<TException>` to validate state that should have been established before a method runs (for example properties set by DI or code-behind wiring). It is not for constructor parameters.

| Method | Throws when |
| --- | --- |
| `When(condition)` | condition is `true` |
| `WhenNot(condition)` | condition is `false` |
| `WhenNull(object)` | object is `null` |
| `WhenNotNull(object)` | object is not `null` |
| `WhenNullOrEmpty(string)` | string is `null` or empty |
| `WhenNotNullOrEmpty(string)` | string is not `null` or empty |

Each has overloads taking up to 4 exception constructor arguments, as values or as `Func<>` for deferred evaluation.

```csharp
Throw<InvalidOperationException>.WhenNull(
    SettingsEditorViewModel,
    $"The {nameof(SettingsEditorViewModel)} has not been set.");

Throw<InvalidOperationException>.WhenNull(someObject, () => $"Unexpected null for {nameof(someObject)}.");
```

## Exception logging

- Prefer `logger.LogError(exception, "Failed to process the request.");` so the logging configuration decides how much detail to render.
- Avoid `exception.ToString()` in general or production code: it forces the full stack trace into the message and can leak file paths, type names and call chains. Reserve it for internal tools or fatal startup failures.
- `logger.LogError("{ErrorMessage}", exception.Message)` is acceptable only for well-known, shallow exceptions (validation errors, configuration binding failures).
