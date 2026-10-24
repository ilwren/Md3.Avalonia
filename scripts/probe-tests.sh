#!/usr/bin/env bash
# Run one group of headless tests in a process of its own and report what happened.
#
# Every class in this suite shares one dispatcher on one UI thread, so a class that fails or stalls
# part way through a pumped dispatcher shows up as failures - or silence - in the classes around it.
# Running a group alone is what separates "these tests are broken" from "these tests break their
# neighbours". The run is bounded so a stall reports itself instead of spending the job's timeout.
#
# Usage: scripts/probe-tests.sh <filter-fragment> <trx-file-name>
set -uo pipefail

filter="${1:?a filter fragment is required}"
trx="${2:?a .trx file name is required}"
label="${3:-$filter}"
project="tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj"

# A filter containing '|' or '!' is a full vstest expression; anything else is a name fragment.
if [[ "$filter" == *'|'* || "$filter" == *'!'* ]]; then
  expression="$filter"
else
  expression="FullyQualifiedName~$filter"
fi

timeout --signal=KILL 300 dotnet test "$project" \
  -c Release --no-build --no-restore \
  --filter "$expression" \
  --logger "trx;LogFileName=$trx" \
  --logger "console;verbosity=normal" > /tmp/probe-console.txt 2>&1
status=$?

# A non-zero exit with no failed tests means the host itself died; the console is the only trace
# that is not lost. .NET prints "Unhandled exception. <Type>:" (or "Fatal error. <Type>") on the
# thread that kills the process, followed by the stack - ship that block, not the xunit frames
# around the pipeline message.
if [ "$status" -ne 0 ]; then
  python3 - /tmp/probe-console.txt "$label" <<'PY'
import re
import sys

path, label = sys.argv[1:3]
try:
    text = open(path, errors="replace").read()
except OSError:
    text = ""

match = re.search(r"(Unhandled exception\. [^\n]+|Fatal error\. [^\n]+)", text)
if match:
    start = match.start()
    chunk = text[start:start + 4000]
    lines = [l.strip() for l in chunk.splitlines() if l.strip()]
    header = lines[0][:160]
    frames = [l[:140] for l in lines[1:] if l.startswith("at ") or l.startswith("   at ")][:4]
    print("::error::%s crash: %s // %s" % (label, header, " | ".join(frames)[:330]))
    raise SystemExit

# No managed marker: a native abort. The aborting library prints its reason right before the
# run summary - ship everything after the last PASS/FAIL test line and before "Results File".
lines = [l for l in text.splitlines() if l.strip()]
cut = 0
for i, l in enumerate(lines):
    if re.search(r"(Passed!|Failed!|Aborted)", l) and ("error" not in l.lower()):
        cut = i
summary = next((i for i, l in enumerate(lines) if "Results File" in l), len(lines))
between = [l.strip() for l in lines[max(cut, 0):summary]
           if l.strip() and not re.match(r"^\[\w+\.net", l.strip())][:25]
blob = " | ".join(between)[:900]
print("::error::%s native-abort region: %s" % (label, blob or "(nothing between last test line and summary)"))
PY
fi

python3 - "$label" "$trx" "$status" <<'PY'
import glob
import sys
import xml.etree.ElementTree as ET

name, trx, status = sys.argv[1:4]
ns = {"ns": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
results = [
    result
    for path in glob.glob(f"**/{trx}", recursive=True)
    for result in ET.parse(path).getroot().findall(".//ns:UnitTestResult", ns)
]

failed = []
detailed = []
for result in results:
    if result.get("outcome") != "Failed":
        continue
    message = result.find(".//ns:Message", ns)
    text = message.text.strip() if message is not None and message.text else ""
    first = text.splitlines()[0]
    failed.append(f"{result.get('testName')}: {first}")
    if len(detailed) < 3 and text:
        detailed.append(f"{result.get('testName')}: {text[:1600]}")

# Annotations are the only part of a run that survives a cancelled job, and GitHub caps them, so
# this is one line however many tests went wrong. The trx lists results in completion order, so
# the last entry names how far a dead process got.
last = results[-1].get("testName", "?").rsplit(".", 1)[-1] if results else "none"
print("::notice::%s alone: exit=%s, %d run, %d failed, last=%s -> %s"
      % (name, status, len(results), len(failed), last, "; ".join(failed) or "none"))

# The first line loses the stack trace; the full message keeps the exact source line of an
# unhandled exception, which is what a truncated console log never survives.
for entry in detailed:
    print("::error::%s detail: %s" % (name, entry.replace("\n", " | ")))
PY

exit "$status"
