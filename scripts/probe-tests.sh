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
project="tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj"

timeout --signal=KILL 300 dotnet test "$project" \
  -c Release --no-build --no-restore \
  --filter "FullyQualifiedName~$filter" \
  --logger "trx;LogFileName=$trx" \
  --logger "console;verbosity=normal"
status=$?

python3 - "$filter" "$trx" "$status" <<'PY'
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
# this is one line however many tests went wrong.
print("::notice::%s alone: exit=%s, %d run, %d failed -> %s"
      % (name, status, len(results), len(failed), "; ".join(failed) or "none"))
PY

exit "$status"
