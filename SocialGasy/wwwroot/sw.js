const CACHE_NAME = 'socialgasy-v1';

self.addEventListener('install', (e) => {
    self.skipWaiting();
    e.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            return cache.addAll([
                '/',
                '/css/site.css',
                '/js/sync-manager.js'
            ]);
        })
    );
});

self.addEventListener('fetch', (e) => {
    // 1. Raha POST na API, avelao handeha mivantana amin'ny Network
    if (e.request.method !== 'GET' || e.request.url.includes('/api/')) {
        e.respondWith(fetch(e.request));
        return;
    }

    // 2. Raha GET (pejy/css/js), andramo ao amin'ny Cache, raha tsy misy dia Network
    e.respondWith(
        caches.match(e.request).then((response) => {
            return response || fetch(e.request).catch(() => {
                // Raha tsy misy Internet ary tsy ao amin'ny Cache, asehoy ny Homepage
                if (e.request.mode === 'navigate') {
                    return caches.match('/');
                }
            });
        })
    );
});