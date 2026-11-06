#!/usr/bin/env python3
"""Expose the useful end of a failed native AOT publish/smoke as an Actions annotation."""
import re
import sys
from pathlib import Path

path = Path(sys.argv[1])
lines = path.read_text(encoding="utf-8", errors="replace").splitlines() if path.exists() else []
errors = [line.strip()[:500] for line in lines if re.search(r"error|failed|fatal|exception|abort", line, re.I)]
tail = [line.strip()[:500] for line in lines[-25:] if line.strip()]
message = "AOT publish/smoke failed. Relevant lines:\n" + "\n".join(errors[-12:]) + "\nFinal output:\n" + "\n".join(tail)
# GitHub's single-line workflow command escapes newlines, carriage returns and percent signs.
message = message[:14000].replace("%", "%25").replace("\r", "%0D").replace("\n", "%0A")
print("::error::" + message)
