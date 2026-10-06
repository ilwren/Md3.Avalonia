# Golden image baselines (layer L3)

Committed reference renders for `MdVisualGoldenTests`. One PNG per scene, named after the scene.

This directory is intentionally empty on first checkout. The harness never adopts its own output
as truth: when a baseline is missing, the candidate is written to
`artifacts/spec/baselines-new/<scene>.png` and the run **reports** rather than fails. Review the
image, then copy it here to arm the golden.

When an armed golden moves, the candidate and a magenta diff mask are written to the same
`artifacts/spec/baselines-new/` directory so the change can be reviewed without flicking between
files.

Comparison thresholds and the baseline OS live in `spec-snapshot/conformance-policy.json` under
`visual`. Scenes containing text are only gated on the baseline OS, because Skia's font
rasterisation differs between platform back ends; text-free scenes are gated everywhere.

See [`docs/SPEC_VERIFICATION.md`](../../../../docs/SPEC_VERIFICATION.md).
