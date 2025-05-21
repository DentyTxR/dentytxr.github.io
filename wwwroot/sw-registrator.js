window.updateAvailable = new Promise((resolve, reject) => {
    if (!('serviceWorker' in navigator)) {
        const errorMessage = `This browser doesn't support service workers`;
        console.error(errorMessage);
        reject(errorMessage);
        return;
    }

    navigator.serviceWorker.register('/service-worker.js')
        .then(registration => {
            console.info(`Service worker registration successful`);
            setInterval(() => {
                registration.update();
            }, 60 * 1000); // 60000ms -> check each minute
            registration.onupdatefound = () => {
                const installingServiceWorker = registration.installing;
                installingServiceWorker.onstatechange = () => {
                    if (installingServiceWorker.state === 'installed') {
                        resolve(!!navigator.serviceWorker.controller);
                    }
                }
            };
        })
        .catch(error => {
            console.error('Service worker registration failed with error:', error);
            reject(error);
        });
});

window.registerForUpdateAvailableNotification = (caller, methodName) => {
    window.updateAvailable.then(async isUpdateAvailable => {
        if (isUpdateAvailable) {
            try {
                const response = await fetch(`/data/version.json?nocache=${Date.now()}`);
                const data = await response.json();

                const previousVersion = localStorage.getItem('app-version');
                if (data.version !== previousVersion) {
                    localStorage.setItem('app-version', data.version);
                    await caller.invokeMethodAsync(methodName, data.version, data.changes);
                } else {
                    console.info('Service worker updated, but app version did not change. Skipping notification.');
                }
            } catch (error) {
                console.error('Error fetching version info:', error);
            }
        }
    });
};