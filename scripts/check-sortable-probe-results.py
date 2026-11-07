#!/usr/bin/env python3
"""Fail if the dedicated vendor probe passed without running every intended case."""
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

path = Path(sys.argv[1])
root = ET.parse(path).getroot()
ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
results = root.findall(".//t:UnitTestResult", ns)
expected = {
    "Restored_Assembly_And_Explicit_Mutation_Contracts",
    "Stale_Source_Index_Can_Change_Target_Even_When_Move_Returns_False",
    "Tracking_Strongly_Retains_Unattached_Controls_Until_Disabled",
    "Real_Pointer_Drag_Can_Transfer_Between_Same_Group_Lists",
    "Real_Pointer_Drag_Can_Be_Rejected_By_Target",
    "Real_Pointer_Drag_Does_Not_Transfer_To_A_Different_Group",
}
found = {result.get("testName", "").rsplit(".", 1)[-1]: result.get("outcome") for result in results}
if set(found) != expected or any(outcome != "Passed" for outcome in found.values()):
    raise SystemExit(f"Vendor probe incomplete or failing: expected {sorted(expected)}, got {found}")
print(f"::notice::Sortable vendor investigation: all {len(found)} characterization tests passed")
