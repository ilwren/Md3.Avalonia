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
else:
    tail = [l.strip() for l in text.splitlines() if l.strip()][-5:]
    print("::error::%s no unhandled-exception marker; tail: %s" % (label, " | ".join(tail)[:400]))
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
for result in results:
    if result.get("outcome") != "Failed":
        continue
    message = result.find(".//ns:Message", ns)
    first = message.text.strip().splitlines()[0] if message is not None and message.text else ""
    failed.append(f"{result.get('testName')}: {first}")

# Annotations are the only part of a run that survives a cancelled job, and GitHub caps them, so
# this is one line however many tests went wrong. The trx lists results in completion order, so
# the last entry names how far a dead process got.
last = results[-1].get("testName", "?").rsplit(".", 1)[-1] if results else "none"
print("::notice::%s alone: exit=%s, %d run, %d failed, last=%s -> %s"
      % (name, status, len(results), len(failed), last, "; ".join(failed) or "none"))
PY

exit "$status"
