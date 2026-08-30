window.registerForUpdateAvailableNotification = (caller, methodName) => {
    if (!('serviceWorker' in navigator)) {
        console.error("Service workers are not supported by this browser.");
        return;
    }

    navigator.serviceWorker.register('/service-worker.js')
        .then(registration => {
            console.info("Service worker registration successful.");

            const checkUpdate = () => {
                if (document.hasFocus()) {
                    registration.update().catch(err => console.warn("Update check failed:", err));
                }
            };

            setInterval(checkUpdate, 60 * 1000);
            document.addEventListener('visibilitychange', () => {
                if (document.visibilityState === 'visible') checkUpdate();
            });

            if (registration.waiting) {
                triggerUpdateNotification(registration.waiting, caller, methodName);
            }

            registration.onupdatefound = () => {
                const installingSW = registration.installing;
                if (!installingSW) return;

                installingSW.onstatechange = async () => {
                    console.log("Service worker state changed to:", installingSW.state);

                    // Ignore redundant workers completely
                    if (installingSW.state === 'redundant') return;

                    if (installingSW.state === 'installed' || installingSW.state === 'activated') {
                        await triggerUpdateNotification(installingSW, caller, methodName);
                    }
                };
            };
        })
        .catch(error => console.error('Service worker registration failed:', error));
};

async function triggerUpdateNotification(worker, caller, methodName) {
    if (worker.hasNotified) return;
    worker.hasNotified = true;

    try {
        const response = await fetch(`/data/version.json?nocache=${Date.now()}`);
        const data = await response.json();
        const previousVersion = localStorage.getItem('app-version');

        console.log(`Version check -> Server: ${data.version} | Local: ${previousVersion}`);

        if (data.version !== previousVersion) {
            localStorage.setItem('app-version', data.version);
            console.log("New version confirmed! Invoking C# method...");
            await caller.invokeMethodAsync(methodName, data.version, data.changes || '');
        }
    } catch (error) {
        console.error('Error fetching version info:', error);
    }
}