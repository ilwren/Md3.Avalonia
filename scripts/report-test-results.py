import os
import sys
import glob
import xml.etree.ElementTree as ET

def main():
    trx_files = glob.glob("**/results.trx", recursive=True) + glob.glob("**/*.trx", recursive=True)
    if not trx_files:
        print("No .trx files found.")
        sys.exit(1)

    trx_file = trx_files[0]
    print(f"Reading test results from {trx_file}")
    tree = ET.parse(trx_file)
    root = tree.getroot()

    ns = {'ns': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
    results = root.findall('.//ns:UnitTestResult', ns)

    failed = 0
    passed = 0
    for result in results:
        outcome = result.get('outcome')
        test_name = result.get('testName')
        if outcome == 'Failed':
            failed += 1
            error_info = result.find('.//ns:ErrorInfo', ns)
            msg = ""
            stack = ""
            if error_info is not None:
                msg_el = error_info.find('ns:Message', ns)
                stack_el = error_info.find('ns:StackTrace', ns)
                if msg_el is not None and msg_el.text:
                    msg = msg_el.text.strip()
                if stack_el is not None and stack_el.text:
                    stack = stack_el.text.strip()
            # Escape newlines for github workflow command
            annotation = f"{test_name}: {msg}" if test_name else msg
            escaped_msg = annotation.replace('%', '%25').replace('\r', '%0D').replace('\n', '%0A')
            print(f"::error title=Headless test failed::{escaped_msg}")
            print(f"FAILED TEST: {test_name}\nMESSAGE:\n{msg}\nSTACK:\n{stack}\n" + "="*60)
        elif outcome == 'Passed':
            passed += 1

    print(f"Summary: {passed} passed, {failed} failed.")
    summary_path = os.environ.get("GITHUB_STEP_SUMMARY")
    if summary_path:
        with open(summary_path, "a", encoding="utf-8") as stream:
            stream.write("## Headless test results\n\n")
            stream.write(f"- Passed: **{passed}**\n")
            stream.write(f"- Failed: **{failed}**\n")
    if failed > 0:
        sys.exit(1)

if __name__ == "__main__":
    main()
