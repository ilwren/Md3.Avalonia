#!/usr/bin/env python3
"""Run repository verification and emit auditable evidence.

This script deliberately does not award Material or Flutter compliance from constants, file names,
or source-text matches. It validates the pinned baseline, parses markup, and runs the real test
projects when the .NET SDK is available. Platform claims remain unverified unless their platform
job supplies evidence separately.
"""

from __future__ import annotations

import argparse
import datetime as dt
import json
import shutil
import subprocess
import sys
import time
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parent.parent
REPORT_PATH = ROOT / "docs" / "reports" / "test-spec-execution-results.json"
MANIFEST_PATH = ROOT / "docs" / "spec-baseline-manifest.json"
MATRIX_PATH = ROOT / "docs" / "spec-test-matrix.json"


class Verification:
    def __init__(self) -> None:
        self.started = time.monotonic()
        self.results: list[dict[str, Any]] = []

    def record(self, check: str, status: str, message: str, evidence: list[str] | None = None) -> None:
        assert status in {"passed", "failed", "not_run"}
        result = {
            "check": check,
            "status": status,
            "message": message,
            "evidence": evidence or [],
        }
        self.results.append(result)
        label = {"passed": "PASS", "failed": "FAIL", "not_run": "NOT RUN"}[status]
        print(f"[{label}] {check}: {message}")

    def verify_baseline(self) -> None:
        try:
            manifest = json.loads(MANIFEST_PATH.read_text(encoding="utf-8"))
            flutter = manifest["flutter"]
            material = manifest["material_design_3"]
            commit = flutter["commit"]
            urls = material["sources"]
            valid = (
                manifest["schema_version"] == 1
                and len(commit) == 40
                and all(c in "0123456789abcdef" for c in commit)
                and bool(flutter["version"])
                and bool(material["retrieved_at"])
                and len(urls) >= 1
                and all(url.startswith("https://m3.material.io/") for url in urls)
            )
            if not valid:
                raise ValueError("required pinned version, commit, retrieval date, or official URLs are invalid")
            self.record(
                "pinned-spec-baseline",
                "passed",
                f"M3 retrieval date {material['retrieved_at']}; Flutter {flutter['version']} at {commit}.",
                [str(MANIFEST_PATH.relative_to(ROOT))],
            )
        except (OSError, KeyError, TypeError, ValueError, json.JSONDecodeError) as exc:
            self.record("pinned-spec-baseline", "failed", str(exc), [str(MANIFEST_PATH.relative_to(ROOT))])

    def verify_requirement_matrix(self) -> None:
        try:
            matrix = json.loads(MATRIX_PATH.read_text(encoding="utf-8"))
            requirements = matrix["requirements"]
            ids = [entry["id"] for entry in requirements]
            allowed_statuses = list(matrix["status_definitions"])
            if len(ids) != len(set(ids)):
                raise ValueError("requirement IDs must be unique")
            if not requirements:
                raise ValueError("at least one requirement is required")
            for entry in requirements:
                if entry["verification_status"] not in allowed_statuses:
                    raise ValueError(f"{entry['id']} has an unknown status")
                if not entry["platforms"] or not entry["source"].startswith("https://"):
                    raise ValueError(f"{entry['id']} is missing platforms or an upstream source")
                if not entry["automated_evidence"]:
                    raise ValueError(f"{entry['id']} has no named evidence")
            counts = {
                status: sum(entry["verification_status"] == status for entry in requirements)
                for status in allowed_statuses
            }
            self.record(
                "traceable-requirement-matrix",
                "passed",
                f"Validated {len(requirements)} uniquely identified requirements; statuses: {counts}. Matrix status is not test execution.",
                [str(MATRIX_PATH.relative_to(ROOT))],
            )
        except (OSError, KeyError, TypeError, ValueError, json.JSONDecodeError) as exc:
            self.record("traceable-requirement-matrix", "failed", str(exc), [str(MATRIX_PATH.relative_to(ROOT))])

    def parse_axaml(self) -> None:
        files = sorted(ROOT.glob("src/**/*.axaml")) + sorted(ROOT.glob("tests/**/*.axaml"))
        failures: list[str] = []
        for path in files:
            try:
                ET.parse(path)
            except ET.ParseError as exc:
                failures.append(f"{path.relative_to(ROOT)}: {exc}")
        if failures:
            self.record("axaml-well-formedness", "failed", "; ".join(failures), failures)
        else:
            self.record(
                "axaml-well-formedness",
                "passed",
                f"Parsed {len(files)} AXAML files as XML. This does not prove Avalonia XAML compilation.",
                ["src/**/*.axaml", "tests/**/*.axaml"],
            )

    def verify_claim_boundaries(self) -> None:
        # The parity document has to keep the vocabulary that separates full parity from subsets
        # and API shells.
        required = {
            "docs/FLUTTER_PARITY_STATUS.md": ["API shell", "Experimental", "Implemented subset"],
        }
        # And an API that finished its choreography must not still announce itself as a preview
        # shell. The four shared-axis transitions carried MdExperimental while they were state
        # shells; the marker came off with the real transitions, so the claim to guard is now the
        # absence, not the presence.
        forbidden = {
            "src/Md3.Avalonia.Extra/Controls/MdMotionControls.cs": ["MdExperimental("],
        }
        problems: list[str] = []
        for relative, markers in required.items():
            path = ROOT / relative
            text = path.read_text(encoding="utf-8") if path.exists() else ""
            for marker in markers:
                if marker not in text:
                    problems.append(f"{relative}: missing {marker!r}")
        for relative, markers in forbidden.items():
            path = ROOT / relative
            text = path.read_text(encoding="utf-8") if path.exists() else ""
            for marker in markers:
                if marker in text:
                    problems.append(f"{relative}: still claims {marker!r}")
        self.record(
            "parity-claim-boundaries",
            "failed" if problems else "passed",
            "; ".join(problems) if problems
            else "Parity subsets and API shells are separated, and completed motion APIs no longer claim to be experimental.",
            list(required) + list(forbidden),
        )

    def run_dotnet_tests(self, source_only: bool) -> None:
        if source_only:
            self.record("dotnet-build-and-tests", "not_run", "Skipped by --source-only; no runtime behavior is verified.")
            return
        dotnet = shutil.which("dotnet")
        if dotnet is None:
            self.record("dotnet-build-and-tests", "not_run", ".NET SDK is unavailable; C#, Avalonia XAML, and Headless tests are unverified.")
            return
        command = [
            dotnet,
            "test",
            str(ROOT / "tests" / "Md3.Avalonia.HeadlessTests" / "Md3.Avalonia.HeadlessTests.csproj"),
            "--configuration",
            "Release",
            "--nologo",
        ]
        completed = subprocess.run(command, cwd=ROOT, text=True, capture_output=True, check=False)
        log_path = ROOT / "artifacts" / "verification" / "dotnet-test.log"
        log_path.parent.mkdir(parents=True, exist_ok=True)
        log_path.write_text(completed.stdout + "\n" + completed.stderr, encoding="utf-8")
        self.record(
            "dotnet-build-and-tests",
            "passed" if completed.returncode == 0 else "failed",
            f"dotnet test exited with code {completed.returncode}.",
            [str(log_path.relative_to(ROOT))],
        )

    def record_android_evidence(self, junit_path: str | None) -> None:
        if junit_path is None:
            self.record(
                "android-device-regression",
                "not_run",
                "Must be supplied by the Android workflow/device job; Headless execution cannot validate popup/platform crashes.",
                [".github/workflows/android-test.yml", "tests/maestro"],
            )
            return
        path = Path(junit_path)
        if not path.is_absolute():
            path = ROOT / path
        evidence = [str(path.relative_to(ROOT))] if path.is_relative_to(ROOT) else [str(path)]
        try:
            root = ET.parse(path).getroot()
            suites = [root] if root.tag == "testsuite" else list(root.iter("testsuite"))
            if not suites:
                raise ValueError("JUnit contains no testsuite")
            tests = sum(int(suite.attrib.get("tests", "0")) for suite in suites)
            failures = sum(int(suite.attrib.get("failures", "0")) for suite in suites)
            errors = sum(int(suite.attrib.get("errors", "0")) for suite in suites)
            if tests <= 0:
                raise ValueError("JUnit contains no executed tests")
            self.record(
                "android-device-regression",
                "passed" if failures + errors == 0 else "failed",
                f"Android Maestro JUnit: {tests} tests, {failures} failures, {errors} errors.",
                evidence,
            )
        except (OSError, ValueError, ET.ParseError) as exc:
            self.record("android-device-regression", "failed", str(exc), evidence)

    def write(self) -> int:
        failed = sum(result["status"] == "failed" for result in self.results)
        not_run = sum(result["status"] == "not_run" for result in self.results)
        passed = sum(result["status"] == "passed" for result in self.results)
        all_runner_checks_completed = failed == 0 and not_run == 0
        payload = {
            "schema_version": 2,
            "generated_at": dt.datetime.now(dt.timezone.utc).isoformat(),
            "purpose": "repository verification evidence; not a Material or Flutter certification",
            "summary": {
                "passed": passed,
                "failed": failed,
                "not_run": not_run,
                "all_runner_checks_completed": all_runner_checks_completed,
                "material_or_flutter_compliance_claim": False,
            },
            "elapsed_seconds": round(time.monotonic() - self.started, 3),
            "results": self.results,
        }
        REPORT_PATH.parent.mkdir(parents=True, exist_ok=True)
        REPORT_PATH.write_text(json.dumps(payload, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
        print(f"Evidence written to {REPORT_PATH}")
        if failed:
            return 1
        if not_run:
            return 2
        return 0


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--source-only", action="store_true", help="skip .NET execution and record it as not run")
    parser.add_argument("--android-junit", help="Maestro JUnit XML produced by a real Android emulator/device job")
    args = parser.parse_args()
    verification = Verification()
    verification.verify_baseline()
    verification.verify_requirement_matrix()
    verification.parse_axaml()
    verification.verify_claim_boundaries()
    verification.run_dotnet_tests(args.source_only)
    verification.record_android_evidence(args.android_junit)
    return verification.write()


if __name__ == "__main__":
    sys.exit(main())
