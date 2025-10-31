# Optima.Net Release Notes

## v1.0.5
Added reference links to README for easier navigation and adoption.

---

## v1.0.4
Introduced `Result<T, TError>`  
Structured dual-channel result handling for richer domain signaling without exceptions.

### Highlights
- Strongly typed success and error paths
- Clearer intent when domain errors differ from system failures

---

## v1.0.3
Fixed exception type thrown by `Result<T>` on invalid usage.

### Issue
Incorrect exception was being thrown internally when accessing invalid state.

### Fix
Corrected to throw the appropriate exception type to maintain contract clarity and correctness.

---

## v1.0.2
Documentation update.

- Improved README examples and usage notes
- Clarified intent and semantics of Optional/Result primitives

---

## v1.0.1
Introduced `Result<T>`  
Explicit success / failure return type to replace ambiguous nulls and control–flow exceptions.

### Features
- `Result.Ok(value)`
- `Result.Fail(error)`
- Value & error accessors
- Guarding against invalid state access
- Functional matching helpers

---

## v1.0.0
Initial release  
Added `Optional<T>` to model optional values safely and explicitly.

### Highlights
- Avoids null–ambiguity in public APIs
- Clear representation of "may or may not exist"
- Safer and more expressive design than nullable return values

---

> **Optima.Net** — explicit value modeling for serious .NET systems.  
Nulls lie. Exceptions are exceptional. Truth should be modeled.
