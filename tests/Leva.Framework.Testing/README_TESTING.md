# Leva.Framework.Testing

`Leva.Framework.Testing` provides shared test-only helpers used by the framework test projects.

## Purpose and dependencies

Testing exists to keep common assertions and test utilities out of individual test projects. It depends on `Leva.Framework.Core` and xUnit because it is used only by test projects. Runtime libraries must not depend on Testing.

```text
Leva.Framework.Testing
  -> Leva.Framework.Core
  -> xUnit

Framework test projects
  -> Leva.Framework.Testing
```

## Project overview

Testing is intentionally small. It should contain reusable assertion helpers and test utilities that are shared by multiple test projects. It should not contain production fakes, provider implementations, or application-specific test setup.

## Files and classes

### Result assertions

`ResultAssert` - Provides shared assertions for successful `Result` and `Result<T>` values.
