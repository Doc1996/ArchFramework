# Leva.Framework.Presentation

`Leva.Framework.Presentation` defines the provider-neutral presentation vocabulary of ArchFramework. It contains compact state values and host-facing contracts for views, commands, forms, dialogs, navigation, and user-facing messages.

Presentation exists so applications can model common UI-facing state consistently before a concrete host renders it. It contains presentation concepts, not Blazor components, CSS, JavaScript, routing hosts, mobile controls, desktop controls, notification providers, storage, identity, engine execution, or application-specific screens. `Leva.Framework.Presentation` depends on `Leva.Framework.Core` so presentation messages can be created from structured framework errors. Core must not depend on Presentation. Engine, Storage, Identity, Execution, Notifications, provider implementations, and application domain projects should not depend on Presentation directly.

```text
Leva.Framework.Presentation
  -> Leva.Framework.Core
```

## Project overview

Presentation is intentionally small and UI-neutral. A type belongs here only when it represents reusable state or host interaction that many presentation technologies can share safely. This includes view state such as loading, ready, empty, and failed; command state such as ready, running, succeeded, failed, and disabled; form and field state; dialog requests and results; transient user-facing messages; and simple navigation capability.

Presentation does not decide how UI is rendered. A `MessageEntry` may become a toast in Blazor, a snackbar in mobile UI, an alert banner in a web page, a status line in a desktop application, or an inspected entry in tests. A `DialogRequest` may become a modal, native dialog, bottom sheet, or console prompt. `INavigation` only defines the navigation capability; the host decides how paths are handled.

Presentation also avoids binding application code to a specific UI framework. It does not know about Blazor `EditContext`, ASP.NET routing, JavaScript interop, CSS classes, MAUI, Avalonia, WPF, or any third-party component library. Concrete hosts and application presentation projects adapt these neutral values to the actual UI.

## Files and classes

### Commands

`CommandStatus` - Describes the current state of one user-facing command or action: ready, running, succeeded, failed, or disabled.
`CommandState` - Stores command state, optional label, optional message, helper booleans, and factory methods for common command states.

### Views

`ViewStatus` - Describes the loading state of a screen, page, or section: idle, loading, ready, empty, or failed.
`ViewState<T>` - Stores view state, optional loaded value, optional message, helper booleans, and factory methods for common view states.

### Forms

`FormStatus` - Describes the state of one form: clean, modified, submitting, submitted, invalid, or failed.
`FormState` - Stores form state and form-level messages with helper booleans and factory methods.
`FieldState<T>` - Stores one field value, touched/modified flags, field-level messages, and update helpers.

### Dialogs

`DialogRequest` - Describes one dialog request with message, optional title, accept text, and optional cancel text.
`DialogResult` - Represents whether the user accepted or cancelled a dialog.
`IDialogService` - Shows dialogs through a concrete presentation technology.

### Messages

`MessageLevel` - Describes message severity for user-facing presentation messages.
`MessageEntry` - Represents one user-facing presentation message with text, level, optional title, optional code, and optional target. It can also be created from a Core `Error`.
`IMessageSink` - Receives transient presentation messages without deciding how a host renders them.
`MemoryMessageSink` - Stores presentation message entries in memory for inspection, demos, or tests.
`NullMessageSink` - Message sink implementation that intentionally ignores presentation messages.

### Navigation

`INavigation` - Provides a presentation-level navigation capability without binding application code to a specific UI host.
