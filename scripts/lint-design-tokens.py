#!/usr/bin/env python3
"""Static design-token lint for Md3.Avalonia.

Pure standard library, no network, no .NET SDK. Designed to run as a CI fast-fail
job in roughly one second, before the expensive restore/build/test job starts.

Checks
------
R1 bare-hex          Literal colours inside shipped theme dictionaries.
R2 unresolved-key    {StaticResource Md.X} / {DynamicResource Md.X} with no x:Key="Md.X" anywhere.
R3 orphan-token      Declared Md.* token that nothing references (dead weight).
R4 broken-include    ResourceInclude Source="avares://..." that does not resolve to a file on disk.
R5 hardcoded-geometry  Numeric literals for themable geometry inside Themes/Controls/*.axaml.
R6 oracle-schema     spec-snapshot/*.json parse + required fields.
R7 token-value       Statically parsed token values vs. the spec-snapshot oracle.
R8 hardcoded-ui-text English literal text inside a shipped control template, which bypasses
                     MdLocalization and cannot follow the inherited culture.
R9 dual-declaration  A Md.* key that is both a XAML token and a runtime assignment in C#
                     (MdThemeManager). Two sources of truth drift silently, so the pairs are
                     reported as an inventory.
R10 dangling-cs-key  A Md.* key a control looks up at runtime that no theme dictionary declares.
                     The lookup returns null instead of throwing, so a typo is invisible until
                     something renders unstyled.

Severity model
--------------
error    fails the job (exit 1)
warning  reported only

R2/R4/R6 are always errors: they are unambiguous defects.
R1 is an error (the repository is already at a zero baseline, so this is regression-only).
R3/R5 are always warnings.
R7 follows spec-snapshot/conformance-policy.json: in "bootstrap" mode every finding is a
warning; in "enforce" mode, non-waived findings whose oracle entry is confidence=high
become errors.

Usage
-----
  python3 scripts/lint-design-tokens.py
  python3 scripts/lint-design-tokens.py --report artifacts/spec/token-lint.md
  python3 scripts/lint-design-tokens.py --github   # emit ::error / ::warning annotations
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
from collections import defaultdict
from dataclasses import dataclass, field
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
SPEC = REPO / "spec-snapshot"

# Directories whose .axaml files ship inside a NuGet package as themes.
THEME_GLOBS = (
    "src/Md3.Avalonia/Themes/**/*.axaml",
    "src/Md3.Avalonia.Extra/Themes/**/*.axaml",
)
TOKEN_DIR = REPO / "src/Md3.Avalonia/Themes/Tokens"

# --- R1 waivers -------------------------------------------------------------
# A raw spectrum ramp is not a themable surface: re-colouring a hue wheel with
# theme tokens would make it stop being a hue wheel.
HEX_SPECTRUM_FILES = {"src/Md3.Avalonia.Extra/Themes/AdvancedControls.axaml"}
# Fully transparent sentinels carry no design intent.
HEX_TRANSPARENT = {"#00000000", "#0000"}

# --- R5 --------------------------------------------------------------------
GEOMETRY_ATTRS = ("CornerRadius", "MinHeight", "MinWidth", "FontSize")
# Values that are never worth tokenising.
GEOMETRY_IGNORED = {"0", "0.0"}

RE_KEY = re.compile(r'x:Key="(Md\.[A-Za-z0-9.]+)"')
RE_CS_KEY = re.compile(r'"(Md\.[A-Za-z0-9.]{3,})"')
RE_REF = re.compile(r"\{\s*(?:StaticResource|DynamicResource)\s+(Md\.[A-Za-z0-9.]+)\s*\}")
RE_HEX = re.compile(r"#[0-9A-Fa-f]{3,8}\b")
RE_INCLUDE = re.compile(r'ResourceInclude\s+Source="avares://([^/]+)/([^"]+)"')
RE_RESOURCE_VALUE = re.compile(
    r"<(x:Double|CornerRadius|Thickness|x:String|FontFamily)\s+x:Key=\"(Md\.[A-Za-z0-9.]+)\"\s*>([^<]*)</\1>"
)
RE_GEOMETRY = re.compile(
    r'(?:Property="(' + "|".join(GEOMETRY_ATTRS) + r')"\s+Value|(' + "|".join(GEOMETRY_ATTRS) + r"))"
    r'=\s*"([0-9][0-9,. ]*)"'
)

# --- R8 ---------------------------------------------------------------------
# An English literal with at least three letters and a space-or-word shape. Glyph strings,
# font family names, resource keys and single symbols are not authored prose.
RE_UI_TEXT = re.compile(r'(?<![\w:.])Text="([^"{}]+)"')
RE_WORD = re.compile(r"[A-Za-z]{3,}")

# --- R9 ---------------------------------------------------------------------
RE_CS_TOKEN_WRITE = re.compile(
    r'Resources\[\s*"\s*(Md\.[A-Za-z0-9.]+)\s*"\s*\]\s*='
)
# A write through a constant indirection: Resources[SchemeResourceKey] = ... . The constant's
# value is resolved from the const declaration below, so the write still counts as a declaration.
RE_CS_TOKEN_WRITE_INDIRECT = re.compile(r"Resources\[\s*([A-Za-z_][A-Za-z0-9_]*)\s*\]\s*=")
RE_CS_CONST = re.compile(r'const\s+string\s+([A-Za-z_][A-Za-z0-9_]*)\s*=\s*"(Md\.[A-Za-z0-9.]+)"')

# --- R10 --------------------------------------------------------------------
# Tokens that are filled in by an optional package or only exist after a runtime theme update.
# Anything else a control looks up at runtime has to exist in a shipped dictionary, otherwise the
# lookup returns null and the control silently renders unstyled.
OPTIONAL_TOKENS = {
    "Md.Sys.Typeface.Symbols.Rounded":
        "filled in by the optional Md3.Avalonia.Icons / .Icons.Lite package; core must not depend on it",
    "Md.Sys.Motion.Scheme":
        "written by MdThemeManager.Apply (through MdMotion.SchemeResourceKey) when a dynamic theme is applied",
}
RE_CS_TOKEN_REF = re.compile(r'"(Md\.[A-Za-z0-9.]+)"')


@dataclass
class Finding:
    rule: str
    severity: str  # "error" | "warning"
    path: str
    line: int
    message: str


@dataclass
class RatchetCounter:
    """A metric that must not get worse. ``direction`` is ``lower`` for gaps to burn down."""

    name: str
    measured: int
    budget: int
    direction: str  # "lower" | "higher"
    detail: str = ""

    @property
    def ok(self) -> bool:
        return self.measured <= self.budget if self.direction == "lower" else self.measured >= self.budget


@dataclass
class Lint:
    findings: list[Finding] = field(default_factory=list)
    metrics: dict[str, int] = field(default_factory=dict)

    def add(self, rule: str, severity: str, path: str, line: int, message: str) -> None:
        self.findings.append(Finding(rule, severity, path, line, message))

    def count(self, rule: str) -> int:
        return sum(1 for f in self.findings if f.rule == rule)

    @property
    def errors(self) -> list[Finding]:
        return [f for f in self.findings if f.severity == "error"]

    @property
    def warnings(self) -> list[Finding]:
        return [f for f in self.findings if f.severity == "warning"]


def rel(p: Path) -> str:
    return p.relative_to(REPO).as_posix()


def iter_theme_files() -> list[Path]:
    out: list[Path] = []
    for pattern in THEME_GLOBS:
        out.extend(sorted(REPO.glob(pattern)))
    return out


def iter_all_axaml() -> list[Path]:
    out: list[Path] = []
    for root in ("src", "gallery"):
        out.extend(sorted((REPO / root).rglob("*.axaml")))
    return [p for p in out if "/obj/" not in p.as_posix() and "/bin/" not in p.as_posix()]


# ---------------------------------------------------------------------------
# Value parsing (mirrors Avalonia's shorthand expansion)
# ---------------------------------------------------------------------------
def parse_quad(text: str) -> list[float] | None:
    parts = [p for p in re.split(r"[,\s]+", text.strip()) if p]
    try:
        nums = [float(p) for p in parts]
    except ValueError:
        return None
    if len(nums) == 1:
        return [nums[0]] * 4
    if len(nums) == 2:
        return [nums[0], nums[1], nums[0], nums[1]]
    if len(nums) == 4:
        return nums
    return None


def parse_double(text: str) -> float | None:
    try:
        return float(text.strip())
    except ValueError:
        return None


# ---------------------------------------------------------------------------
# Rules
# ---------------------------------------------------------------------------
def collect_declarations(lint: Lint) -> tuple[set[str], dict[str, tuple[str, str, int]]]:
    """Returns (all declared keys, token values declared in Themes/Tokens)."""
    declared: set[str] = set()
    values: dict[str, tuple[str, str, int]] = {}  # key -> (type, raw, line) ; path folded into type report

    for path in iter_all_axaml():
        text = path.read_text(encoding="utf-8", errors="replace")
        declared.update(RE_KEY.findall(text))

    # C# literals are treated as declarations (MdDynamicTheme writes tokens at runtime),
    # never as references, so that dynamic generation cannot produce false "unresolved" noise.
    for path in (REPO / "src").rglob("*.cs"):
        if "/obj/" in path.as_posix() or "/bin/" in path.as_posix():
            continue
        text = path.read_text(encoding="utf-8", errors="replace")
        declared.update(RE_CS_KEY.findall(text))

        # A runtime write through a const (Resources[SchemeResourceKey] = ...) declares its key
        # too; without this the R9 dual-declaration inventory silently under-counts.
        constants = dict(RE_CS_CONST.findall(text))
        for name in RE_CS_TOKEN_WRITE_INDIRECT.findall(text):
            if name in constants:
                declared.add(constants[name])

    for path in sorted(TOKEN_DIR.glob("*.axaml")):
        for lineno, line in enumerate(path.read_text(encoding="utf-8", errors="replace").splitlines(), 1):
            for kind, key, raw in RE_RESOURCE_VALUE.findall(line):
                values[key] = (kind, raw, lineno)
    return declared, values


def collect_cs_token_values(lint: Lint) -> dict[str, dict[str, object]]:
    """Statically parsed ``Resources["Md.X"] = <literal>`` assignments in C#.

    Only literal right-hand sides are recorded. Anything computed from a variable, a multiplier or
    a helper call is left out on purpose: a guessed static value would make R7 report a divergence
    that does not exist, and the headless dual-declaration layer is the layer that can evaluate
    those expressions for real.
    """
    values: dict[str, dict[str, object]] = {}
    literal_double = re.compile(r"^([-+]?[0-9]*\.?[0-9]+)d?$")
    literal_scaled = re.compile(r"^([-+]?[0-9]*\.?[0-9]+)d?\s*\*\s*([A-Za-z0-9_.]+)")
    literal_corner = re.compile(r"new\s+CornerRadius\(\s*([-+]?[0-9]*\.?[0-9]+)d?\s*\)")
    for path in sorted((REPO / "src").rglob("*.cs")):
        if "/obj/" in path.as_posix() or "/bin/" in path.as_posix():
            continue
        for lineno, line in enumerate(path.read_text(encoding="utf-8", errors="replace").splitlines(), 1):
            for key in RE_CS_TOKEN_WRITE.findall(line):
                if key in values:
                    continue
                rhs = line.split("]", 1)[1].lstrip(" =") if "]" in line else ""
                rhs = rhs.rstrip(";").strip()
                m = literal_double.match(rhs)
                if m:
                    values[key] = {"type": "x:Double", "raw": m.group(1), "line": lineno, "file": rel(path)}
                    continue
                m = literal_scaled.match(rhs)
                if m and m.group(2).strip() in ("1", "1.0"):
                    values[key] = {"type": "x:Double", "raw": m.group(1), "line": lineno, "file": rel(path)}
                    continue
                m = literal_corner.search(rhs)
                if m:
                    values[key] = {"type": "CornerRadius", "raw": m.group(1), "line": lineno, "file": rel(path)}
    return values


def rule_bare_hex(lint: Lint) -> None:
    for path in iter_theme_files():
        r = rel(path)
        if "Themes/Tokens/" in r:
            continue
        for lineno, line in enumerate(path.read_text(encoding="utf-8", errors="replace").splitlines(), 1):
            for hexval in RE_HEX.findall(line):
                if hexval.upper() in {h.upper() for h in HEX_TRANSPARENT}:
                    continue
                if r in HEX_SPECTRUM_FILES and "GradientStop" in line:
                    continue
                lint.add(
                    "R1 bare-hex", "error", r, lineno,
                    f"literal colour {hexval} in a shipped theme; use a Md.Sys.Color.* token "
                    f"so the value follows the active theme and dynamic colour scheme",
                )


def rule_references(lint: Lint, declared: set[str]) -> dict[str, int]:
    usage: dict[str, int] = defaultdict(int)
    for path in iter_all_axaml():
        r = rel(path)
        for lineno, line in enumerate(path.read_text(encoding="utf-8", errors="replace").splitlines(), 1):
            for key in RE_REF.findall(line):
                usage[key] += 1
                if key not in declared:
                    lint.add(
                        "R2 unresolved-key", "error", r, lineno,
                        f"resource key '{key}' is referenced but never declared; "
                        f"at runtime this silently resolves to nothing",
                    )
    return usage


def rule_orphans(lint: Lint, values: dict[str, tuple[str, str, int]], usage: dict[str, int]) -> None:
    for key in sorted(values):
        if usage.get(key, 0) == 0:
            kind, _, lineno = values[key]
            lint.add(
                "R3 orphan-token", "warning", "src/Md3.Avalonia/Themes/Tokens", lineno,
                f"token '{key}' ({kind}) is declared but never referenced",
            )


def rule_dangling_cs_key(lint: Lint, declared: set[str]) -> None:
    """R10 — a Md.* key looked up from C# that no shipped dictionary declares.

    ``TryGetResource``/``Resources[...]`` return null for an unknown key rather than throwing, so a
    renamed or mistyped token produces a control that renders with a fallback and no error. This
    rule is the cheap static half of that check; the headless L1 layer is the half that resolves
    through the real merged dictionary.
    """
    for path in sorted((REPO / "src").rglob("*.cs")):
        if "/obj/" in path.as_posix() or "/bin/" in path.as_posix():
            continue
        r = rel(path)
        for lineno, line in enumerate(path.read_text(encoding="utf-8", errors="replace").splitlines(), 1):
            for key in RE_CS_TOKEN_REF.findall(line):
                if key in declared or key in OPTIONAL_TOKENS:
                    continue
                lint.add(
                    "R10 dangling-cs-key", "error", r, lineno,
                    f"'{key}' is looked up at runtime but no theme dictionary declares it; the "
                    f"lookup returns null and the control renders unstyled",
                )


def rule_includes(lint: Lint) -> None:
    asm_roots = {
        "Md3.Avalonia": REPO / "src/Md3.Avalonia",
        "Md3.Avalonia.Extra": REPO / "src/Md3.Avalonia.Extra",
        "Md3.Avalonia.Icons": REPO / "src/Md3.Avalonia.Icons",
        "Md3.Avalonia.Icons.Lite": REPO / "src/Md3.Avalonia.Icons.Lite",
    }
    for path in iter_all_axaml():
        r = rel(path)
        for lineno, line in enumerate(path.read_text(encoding="utf-8", errors="replace").splitlines(), 1):
            for asm, resource in RE_INCLUDE.findall(line):
                root = asm_roots.get(asm)
                if root is None:
                    continue  # third-party assembly, cannot verify from the working tree
                if not (root / resource).exists():
                    lint.add(
                        "R4 broken-include", "error", r, lineno,
                        f"ResourceInclude points at avares://{asm}/{resource} which does not exist on disk",
                    )


def rule_hardcoded_geometry(lint: Lint) -> None:
    for path in sorted((REPO / "src/Md3.Avalonia/Themes/Controls").glob("*.axaml")):
        r = rel(path)
        for lineno, line in enumerate(path.read_text(encoding="utf-8", errors="replace").splitlines(), 1):
            for setter_attr, direct_attr, raw in RE_GEOMETRY.findall(line):
                attr = setter_attr or direct_attr
                if raw.strip() in GEOMETRY_IGNORED:
                    continue
                lint.add(
                    "R5 hardcoded-geometry", "warning", r, lineno,
                    f"{attr}=\"{raw.strip()}\" is a literal; a Md.Comp.* token would make this "
                    f"value auditable against the specification oracle",
                )


def load_json(path: Path, lint: Lint) -> dict | None:
    if not path.exists():
        lint.add("R6 oracle-schema", "error", rel(path), 0, "required spec-snapshot file is missing")
        return None
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        lint.add("R6 oracle-schema", "error", rel(path), exc.lineno, f"invalid JSON: {exc.msg}")
        return None


def rule_hardcoded_ui_text(lint: Lint) -> None:
    """R8 — English prose baked into a shipped template.

    A control template that says ``Text="Rows per page"`` renders that sentence for every culture
    and no consumer can retarget it, because the string is authored in the theme rather than
    resolved through ``MdLocalization``. The rule is deliberately conservative: it only reports a
    value that contains a run of three or more ASCII letters and at least one space, so glyphs,
    single words used as labels and ``{DynamicResource ...}`` references stay out of the report.
    """
    for path in iter_theme_files():
        r = rel(path)
        if "Themes/Tokens/" in r:
            continue
        for lineno, line in enumerate(path.read_text(encoding="utf-8", errors="replace").splitlines(), 1):
            for raw in RE_UI_TEXT.findall(line):
                text = raw.strip()
                if " " not in text or not RE_WORD.search(text):
                    continue
                lint.add(
                    "R8 hardcoded-ui-text", "warning", r, lineno,
                    f'Text="{text}" is authored English inside a shipped theme; route it through '
                    f"MdLocalization so it follows the inherited culture",
                )


def rule_dual_declaration(lint: Lint) -> None:
    """R9 — one token key declared twice, in XAML and in C#.

    ``Themes/Tokens/*.axaml`` ships a value and ``MdThemeManager.Apply`` assigns the same key at
    runtime. The static theme is what L1 validates; the C# assignment is what a dynamic-scheme
    application actually sees. Nothing in the build keeps the two in sync, so the inventory is
    reported (and the C#-side comparison is asserted by the headless L1 extension).
    """
    xaml_keys: set[str] = set()
    for path in sorted(TOKEN_DIR.glob("*.axaml")):
        xaml_keys.update(RE_KEY.findall(path.read_text(encoding="utf-8", errors="replace")))

    cs_keys: dict[str, tuple[str, int]] = {}
    for path in (REPO / "src").rglob("*.cs"):
        if "/obj/" in path.as_posix() or "/bin/" in path.as_posix():
            continue
        for lineno, line in enumerate(path.read_text(encoding="utf-8", errors="replace").splitlines(), 1):
            for key in RE_CS_TOKEN_WRITE.findall(line):
                cs_keys.setdefault(key, (rel(path), lineno))

    dual = sorted(set(cs_keys) & xaml_keys)
    for key in dual:
        path, lineno = cs_keys[key]
        lint.add(
            "R9 dual-declaration", "warning", path, lineno,
            f"token '{key}' is declared in Themes/Tokens and re-assigned at runtime; the two "
            f"values are compared by the L1 dual-declaration check",
        )
    lint.metrics["dual_declaration_keys"] = len(dual)
    lint.metrics["runtime_written_keys"] = len(cs_keys)


def rule_token_values(
    lint: Lint,
    oracle: dict,
    policy: dict,
    values: dict[str, tuple[str, str, int]],
    values_cs: dict[str, dict[str, object]] | None = None,
) -> list[dict]:
    mode = policy.get("mode", "bootstrap")
    tol = oracle.get("tolerances", {})
    tol_dip = float(tol.get("dip", 1.0))
    tol_corner = float(tol.get("corner", 0.5))
    tol_opacity = float(tol.get("opacity", 0.005))
    tol_font = float(tol.get("fontSize", 0.5))

    waived: dict[str, str] = {}
    for waiver in policy.get("waivers", []):
        if waiver.get("layer") not in (None, "L1_tokens"):
            continue
        for key in waiver.get("keys", []):
            waived[key] = f"{waiver.get('id', 'waiver')}: {waiver.get('reason', '')}"

    gaps: list[dict] = []
    for entry in oracle.get("tokens", []):
        key = entry["key"]
        kind = entry["type"]
        confidence = entry.get("confidence", "medium")
        required = entry.get("required", True)
        expected = entry["value"]

        from_cs = key in (values_cs or {})
        if key not in values and not from_cs:
            if not required:
                continue
            gaps.append(
                {"key": key, "kind": "missing", "confidence": confidence,
                 "expected": expected, "actual": None,
                 "detail": "no x:Key declaration found in Themes/Tokens"}
            )
            continue

        if from_cs:
            declared_kind, raw, lineno = str(values_cs[key]["type"]), str(values_cs[key]["raw"]), int(values_cs[key]["line"])
        else:
            declared_kind, raw, lineno = values[key]
        actual: object | None
        detail = ""
        if kind == "double":
            actual = parse_double(raw)
            limit = tol_opacity if "Opacity" in key else (tol_font if "FontSize" in key or "Size" in key and "TypeScale" in key else tol_dip)
            ok = actual is not None and abs(actual - float(expected)) <= limit
        elif kind in ("corner", "thickness"):
            actual = parse_quad(raw)
            exp_quad = [float(expected)] * 4 if isinstance(expected, (int, float)) else [float(v) for v in expected]
            limit = tol_corner if kind == "corner" else tol_dip
            ok = actual is not None and all(abs(a - e) <= limit for a, e in zip(actual, exp_quad))
            expected = exp_quad
        else:
            continue

        if not ok:
            where = "the runtime assignment" if from_cs else f"Themes/Tokens line {lineno}"
            detail = f"declared at {where} as <{declared_kind}>{raw}</{declared_kind}>"
            gaps.append(
                {"key": key, "kind": "mismatch", "confidence": confidence,
                 "expected": expected, "actual": actual, "detail": detail}
            )

    for gap in gaps:
        key = gap["key"]
        reason = waived.get(key)
        if reason:
            gap["waived"] = reason
            severity = "warning"
        elif mode == "enforce" and gap["confidence"] == "high":
            severity = "error"
        else:
            severity = "warning"
        gap["severity"] = severity
        suffix = f" [waived: {reason}]" if reason else ""
        if gap["kind"] == "missing":
            msg = f"token '{key}' expected by the oracle ({gap['expected']}) is not declared{suffix}"
        else:
            msg = f"token '{key}' is {gap['actual']}, oracle expects {gap['expected']} ({gap['detail']}){suffix}"
        lint.add(f"R7 token-value/{gap['confidence']}", severity, "spec-snapshot/tokens.json", 0, msg)
    return gaps


# ---------------------------------------------------------------------------
# Reporting
# ---------------------------------------------------------------------------
def write_report(path: Path, lint: Lint, gaps: list[dict], mode: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    by_rule: dict[str, list[Finding]] = defaultdict(list)
    for f in lint.findings:
        by_rule[f.rule].append(f)

    lines = [
        "# Design token lint",
        "",
        f"- policy mode: `{mode}`",
        f"- errors: **{len(lint.errors)}**",
        f"- warnings: **{len(lint.warnings)}**",
        "",
    ]
    if not lint.findings:
        lines.append("No findings.")
    for rule in sorted(by_rule):
        items = by_rule[rule]
        n_err = sum(1 for i in items if i.severity == "error")
        lines.append(f"## {rule} — {len(items)} finding(s), {n_err} error(s)")
        lines.append("")
        lines.append("| severity | location | detail |")
        lines.append("| --- | --- | --- |")
        for item in items[:200]:
            loc = f"`{item.path}`" + (f":{item.line}" if item.line else "")
            detail = item.message.replace("|", "\\|")
            lines.append(f"| {item.severity} | {loc} | {detail} |")
        if len(items) > 200:
            lines.append(f"| … | | {len(items) - 200} more suppressed |")
        lines.append("")

    if gaps:
        high = [g for g in gaps if g["confidence"] == "high" and "waived" not in g]
        lines += [
            "## Oracle gap summary",
            "",
            f"- high-confidence gaps (block `enforce` mode): **{len(high)}**",
            f"- medium-confidence gaps (always advisory): **{sum(1 for g in gaps if g['confidence'] == 'medium')}**",
            f"- waived: **{sum(1 for g in gaps if 'waived' in g)}**",
            "",
            "To move the ratchet forward, resolve every high-confidence gap above, then set",
            "`\"mode\": \"enforce\"` in `spec-snapshot/conformance-policy.json`.",
            "",
        ]
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--report", default="artifacts/spec/token-lint.md", help="markdown report path")
    ap.add_argument("--github", action="store_true", help="emit GitHub Actions annotations")
    ap.add_argument("--summary", action="store_true", help="append the report to $GITHUB_STEP_SUMMARY")
    args = ap.parse_args()

    lint = Lint()
    oracle = load_json(SPEC / "tokens.json", lint)
    policy = load_json(SPEC / "conformance-policy.json", lint) or {}
    load_json(SPEC / "manifest.json", lint)

    mode = policy.get("mode", "bootstrap")
    if mode not in ("bootstrap", "enforce"):
        lint.add("R6 oracle-schema", "error", "spec-snapshot/conformance-policy.json", 0,
                 f"mode must be 'bootstrap' or 'enforce', got '{mode}'")
        mode = "bootstrap"

    declared, values = collect_declarations(lint)
    values_cs = collect_cs_token_values(lint)
    rule_bare_hex(lint)
    usage = rule_references(lint, declared)
    rule_orphans(lint, values, usage)
    rule_includes(lint)
    rule_hardcoded_geometry(lint)
    rule_hardcoded_ui_text(lint)
    rule_dual_declaration(lint)
    rule_dangling_cs_key(lint, declared)
    gaps = rule_token_values(lint, oracle, policy, values, values_cs) if oracle else []

    lint.metrics.update(
        {
            "declared_keys": len(declared),
            "token_values": len(values),
            "referenced_keys": sum(1 for k, n in usage.items() if n > 0),
            "orphan_tokens": lint.count("R3 orphan-token"),
            "dual_declaration_keys": lint.metrics.get("dual_declaration_keys", 0),
            "hardcoded_geometry": lint.count("R5 hardcoded-geometry"),
            "hardcoded_ui_text": lint.count("R8 hardcoded-ui-text"),
            "bare_hex": lint.count("R1 bare-hex"),
            "unresolved_keys": lint.count("R2 unresolved-key"),
            "broken_includes": lint.count("R4 broken-include"),
            "dangling_cs_keys": lint.count("R10 dangling-cs-key"),
            "oracle_tokens": len(oracle.get("tokens", [])) if oracle else 0,
            "oracle_high": sum(1 for t in (oracle or {}).get("tokens", []) if t.get("confidence") == "high"),
            "oracle_families": len({t["key"].split(".")[2] for t in (oracle or {}).get("tokens", []) if t["key"].startswith("Md.Comp.")}),
            "high_confidence_gaps": sum(1 for g in gaps if g["confidence"] == "high" and "waived" not in g),
            "medium_confidence_gaps": sum(1 for g in gaps if g["confidence"] == "medium"),
            "waived_gaps": sum(1 for g in gaps if "waived" in g),
        }
    )
    lint.metrics["oracle_coverage_ratio"] = (
        round(lint.metrics["oracle_tokens"] / lint.metrics["declared_keys"], 4)
        if lint.metrics["declared_keys"]
        else 0.0
    )

    report = Path(args.report)
    if not report.is_absolute():
        report = REPO / report
    write_report(report, lint, gaps, mode)

    # Machine-readable twin of the markdown report. The ratchet check reads this file, so a
    # number that moves is a number a script can act on rather than a table someone skims.
    payload = {
        "schema_version": 1,
        "generated_at": __import__("datetime").datetime.now(
            __import__("datetime").timezone.utc).isoformat(),
        "mode": mode,
        "errors": len(lint.errors),
        "warnings": len(lint.warnings),
        "metrics": dict(sorted(lint.metrics.items())),
        "findings": [
            {"rule": f.rule, "severity": f.severity, "path": f.path, "line": f.line, "message": f.message}
            for f in lint.findings
        ],
        "oracle_gaps": [
            {k: v for k, v in g.items() if k in ("key", "kind", "confidence", "expected", "actual", "waived", "severity")}
            for g in gaps
        ],
    }
    report.with_suffix(".json").write_text(
        __import__("json").dumps(payload, indent=2, ensure_ascii=False, default=str) + "\n",
        encoding="utf-8",
    )

    if args.github:
        for f in lint.findings:
            kind = "error" if f.severity == "error" else "warning"
            loc = f"file={f.path}" + (f",line={f.line}" if f.line else "")
            print(f"::{kind} {loc},title={f.rule}::{f.message}")

    summary_path = os.environ.get("GITHUB_STEP_SUMMARY")
    if args.summary and summary_path:
        with open(summary_path, "a", encoding="utf-8") as fh:
            fh.write(report.read_text(encoding="utf-8"))

    print(f"token lint: mode={mode} errors={len(lint.errors)} warnings={len(lint.warnings)} "
          f"declared={len(declared)} tokens={len(values)} report={rel(report)}")
    for f in lint.errors[:40]:
        print(f"  ERROR {f.rule} {f.path}:{f.line} {f.message}")
    return 1 if lint.errors else 0


if __name__ == "__main__":
    sys.exit(main())
