---
id: ADR-0055
title: Services are registered by scanning each layer for its marker interface
status: accepted
date: 2025-01-26
area: backend
---

# ADR-0055: Services are registered by scanning each layer for its marker interface

**Summary:** In the context of a layered server that adds services in every feature, facing a
composition root that would otherwise list every registration by hand, we decided to register services
by scanning each layer's assembly for the interfaces that extend a lifetime marker, to achieve a
composition root that cannot forget a new service, accepting that a service's lifetime is declared far
from the code that composes the layer.

## Context and Problem Statement

`Pot.AspNetCore` composes `Pot.App`, `Pot.Data` and `Pot.EmailSender`, and each of those layers adds
services in every feature it carries: 107 interfaces extend `IPotScopedDependency` and 12 extend
`IPotSingletonDependency`. Registering each one by hand means a composition-root method that names them
all, and a new service that nobody adds to it fails only when the path using it is exercised, as
"unable to resolve" at runtime rather than at build time.

The scanning is in the code rather than in a document. Each layer declares one empty subclass of
`ServiceRegistrarBase` to mark its assembly (`Pot.App/DependencyRegistrar.cs:6`,
`Pot.Data/PotDataRegistrar.cs:6`, `Pot.AspNetCore/DependencyRegistrar.cs:6`,
`Pot.EmailSender/DependencyRegistrar.cs:6`), and the layer's extension method calls
`AutoRegisterScoped` / `AutoRegisterSingleton` from `AllOverIt.DependencyInjection` with a marker as the
service-type filter (`Pot.App/Extensions/ServiceCollectionExtensions.cs:14,31`;
`Pot.Data/Extensions/ServiceCollectionExtensions.cs:11,20`;
`Pot.AspNetCore/Extensions/WebApplicationBuilderExtensions.cs:457,473`;
`Pot.EmailSender/Extensions/ServiceCollectionExtensions.cs:12`). The markers are empty interfaces in
`Pot.Shared/DependencyInjection/`. No class implements a marker directly, so every registered service is
a pair of interface and implementation.

The decision was taken while the architecture was being worked out, in the dependency-registration
refactor of January 2025. The reason on record is convenience: a marker costs one line where manual
registration costs an entry per service. It is accepted for as long as the scan does not cost startup
time worth counting.

## Decision Drivers

- A composition root that lists every service is a list that goes stale silently.
- The layers are separate assemblies, so each one can own its own scanning.
- Registration has no per-request cost: the scan runs once, while the host is built.

## Considered Options

1. Scan each layer for interfaces that extend a lifetime marker (status quo).
2. Register each service explicitly in its layer's composition-root method.
3. Scan every interface in the layer, with no marker to opt in.

## Decision Outcome

Chosen option: 1, because it removes a step that is easy to forget and costs nothing per request. The
record does not show that the alternatives were weighed when the decision was taken; the reason on
record is convenience, and the condition on it is startup time.

The rules it imposes:

- A service is registered because its interface extends `IPotScopedDependency` or
  `IPotSingletonDependency`. The marker goes on the interface; a class that implements a marker is not
  registered, because the scan registers interface-to-implementation pairs.
- Scoped is the default and singleton is the marked exception. There is no transient marker, so a
  transient registration is explicit and states why.
- A layer that adds services declares its own `ServiceRegistrarBase` subclass and scans from its own
  extension method, so a layer boundary is visible in the composition rather than in a list.
- The filters exist to keep non-services out of the scan — the open generic validators the layers also
  carry, and the markers themselves. Registering a service explicitly beside a scanning call is not a
  style to mix in; explicit registration is for what the scan cannot express: a framework abstraction
  that must not carry a Pot marker, a type consumed as a concrete class such as middleware, or a
  registration built by a factory delegate.

### Consequences

- Good, because a new service cannot be forgotten: extending the marker is the registration.
- Good, because each layer composes itself, so the scan and the layer's own extension method are the
  same boundary.
- Good, because there is no per-request cost; the scan runs while the host is built.
- Bad, because a service's lifetime is decided in the file that declares its interface, away from the
  code that composes the layer, so a reader of the class alone does not see how it is registered.
- Bad, because an interface that does not extend a marker is silently unregistered, and the failure
  arrives as a resolution error at first use rather than as a build error.
- Bad, because it requires an interface on every service, including one with a single implementation
  and nothing that substitutes it.
- Bad, because scanning reflects over each layer's assembly at startup, which the decision accepts only
  while startup time is not a concern.

### Confirmation

- Test: `Pot.Data.Tests/Extensions/ServiceCollectionExtensionsFixture` asserts a scan registers the
  repositories it is meant to and leaves the marker itself unregistered.
- Test: the hosted suite boots `Program` through `WebApplicationFactory<Program>` for every fixture, so
  a service the scan misses fails in the first hosted test that reaches it.
- Instruction: `.github/instructions/server.instructions.md` (Dependency injection).
- Review: a service interface with no marker is a defect, and so is an explicit registration beside a
  scanning call that does not fall into one of the three stated exceptions.

## Pros and Cons of the Options

### Scanning for a marker

- Good: registration cannot be forgotten, and each layer composes itself.
- Bad: the lifetime is declared away from the composition, and a missing marker is silent.

### Explicit registration

- Good: every registration is visible in one place, and there is no startup scan.
- Bad: over a hundred entries to keep in step, and a missed one fails only at runtime.

### Scanning every interface, with no marker

- Good: nothing can be forgotten and nothing has to be declared.
- Bad: it registers abstractions that are not services, so exclusion lists grow with the code and the
  rule ends up stated by exception instead of by declaration.

## More Information

- The framework is AllOverIt's `ServiceRegistrarBase` and `AutoRegister*` extensions; the filters exist
  because the layers also carry `IValidator<>` and `ValidatorBase<>`.
- The interfaces are also the test seam: the server test projects make 362 `Substitute.For<` calls
  against them.
