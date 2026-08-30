/* Manifest version: zOMhhvkt */
// Import the assets manifest
self.importScripts('./service-worker-assets.js');

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;

const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/];
const offlineAssetsExclude = [/^service-worker\.js$/, /^data\/.*\.json$/];

const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

self.addEventListener('install', (event) => event.waitUntil(onInstall(event)));
self.addEventListener('activate', (event) => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', (event) => event.respondWith(onFetch(event)));

self.addEventListener('message', (event) => {
    if (event.data === 'skipWaiting') {
        self.skipWaiting();
    }
});

async function onInstall(event) {
    console.info('Service worker Install State');

    // Activate the new service worker as soon as the old one is retired
    self.skipWaiting();

    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { cache: 'no-cache' }));

    const cache = await caches.open(cacheName);
    await cache.addAll(assetsRequests);
}

async function onActivate(event) {
    console.info('Service worker Activate State');

    const cacheKeys = await caches.keys();
    await Promise.all(
        cacheKeys
            .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
            .map(key => caches.delete(key))
    );

    await self.clients.claim();
}

async function onFetch(event) {
    const cache = await caches.open(cacheName);
    const url = new URL(event.request.url);

    if (url.pathname.startsWith('/data/') && url.pathname.endsWith('.json')) {
        try {
            return await fetch(event.request, { cache: 'no-store' });
        } catch (error) {
            const cached = await cache.match(event.request);
            return cached || Promise.reject(error);
        }
    }

    if (event.request.method === 'GET') {
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !manifestUrlList.includes(url.href);

        const request = shouldServeIndexHtml ? 'index.html' : event.request;

        try {
            const networkResponse = await fetch(event.request);
            cache.put(request, networkResponse.clone());
            return networkResponse;
        } catch (error) {
            const cachedResponse = await cache.match(request);
            return cachedResponse || Promise.reject(error);
        }
    }

    return fetch(event.request);
}