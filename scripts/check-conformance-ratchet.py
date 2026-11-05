#!/usr/bin/env python3
"""Check the quantified conformance backlog in spec-snapshot/conformance-budget.json.

Two kinds of measurement feed this script:

``lint``
    Recomputes from the tree by importing ``lint-design-tokens.py``. Available with nothing but
    Python, so it runs in the static-checks job before any restore.

``headless``
    Read from ``artifacts/spec/*.json``, which the headless suite writes (the spec layers and the
    inventory layer both emit a ``metrics`` block). If the file is absent the metric is reported as
    *not measured* - never as passing, because a missing measurement is not evidence.

The script fails when a gap grew or a coverage figure shrank; it never fails because a gap shrank,
and it prints the whole table either way so the direction of travel is visible on every run.
"""

from __future__ import annotations

import argparse
import datetime as dt
import importlib.util
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
BUDGET_PATH = ROOT / "spec-snapshot" / "conformance-budget.json"

# Filled from the budget file on load so the merge rule above can be direction-aware.
_DIRECTIONS: dict[str, str] = {}
REPORT_PATH = ROOT / "artifacts" / "spec" / "conformance-ratchet.md"
JSON_PATH = ROOT / "artifacts" / "spec" / "conformance-ratchet.json"
SPEC_DIR = ROOT / "artifacts" / "spec"


def load_module(name: str, path: Path):
    """Import a script whose filename is not a valid module name.

    ``sys.modules`` has to be populated before ``exec_module`` because ``@dataclass`` resolves the
    defining module through it; the hyphen in ``lint-design-tokens.py`` rules out a plain import.
    """
    spec = importlib.util.spec_from_file_location(name, path)
    if spec is None or spec.loader is None:
        raise ImportError(f"cannot load {path}")
    module = importlib.util.module_from_spec(spec)
    sys.modules[name] = module
    spec.loader.exec_module(module)
    return module


def lint_metrics() -> dict[str, float]:
    """Recompute the L0 metrics by running the lint in-process (no report written)."""
    lint_module = load_module("md3_lint", ROOT / "scripts" / "lint-design-tokens.py")

    lint = lint_module.Lint()
    oracle = lint_module.load_json(lint_module.SPEC / "tokens.json", lint)
    policy = lint_module.load_json(lint_module.SPEC / "conformance-policy.json", lint) or {}
    declared, values = lint_module.collect_declarations(lint)
    usage = lint_module.rule_references(lint, declared)
    lint_module.rule_bare_hex(lint)
    lint_module.rule_orphans(lint, values, usage)
    lint_module.rule_includes(lint)
    lint_module.rule_hardcoded_geometry(lint)
    lint_module.rule_hardcoded_ui_text(lint)
    lint_module.rule_dual_declaration(lint)
    lint_module.rule_dangling_cs_key(lint, declared)
    gaps = lint_module.rule_token_values(lint, oracle, policy, values) if oracle else []

    tokens = (oracle or {}).get("tokens", [])
    metrics = {
        "errors": len(lint.errors),
        "warnings": len(lint.warnings),
        "bare_hex": lint.count("R1 bare-hex"),
        "unresolved_keys": lint.count("R2 unresolved-key"),
        "broken_includes": lint.count("R4 broken-include"),
        "dangling_cs_keys": lint.count("R10 dangling-cs-key"),
        "orphan_tokens": lint.count("R3 orphan-token"),
        "hardcoded_geometry": lint.count("R5 hardcoded-geometry"),
        "hardcoded_ui_text": lint.count("R8 hardcoded-ui-text"),
        "dual_declaration_keys": lint.metrics.get("dual_declaration_keys", 0),
        "high_confidence_gaps": sum(
            1 for g in gaps if g["confidence"] == "high" and "waived" not in g
        ),
        "declared_keys": len(declared),
        "oracle_tokens": len(tokens),
    }
    metrics["oracle_coverage_ratio"] = (
        round(metrics["oracle_tokens"] / metrics["declared_keys"], 4)
        if metrics["declared_keys"]
        else 0.0
    )
    metrics["oracle_families"] = len(
        {t["key"].split(".")[2] for t in tokens if t["key"].startswith("Md.Comp.")}
    )
    return metrics


