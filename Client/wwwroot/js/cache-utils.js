// Development utility to clear browser cache for samples
function clearSampleCache() {
    if ('caches' in window) {
        caches.keys().then(function(cacheNames) {
            cacheNames.forEach(function(cacheName) {
                if (cacheName.includes('sample') || cacheName.includes('blazor')) {
                    caches.delete(cacheName).then(function() {
                        console.log('Cache cleared:', cacheName);
                    });
                }
            });
        });
    }
    
    // Also clear localStorage if used for samples
    const keysToRemove = [];
    for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        if (key && key.includes('sample')) {
            keysToRemove.push(key);
        }
    }
    keysToRemove.forEach(key => localStorage.removeItem(key));
    
    console.log('Sample cache clearing completed. Please refresh the page.');
}

// Make function available globally for debugging
window.clearSampleCache = clearSampleCache;