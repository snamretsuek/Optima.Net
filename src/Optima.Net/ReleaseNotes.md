# Optima.Net Release Notes


## v1.0.8

Result Semantics Hardening (Backward Compatible)

This release strengthens the internal invariants of Result\<T\> and Result\<T, TError\> while remaining fully backward compatible.
No existing contracts were loosened; several were made explicit and consistently enforced across all entry points.

What changed

Failure semantics were clarified and hardened

Failures may exist with or without a value.

Canonical form is Fail(value, error).

Legacy form Fail(error) remains supported for backward compatibility.

Illegal states are now prevented from leaking

Accessing Value on a failure that does not carry a value now fails loudly.

This prevents silent propagation of default(T).

Invariant enforcement was aligned across Result surfaces

Result\<T\> and Result\<T, TError\> now enforce the same domain rules.

Public façades no longer rely on indirect enforcement only.

Exception signaling was improved

Invariant violations now throw domain-specific exceptions
(for example NullValueException) instead of generic runtime exceptions.

Exception types now clearly communicate what went wrong, not just that something went wrong.

Backward compatibility

No public APIs were removed or renamed.

Existing code using Fail(error) continues to compile and run.

Behavioral changes only affect previously unsafe or undefined usage
(for example, accessing Value when none exists).

Guidance

Prefer Fail(value, error) to preserve state on failure.

Legacy overloads Fail(error) are retained but marked as obsolete to guide migration.

## v1.0.7
Added support for Result<Unit> paradigm.

---

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
