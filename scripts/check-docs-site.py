#!/usr/bin/env python3
"""Checks the built documentation site for the metadata search engines read.

The DocFX build itself only fails on syntax errors. A Mustache tag that is
spelled wrongly, a front-matter key that never reaches the template, or a
sitemap that silently stops being generated all produce a green build and a
page that looks right in a browser. This script inspects the generated HTML
instead, so `.github/workflows/docs.yml` fails when the metadata regresses.

Usage: python3 scripts/check-docs-site.py [site-directory]
"""

from __future__ import annotations

import os
import re
import sys
from pathlib import Path

BASE = "https://ilwren.github.io/Md3.Avalonia"
SITE = Path(sys.argv[1] if len(sys.argv) > 1 else "_site")

failures: list[str] = []


def fail(message: str) -> None:
    failures.append(message)


def read(relative: str) -> str:
    path = SITE / relative
    if not path.is_file():
        fail(f"{relative}: missing from the built site")
        return ""
    return path.read_text(encoding="utf-8")


def expect(relative: str, needle: str, description: str) -> None:
    html = read(relative)
    if html and needle not in html:
        fail(f"{relative}: {description} missing (expected {needle!r})")


# --- sitemap -----------------------------------------------------------------

sitemap = read("sitemap.xml")
if sitemap:
    locations = re.findall(r"<loc>([^<]+)</loc>", sitemap)
    if not locations:
        fail("sitemap.xml: contains no <loc> entries")
    off_site = [url for url in locations if not url.startswith(BASE + "/")]
    if off_site:
        fail(f"sitemap.xml: {len(off_site)} entries outside {BASE} (first: {off_site[0]})")
    if len(locations) < 20:
        fail(f"sitemap.xml: only {len(locations)} entries; the API reference alone should exceed that")
    for required in ("docs/GETTING_STARTED.html", "index.html"):
        if not any(url.endswith(required) for url in locations):
            fail(f"sitemap.xml: no entry for {required}")

# --- per-page metadata --------------------------------------------------------

expect("index.html", f'<link rel="canonical" href="{BASE}/">', "canonical link")
expect("index.html", '<meta property="og:image"', "Open Graph image")
expect("index.html", '<meta name="twitter:card"', "Twitter card")
expect("index.html", 'class="md3-footer-nav"', "crawlable footer navigation")
expect("index.html", '<html lang="en">', "English document language")

expect("index.zh-CN.html", f'<link rel="canonical" href="{BASE}/index.zh-CN.html">', "canonical link")
expect("index.zh-CN.html", '<html lang="zh-CN">', "Chinese document language")
expect("index.zh-CN.html", 'hreflang="zh-Hans"', "Chinese alternate link")

page = "docs/GETTING_STARTED.html"
expect(page, f'<link rel="canonical" href="{BASE}/docs/GETTING_STARTED.html">', "canonical link")
expect(page, f'<link rel="alternate" hreflang="en" href="{BASE}/docs/GETTING_STARTED.html">', "English alternate")
expect(page, f'<link rel="alternate" hreflang="zh-Hans" href="{BASE}/docs/GETTING_STARTED.zh-CN.html">', "Chinese alternate")
expect(page, f'<link rel="alternate" hreflang="x-default" href="{BASE}/docs/GETTING_STARTED.html">', "x-default alternate")
expect(page, '<meta name="description" content="Add Md3.Avalonia to an Avalonia app', "meta description")

zh_page = "docs/GETTING_STARTED.zh-CN.html"
expect(zh_page, f'<link rel="canonical" href="{BASE}/docs/GETTING_STARTED.zh-CN.html">', "canonical link")
expect(zh_page, '<meta name="description" content="在 Avalonia 应用中接入 Md3.Avalonia', "Chinese meta description")

# A wrong Mustache tag renders as literal text rather than failing the build.
for relative in ("index.html", page, zh_page):
    html = read(relative)
    head = html[: html.find("</head>")] if "</head>" in html else html
    if "{{" in head or "}}" in head:
        fail(f"{relative}: unresolved template markers in <head>")

# --- comments stay off until giscus is configured -----------------------------

for relative in ("index.html", page):
    if "giscus.app/client.js" in read(relative):
        fail(
            f"{relative}: giscus rendered without _giscusRepo/_giscusRepoId in docfx.json; "
            "an unconfigured widget requests an empty repository and fails in the console"
        )

# --- the footer counter is configured, and its script ships --------------------

views_js = read("public/views.js")
if views_js and "abacus.jasoncameron.dev" not in views_js:
    fail("public/views.js: the abacus provider is missing from the shipped script")

expect("index.html", 'data-md3-views', "page-view counter container")
expect("index.html", 'data-provider="abacus"', "configured page-view provider")
expect("index.html", 'public/views.js', "page-view script")
if "busuanzi" in read("index.html").lower():
    fail("index.html: still requests the retired busuanzi counter")

# --- the API reference is generated, so only its presence is checked ----------

api_pages = list((SITE / "api").glob("*.html")) if (SITE / "api").is_dir() else []
if len(api_pages) < 100:
    fail(f"api/: only {len(api_pages)} generated pages; expected the full reference")

if failures:
    print(f"docs site: {len(failures)} problem(s)")
    for message in failures:
        print(f"  - {message}")
    # Also surface each one as a GitHub annotation: the log of a failing job is not
    # always reachable from a local `gh`, but annotations are.
    if os.environ.get("GITHUB_ACTIONS") == "true":
        for message in failures:
            print(f"::error title=docs site::{message}")
    raise SystemExit(1)

print(f"docs site: sitemap urls={len(locations)}, api pages={len(api_pages)}, metadata ok")
