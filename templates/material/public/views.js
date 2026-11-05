/*
 * Page-view counter for the Md3.Avalonia documentation site.
 *
 * The footer renders a container with data attributes; this file picks the provider
 * named by `_viewsProvider` in docfx.json globalMetadata. Swapping services is a
 * configuration change, not a template change.
 *
 * Every provider fails quietly: an unreachable counter leaves the footer without a
 * number instead of showing 0 or an error. Counters key on the canonical URL rather
 * than on `location`, because `/` and `/index.html` are the same page and the
 * canonical link already collapses them.
 */
(() => {
  const root = document.querySelector('[data-md3-views]');
  if (!root) return;

  const provider = (root.dataset.provider || 'none').trim().toLowerCase();
  if (provider === 'none') return;

  const pageItem = root.querySelector('[data-md3-views-page]');
  const pageValue = root.querySelector('[data-md3-views-page-value]');
  const separator = root.querySelector('[data-md3-views-sep]');
  const siteItem = root.querySelector('[data-md3-views-site]');
  const siteValue = root.querySelector('[data-md3-views-site-value]');

  const locale = document.documentElement.lang || 'en';

  const toNumber = (raw) => {
    const digits = String(raw ?? '').replace(/[^0-9]/g, '');
    return digits === '' ? null : Number(digits);
  };

  const show = (item, value, target) => {
    const number = toNumber(value);
    if (!item || !target || number === null) return;
    target.textContent = number.toLocaleString(locale);
    item.hidden = false;
  };

  const showPage = (value) => show(pageItem, value, pageValue);

  const showSite = (value) => {
    const number = toNumber(value);
    if (!siteItem || !siteValue || number === null) return;
    siteValue.textContent = number.toLocaleString(locale);
    if (separator) {
      // The separator only earns its place once both halves are visible.
      separator.hidden = pageItem ? pageItem.hidden : false;
    }
    siteItem.hidden = false;
  };

  const getJson = async (url) => {
    try {
      const response = await fetch(url, { cache: 'no-store' });
      if (!response.ok) return null;
      return await response.json();
    } catch {
      return null;
    }
  };

  const canonical = document.querySelector('link[rel="canonical"]');
  const path = (() => {
    if (!canonical) return location.pathname;
    try {
      return new URL(canonical.getAttribute('href'), location.href).pathname;
    } catch {
      return location.pathname;
    }
  })();

  // Abacus keys are unlisted strings, so a path becomes a stable slug.
  const slug = path.replace(/[^A-Za-z0-9]+/g, '-').replace(/^-+|-+$/g, '') || 'home';

  const session = {
    get(key) {
      try {
        return sessionStorage.getItem(key);
      } catch {
        return null;
      }
    },
    set(key, value) {
      try {
        sessionStorage.setItem(key, value);
      } catch {
        /* storage disabled: counting every load is the acceptable fallback */
      }
    },
  };

  const providers = {
    /*
     * Abacus: no account, one counter per key inside a namespace.
     * https://abacus.jasoncameron.dev — /hit increments and returns {"value": n},
     * /get only reads. A reader increments once per tab session and reads afterwards,
     * so a reload does not inflate the number.
     */
    async abacus() {
      const namespace = (root.dataset.namespace || '').trim();
      if (!namespace) return;
      const marker = `md3-views:${namespace}:${slug}`;
      const verb = session.get(marker) ? 'get' : 'hit';
      const data = await getJson(
        `https://abacus.jasoncameron.dev/${verb}/${encodeURIComponent(namespace)}/${encodeURIComponent(slug)}`,
      );
      if (!data) return;
      showPage(data.value);
      session.set(marker, '1');
    },

    /*
     * GoatCounter: count.js records the visit (it follows the canonical link, the same
     * path used below), and the public counter endpoint reports it.
     * Both the per-page JSON and TOTAL.json need "Allow adding visitor counts on your
     * website" in the GoatCounter settings, otherwise they answer 403 without CORS.
     */
    async goatcounter() {
      const code = (root.dataset.goatcounter || '').trim();
      if (!code) return;

      const script = document.createElement('script');
      script.async = true;
      script.src = 'https://gc.zgo.at/count.js';
      script.dataset.goatcounter = `https://${code}.goatcounter.com/count`;
      document.head.append(script);

      const base = `https://${code}.goatcounter.com/counter/`;
      const page = await getJson(`${base}${encodeURI(path)}.json`);
      if (page) showPage(page.count);

      const site = await getJson(`${base}TOTAL.json`);
      if (site) showSite(site.count);
    },

    /*
     * Anything that answers {"count": n} or {"value": n} for ?page=<path> — a
     * Cloudflare Worker in front of Umami or Plausible, a self-hosted counter, or
     * another hosted counter with a JSON API.
     */
    async endpoint() {
      const endpoint = (root.dataset.endpoint || '').trim();
      if (!endpoint) return;
      const url = new URL(endpoint, location.href);
      url.searchParams.set('page', path);
      const data = await getJson(url.href);
      if (!data) return;
      showPage(data.count ?? data.value);
      if (data.site ?? data.total) showSite(data.site ?? data.total);
    },
  };

  const run = providers[provider];
  if (run) run();
})();
