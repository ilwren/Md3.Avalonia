# Historical verification report — retracted as compliance evidence

> **Status: INVALID / NON-CERTIFYING (retracted 2026-10-02).**
>
> This file formerly claimed “100% Material Design 3 and Flutter compliant.” That conclusion was
> unsupported and must not be quoted in documentation, release notes, package metadata, or CI.

## Why the former conclusion was invalid

- The former Python runner recorded many checks as unconditional passes; it did not inspect the
  named control's runtime geometry or behavior.
- It tested standalone color and easing arithmetic rather than the brushes and transitions actually
  used by rendered controls.
- It treated source inventory as proof that controls compiled, had registered themes, and worked on
  every platform.
- Its generated JSON contained 123 records while this report claimed 128 passing tests.
- Its sub-millisecond execution could not have represented Headless, Android, rendering, keyboard,
  assistive-technology, or Flutter differential testing.

## Replacement evidence contract

- The upstream baseline is pinned in [`../spec-baseline-manifest.json`](../spec-baseline-manifest.json).
- Requirement IDs, variants, platforms, named evidence, and unexecuted statuses are tracked in
  [`../spec-test-matrix.json`](../spec-test-matrix.json).
- Flutter parity boundaries are explicit in
  [`../FLUTTER_PARITY_STATUS.md`](../FLUTTER_PARITY_STATUS.md).
- `scripts/run-spec-verification.py` now labels source checks, real `dotnet test` execution, and
  Android/device evidence separately. Missing tools or platform jobs are reported as `not_run`, not
  as passes.
- The latest machine-readable result is
  [`test-spec-execution-results.json`](test-spec-execution-results.json). It is repository evidence,
  **not** a Material or Flutter certification.
- The detailed audit and remaining risk register are in
  [`material-design-full-component-audit-2026-10-02.md`](material-design-full-component-audit-2026-10-02.md).

No aggregate compliance percentage is published until each requirement is traceable to a pinned
source clause, a control/variant/platform matrix entry, and reproducible behavioral evidence.
