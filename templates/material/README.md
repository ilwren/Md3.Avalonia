# Material Design 3 layer for DocFX Modern

This is a small, repository-owned DocFX template extension. DocFX's built-in `default` and `modern` templates—and their `docfx.min.css` styles—remain the foundation. The custom `public/main.css` is loaded afterward and applies Md3.Avalonia's Material Design 3 palette, shape, typography, light/dark colors, and component treatments to the Modern template's existing Bootstrap components and page structure.

Color roles are kept aligned with `src/Md3.Avalonia/Themes/Tokens/ColorTokens.axaml`; the landing-page components are documented in the root `index.md`. There is no vendored third-party theme stylesheet or build-time download. When upgrading DocFX, review the selectors in `public/main.css` against the Modern template's DOM and CSS variables.
