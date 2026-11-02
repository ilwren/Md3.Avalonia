# Material Design 3 layer for DocFX Modern

This is a small, repository-owned DocFX template extension. DocFX's built-in `default` and `modern` templates—and their `docfx.min.css` styles—remain the foundation. The custom `public/main.css` is loaded afterward and applies Md3.Avalonia's Material Design 3 palette, shape, typography, light/dark colors, and component treatments to the Modern template's existing Bootstrap components and page structure.

Color roles are kept aligned with `src/Md3.Avalonia/Themes/Tokens/ColorTokens.axaml`; the landing-page components are documented in the root `index.md`. There is no vendored third-party theme stylesheet or build-time download. When upgrading DocFX, review the selectors in `public/main.css` against the Modern template's DOM and CSS variables.

## `layout/_master.tmpl`

The Modern template's master layout is owned here because three things cannot be
expressed through `docfx.json` alone: the `<head>` metadata, the comment thread
and the page-view counter.

The file is a copy of `templates/modern/layout/_master.tmpl` from the DocFX
version pinned in `.github/workflows/docs.yml` (2.81.0), plus the blocks marked
`{{!MD3: ...}}`. DocFX composes templates by file path, so this copy replaces the
Modern one; the `{{!include(/^public/.*/)}}` directives from every template in the
bundle are still merged, which is why `public/main.css` continues to load from
this directory. When the pinned DocFX version moves, diff the two files before
bumping:

```bash
curl -sL https://codeload.github.com/dotnet/docfx/tar.gz/refs/tags/v2.81.0 \
  | tar xz -O docfx-2.81.0/templates/modern/layout/_master.tmpl > /tmp/_master.tmpl
diff /tmp/_master.tmpl templates/material/layout/_master.tmpl
```

### Search indexing

DocFX already writes `sitemap.xml` (because `build.sitemap.baseUrl` is set) and
emits `<meta name="description">` for pages that define it. What the override
adds is the rest of what crawlers and social previews read:

| Block | Source | Effect |
| --- | --- | --- |
| `canonical` | page front matter | one canonical URL per page |
| `hreflang_en` / `hreflang_zh` / `hreflang_x_default` | page front matter | the English and Chinese pages point at each other |
| `og:*`, `twitter:*` | `_appTitle`, `_ogImage`, page front matter | link previews instead of a bare URL |
| `.md3-footer-nav` | this file | server-rendered site links; the DocFX sidebar, breadcrumb and next-article blocks are all built by `docfx.min.js`, so without this a page has no crawlable internal links at all |

`scripts/check-docs-site.py` runs after the build and fails CI if the sitemap,
canonical link, alternates, description or footer navigation disappear from the
generated HTML — or if a Mustache tag survives into `<head>` unresolved.

Search Console and Bing Webmaster Tools both offer an HTML-tag verification method, which is
the only one available here: a `github.io` project site has no DNS record and cannot serve a
verification file at the host root. Paste the token from either service into `docfx.json`:

```json
"_googleSiteVerification": "the content value Google gives you",
"_bingSiteVerification": "the msvalidate.01 content value",
```

`robots.txt` is deliberately absent. The site is a GitHub project page, and a
`robots.txt` only takes effect at the host root (`https://ilwren.github.io/robots.txt`),
which this repository does not own. Nothing blocks crawling, so the sitemap is the
discovery path: submit `https://ilwren.github.io/Md3.Avalonia/sitemap.xml` in
Google Search Console and Bing Webmaster Tools.

### Comment thread

Comments use [giscus](https://giscus.app) over GitHub Discussions. The block only
renders when **both** conditions hold:

1. `_giscusRepo`, `_giscusRepoId`, `_giscusCategory` and `_giscusCategoryId` are set
   in `build.globalMetadata` in `docfx.json`; and
2. the page front matter says `comments: true`.

API pages are generated from metadata, so they never carry front matter and can
never open a discussion — only the guide pages in `docs/`.

To turn it on:

1. Open **Settings → General → Features** in the repository and enable **Discussions**.
2. Create a discussion category for documentation, e.g. `Docs` (type *Announcements*
   keeps new threads to maintainers; an *Open* category lets readers start one).
3. Install the [giscus app](https://github.com/apps/giscus) on the repository.
4. Configure the repository at <https://giscus.app> and copy the four values it
   generates into `docfx.json`:

   ```json
   "_giscusRepo": "ilwren/Md3.Avalonia",
   "_giscusRepoId": "R_kgDO...",
   "_giscusCategory": "Docs",
   "_giscusCategoryId": "DIC_kwDO...",
   "_giscusLang": "en"
   ```

`data-mapping="pathname"` and `data-strict="1"` keep one discussion per page path,
and `data-theme="preferred_color_scheme"` follows the reader's OS instead of the
site toggle. Chinese pages can set `_giscusLang: zh-CN` in their front matter.

Giscus is the recommendation rather than Gitalk: Gitalk asks for an OAuth app whose
client secret would have to ship in the page, and it calls the GitHub API from the
browser, where rate limits are per IP.

### Page-view counter

Set `"_busuanzi": true` in `build.globalMetadata` to show page views and site views
in the footer. The label text lives in `public/main.css` and switches on
`html[lang^='zh']`, so no page needs its own string. The counter is a third-party
script (busuanzi); for analytics rather than a public number, prefer the DocFX
built-in `_googleAnalyticsTagId`, which the template already supports, or a
self-hosted GoatCounter/Umami deployment.
