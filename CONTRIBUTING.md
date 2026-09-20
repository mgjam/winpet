# Development Principles

We do not just write code to deliver the feature and call it a day. We write high-quality code that is maintainable, testable, and makes sense months later. Fundamental principles to follow:
- DRY - do not repeat yourself.
- Open-closed principle - code is open for extension, closed for modification. Beautiful design rarely involves rewrite of existing code to add a new feature.
- Unit-testability - Maintainable and testable units of work.
- SRP - single responsibility principle - unit of work should do one thing, and do it well.
- DDD - domain driven design - we love domain models over glue classes and myriad of orchestrators. Dependency tree of models through constructors and methods should naturally dictate code flow.
- Onion design - domain models are at the core, outer layers such as I/O are not part of the core. Thus, it is easy to change the database implementation for example.
  - There are limits to this rule, as it is unlikely we will for example exchange the GDI+ graphics.

## C# source organization

- Give each top-level type its own file named after the type, including the `I` prefix for interfaces (`IPet.cs`). Keep implementation-only nested types with their owner.
- Match namespaces to project-relative folders: `Domain/IPet.cs` uses `WinPet.Domain`; `src/WinPet.Tests/Domain/PhysicsChecks.cs` uses `WinPet.Tests.Domain`.
- Group code by responsibility: shared pet models in `Domain`, Windows integration in `Desktop`, bitmap composition in `Rendering`, pet-specific artwork in `Pets/<Pet>`, and gallery tooling in `Previewing`.
- Keep checks and test doubles in `src/WinPet.Tests`, mirroring the application folders. Generated visual assets belong in `previews/`, separate from preview tooling source.
- Keep anatomy and expression out of `Domain`. `IPet` defines behavior; `IPetVisual` renders a neutral `PetFrame`. A face, eyelids, mouth, limbs, wheels, and their animation belong to a pet's visual implementation. Use optional rendering helpers rather than mandatory anatomy flags or fields on the shared contract.