def headless_metrics() -> dict[str, float]:
    """Collect every ``metrics`` block a headless run left in artifacts/spec/.

    Inventory blocks count too: a layer that lists the members of a gap (149 controls with no
    geometry case, 470 undeclared part names) is measuring that gap, and the length of the list is
    the metric. Without this the structural budgets could only ever be "not measured".

    A merged key takes the most pessimistic value across layers: for a gap metric that is the
    largest number any layer found (two layers measuring the same gap must not hide behind the
    smaller one), and for a coverage metric the smallest. Erring towards the worse number is the
    only safe direction for a budget.
    """
    collected: dict[str, float] = {}
    if not SPEC_DIR.is_dir():
        return collected
    for path in sorted(SPEC_DIR.glob("*.json")):
        if path.name == "conformance-ratchet.json":
            continue
        try:
            payload = json.loads(path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            continue
        blocks = [payload.get("metrics")]
        inventories = payload.get("inventory")
        if isinstance(inventories, dict):
            blocks.append(
                {key: len(value) for key, value in inventories.items() if isinstance(value, list)}
            )
        for block in blocks:
            if not isinstance(block, dict):
                continue
            for key, value in block.items():
                if not isinstance(value, (int, float)):
                    continue
                collected[key] = value if key not in collected else merge(collected[key], key, value)
    return collected


def merge(existing: float, key: str, value: float) -> float:
    """Combine two measurements of the same metric.

    Gap metrics are ceilings, so the larger number is the honest one; coverage metrics are floors,
    so the smaller number is. The direction is looked up from the budget rather than guessed, and
    an unknown key falls back to the pessimistic maximum.
    """
    try:
        direction = _DIRECTIONS.get(key, "lower")
    except NameError:  # pragma: no cover - only before the budget is loaded
        return max(existing, value)
    return min(existing, value) if direction == "higher" else max(existing, value)


def evaluate(budget: dict, available: dict[str, dict[str, float]], strict: bool) -> tuple[list[dict], int]:
    rows: list[dict] = []
    failures = 0
    for name, entry in budget.get("ratchet", {}).items():
        source = entry.get("source", "lint")
        direction = entry.get("direction", "lower")
        limit = entry.get("budget")
        measured = available.get(source, {}).get(name)

        if limit is None and measured is None:
            # Tracked without a ceiling and not measured in this run: there is nothing to compare,
            # so it is neither a pass nor a failure. --strict does not turn an unpinned metric into
            # a build failure; it only refuses to accept a pinned one with no measurement.
            status = "no-data"
        elif measured is None:
            status = "not-measured"
            if strict:
                failures += 1
        elif limit is None:
            # Tracked without a ceiling: the first run establishes the number a human then pins.
            status = "tracked"
        elif direction == "lower":
            status = "ok" if measured <= limit else "REGRESSED"
        else:
            status = "ok" if measured >= limit else "REGRESSED"

        if status == "REGRESSED":
            failures += 1

        rows.append(
            {
                "metric": name,
                "source": source,
                "direction": direction,
                "budget": limit,
                "measured": measured,
                "status": status,
                "why": entry.get("why", ""),
            }
        )
    return rows, failures


def render(rows: list[dict], failures: int, strict: bool) -> str:
    lines = [
        "# Conformance budget (quantified backlog)",
        "",
        f"- generated: {dt.datetime.now(dt.timezone.utc).isoformat(timespec='seconds')}",
        f"- strict (missing measurement fails): **{str(strict).lower()}**",
        f"- entries regressed or unmeasured: **{failures}**",
        "",
        "| metric | direction | budget | measured | status |",
        "| --- | --- | ---: | ---: | --- |",
    ]
    for row in rows:
        measured = "—" if row["measured"] is None else f"{row['measured']}"
        limit = "—" if row["budget"] is None else row["budget"]
        lines.append(
            f"| `{row['metric']}` | {row['direction']} | {limit} | {measured} | "
            f"{'**' + row['status'] + '**' if row['status'] not in ('ok', 'tracked', 'no-data') else row['status']} |"
        )

    regressed = [r for r in rows if r["status"] == "REGRESSED"]
    unmeasured = [r for r in rows if r["status"] == "not-measured"]
    tracked = [r for r in rows if r["status"] == "tracked"]
    nodata = [r for r in rows if r["status"] == "no-data"]
    if regressed:
        lines += ["", "## Regressions", ""]
        for row in regressed:
            lines.append(
                f"- `{row['metric']}` moved to **{row['measured']}** against a "
                f"{row['direction']} budget of {row['budget']}. {row['why']}"
            )
    if unmeasured:
        lines += ["", "## Not measured in this run", ""]
        for row in unmeasured:
            lines.append(
                f"- `{row['metric']}` needs a measurement from `{row['source']}` that this run "
                f"did not produce. {row['why']}"
            )
    if tracked:
        lines += ["", "## Tracked, no ceiling yet", "",
                  "Measured, printed, and deliberately not gated: pin a budget once a run has "
                  "established the number.", ""]
        for row in tracked:
            lines.append(f"- `{row['metric']}`: **{row['measured']}**. {row['why']}")
    if nodata:
        lines += ["", "## No data in this run", "",
                  "No ceiling and no measurement, so there is nothing to compare yet. The headless "
                  "run produces these.", ""]
        for row in nodata:
            lines.append(f"- `{row['metric']}` ({row['source']})")
    if not regressed and not unmeasured and not tracked and not nodata:
        lines += ["", "Every budgeted metric holds.", ""]
    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--strict", action="store_true",
                        help="treat a metric this run could not measure as a failure")
    parser.add_argument("--allow-not-measured", action="store_true",
                        help="exit 0 even when a headless metric is missing (used by the lint job)")
    args = parser.parse_args()

    if not BUDGET_PATH.exists():
        print(f"error: {BUDGET_PATH.relative_to(ROOT)} is missing", file=sys.stderr)
        return 2

    budget = json.loads(BUDGET_PATH.read_text(encoding="utf-8"))
    _DIRECTIONS.update(
        {name: entry.get("direction", "lower") for name, entry in budget.get("ratchet", {}).items()}
    )
    available = {"lint": lint_metrics(), "headless": headless_metrics()}

    strict = args.strict
    rows, failures = evaluate(budget, available, strict)
    if args.allow_not_measured:
        failures = sum(1 for r in rows if r["status"] == "REGRESSED")

    REPORT_PATH.parent.mkdir(parents=True, exist_ok=True)
    REPORT_PATH.write_text(render(rows, failures, strict), encoding="utf-8")
    JSON_PATH.write_text(
        json.dumps(
            {
                "schema_version": 1,
                "generated_at": dt.datetime.now(dt.timezone.utc).isoformat(),
                "strict": strict,
                "failures": failures,
                "available": available,
                "rows": rows,
            },
            indent=2,
            ensure_ascii=False,
        )
        + "\n",
        encoding="utf-8",
    )

    for row in rows:
        measured = "—" if row["measured"] is None else row["measured"]
        print(f"[{row['status']:>12}] {row['metric']:<26} {row['direction']:<6} "
              f"budget={(row['budget'] if row['budget'] is not None else '—')!s:<8} measured={measured}")

    print(f"\nreport: {REPORT_PATH.relative_to(ROOT)}")
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
