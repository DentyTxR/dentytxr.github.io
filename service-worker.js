// Import the assets manifest
self.importScripts('./service-worker-assets.js');

// Cache name and prefix
const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;

// Include and exclude patterns for caching
const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/];
const offlineAssetsExclude = [/^service-worker\.js$/];

// Replace with your base path if hosting on a subfolder (e.g., "/<repository-name>/")
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

// Install event
self.addEventListener('install', (event) => event.waitUntil(onInstall(event)));

// Activate event
self.addEventListener('activate', (event) => event.waitUntil(onActivate(event)));

// Fetch event
self.addEventListener('fetch', (event) => event.respondWith(onFetch(event)));

// Listen for "skipWaiting" messages from the app
self.addEventListener('message', (event) => {
    if (event.data === 'skipWaiting') {
        self.skipWaiting();
    }
});

// Install handler
async function onInstall(event) {
    console.info('Service worker: Install');

    // Activate the new service worker as soon as the old one is retired
    self.skipWaiting();

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { cache: 'no-cache' })); // Remove integrity to avoid SHA-256 mismatch issues

    const cache = await caches.open(cacheName);
    await cache.addAll(assetsRequests);
}

// Activate handler
async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheKeys = await caches.keys();
    await Promise.all(
        cacheKeys
            .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
            .map(key => caches.delete(key))
    );

    // Take control of all clients immediately
    await self.clients.claim();
}

// Fetch handler
async function onFetch(event) {
    const cache = await caches.open(cacheName);

    if (event.request.method === 'GET') {
        // For navigation requests, serve index.html if not in the manifest
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !manifestUrlList.includes(event.request.url);

        const request = shouldServeIndexHtml ? 'index.html' : event.request;

        // Try network first, then cache as fallback
        try {
            const networkResponse = await fetch(event.request);
            cache.put(request, networkResponse.clone());
            return networkResponse;
        } catch (error) {
            const cachedResponse = await cache.match(request);
            return cachedResponse || Promise.reject(error);
        }
    }

    // For other requests, use default fetch behavior
    return fetch(event.request);
}/* Manifest version: +CLR5nOR */
