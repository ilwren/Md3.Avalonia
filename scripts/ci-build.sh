#!/usr/bin/env bash
#
# Build one project and re-publish any compiler diagnostics as GitHub annotations.
#
# Why this exists rather than a bare `dotnet build`:
#
# A red build is only useful if the reason can be read back. GitHub serves raw job
# logs from a short-lived blob-storage URL, which is a separate host from the REST
# API and is frequently unreachable from locked-down or proxied environments; when
# it is, `gh run view --log-failed` returns nothing at all. Annotations come back
# from api.github.com like any other resource, so echoing every diagnostic as an
# annotation guarantees the failure is legible from wherever the fix is being made.
#
# The same text also goes to the step summary, because annotations are capped at ten
# per level per step and a first build of new code can easily blow past that.
#
# Usage: scripts/ci-build.sh <project.csproj> [label] [extra dotnet args...]

set -o pipefail

project="$1"
label="${2:-$(basename "$project" .csproj)}"
shift 2 2>/dev/null || shift $# # tolerate being called with only a project path

log="$(mktemp)"
trap 'rm -f "$log"' EXIT

dotnet build "$project" -c Release --no-restore "$@" 2>&1 | tee "$log"
status=${PIPESTATUS[0]}

if [ "$status" -eq 0 ]; then
  exit 0
fi

# MSBuild repeats a diagnostic once per target framework and once per referencing
# project, so the raw grep is mostly duplicates; sort -u keeps the annotation budget
# for distinct problems.
diagnostics="$(grep -E 'error [A-Z]+[0-9]+|: error :' "$log" | sed 's/^[[:space:]]*//' | sort -u | head -50)"

if [ -z "$diagnostics" ]; then
  diagnostics="$(tail -30 "$log")"
fi

if [ -n "${GITHUB_STEP_SUMMARY:-}" ]; then
  {
    echo "### Build failed: ${label}"
    echo
    echo '```'
    echo "$diagnostics"
    echo '```'
    echo
  } >>"$GITHUB_STEP_SUMMARY"
fi

while IFS= read -r line; do
  [ -z "$line" ] && continue
  line=${line//'%'/'%25'}
  line=${line//$'\r'/'%0D'}
  line=${line//$'\n'/'%0A'}
  echo "::error title=${label} build failed::${line}"
done <<<"$diagnostics"

exit "$status"
