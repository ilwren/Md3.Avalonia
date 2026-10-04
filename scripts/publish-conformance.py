#!/usr/bin/env python3
"""Publish the specification conformance reports somewhere they can be read.

The reports are written to ``artifacts/spec/`` by the headless test run and uploaded
as a build artifact. An artifact is the wrong place for a result that should inform a
review: downloading it requires a browser session or a reachable blob-storage host,
and it is gone once the retention window closes. So this script fans the same content
out to three durable channels:

* the **job summary**, which is the natural place for a human looking at the run;
* a **notice annotation per layer**, which is the only part of a run that can be read
  back through the REST API alone;
* a **pull request comment**, so the gap list is visible next to the diff that moved
  it, and so a change that widens a gap is obvious during review rather than after.

The PR comment is upserted against a hidden marker, so re-running a workflow updates
the existing comment instead of burying the thread in duplicates.
"""

from __future__ import annotations

import os
import re
import subprocess
import sys
from pathlib import Path

MARKER = "<!-- md3-spec-conformance -->"
REPORT_DIR = Path("artifacts/spec")

# L0 runs in a different job from L1-L5, so each job publishes whatever it produced.
TITLES = {
    "token-lint": "L0 static token lint",
    "L1-tokens": "L1 token conformance",
    "L2-geometry": "L2 rendered geometry",
    "L3-visual": "L3 visual goldens",
    "L5-motion-physics": "L5 spring physics",
    "L5-motion-runtime": "L5 runtime motion",
}

COUNT = re.compile(r"^- (?P<label>[a-z][a-z\- ]*?): \*\*(?P<value>\d+)\*\*", re.MULTILINE)


def digest(text: str) -> str:
    """Condense a report to the one line that belongs in an annotation."""
    counts = [(m.group("label"), m.group("value")) for m in COUNT.finditer(text)]
    if not counts:
        return "no counters found"
    return ", ".join(f"{label}: {value}" for label, value in counts)


def emit(line: str) -> None:
    sys.stdout.write(line + "\n")


def escape(value: str) -> str:
    return value.replace("%", "%25").replace("\r", "%0D").replace("\n", "%0A")


def upsert_pr_comment(body: str) -> None:
    """Attach the report to the PR for this branch, if there is one."""
    repo = os.environ.get("GITHUB_REPOSITORY")
    if not repo or not os.environ.get("GH_TOKEN"):
        return

    branch = os.environ.get("GITHUB_HEAD_REF") or os.environ.get("GITHUB_REF_NAME")
    if not branch:
        return

    def gh(*args: str) -> subprocess.CompletedProcess[str]:
        return subprocess.run(
            ["gh", *args], capture_output=True, text=True, check=False, timeout=120
        )

    found = gh("pr", "list", "--head", branch, "--state", "open",
               "--json", "number", "--jq", ".[0].number // empty")
    number = found.stdout.strip()
    if not number:
        print(f"No open pull request for '{branch}'; skipping the review comment.")
        return

    listed = gh("api", f"repos/{repo}/issues/{number}/comments", "--paginate",
                "--jq", f'.[] | select(.body | contains("{MARKER}")) | .id')
    existing = listed.stdout.split()

    # GitHub rejects comment bodies over 65536 characters.
    if len(body) > 60000:
        body = body[:60000] + "\n\n_Report truncated; see the uploaded artifact for the full text._\n"

    if existing:
        result = gh("api", "--method", "PATCH",
                    f"repos/{repo}/issues/comments/{existing[0]}", "-f", f"body={body}")
        action = "updated"
    else:
        result = gh("api", "--method", "POST",
                    f"repos/{repo}/issues/{number}/comments", "-f", f"body={body}")
        action = "created"

    if result.returncode == 0:
        print(f"Conformance comment {action} on #{number}.")
    else:
        # Never fail the build over a comment; the summary and annotations still carry it.
        print(f"Could not publish the review comment: {result.stderr.strip()[:400]}")


def main() -> int:
    reports = sorted(REPORT_DIR.glob("*.md")) if REPORT_DIR.is_dir() else []
    if not reports:
        print("No conformance reports were produced.")
        return 0

    summary_parts: list[str] = [MARKER, "## Specification conformance", ""]
    full_parts: list[str] = []

    for report in reports:
        stem = report.stem
        text = report.read_text(encoding="utf-8")
        label = TITLES.get(stem, stem)
        emit(f"::notice title=Conformance {label}::{escape(digest(text))}")
        summary_parts.append(f"- **{label}** — {digest(text)}")
        full_parts.append(text.strip())

    summary_parts.append("")
    body = "\n".join(summary_parts) + "\n\n" + "\n\n---\n\n".join(full_parts) + "\n"

    summary_path = os.environ.get("GITHUB_STEP_SUMMARY")
    if summary_path:
        with open(summary_path, "a", encoding="utf-8") as stream:
            stream.write(body.replace(MARKER, "", 1))

    print(body)
    upsert_pr_comment(body)
    return 0


if __name__ == "__main__":
    sys.exit(main())
