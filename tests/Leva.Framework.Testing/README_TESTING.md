# Leva.Framework.Testing

`Leva.Framework.Testing` provides shared test-only helpers used by the framework test projects.

## Purpose and dependencies

Testing exists to keep common assertions and test utilities out of individual test projects. It depends on `Leva.Framework.Core` and does not depend on xUnit, NUnit, MSTest, or any other test framework. Runtime libraries must not depend on Testing.

```text
Leva.Framework.Testing
  -> Leva.Framework.Core

Framework test projects
  -> Leva.Framework.Testing
  -> selected test runner
```

## Project overview

Testing is intentionally small. It contains reusable assertion helpers and test utilities that are shared by multiple test projects. Test failures are reported through ordinary exceptions so the helpers can be used with any test runner.

It should not contain production fakes, provider implementations, or application-specific test setup. Production test doubles belong in `Leva.Framework.Fakes`; test-runner-specific helpers should go into a separate adapter library only if they become necessary.

## Files and classes

### Result assertions

`ResultAssert` - Provides shared assertions for successful `Result` and `Result<T>` values.
`ResultAssertionException` - Exception thrown when a shared result assertion fails.
