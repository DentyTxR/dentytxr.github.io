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

            setInterval(checkUpdate, 5 * 1000);
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
        const previousBuild = localStorage.getItem('app-build');

        console.log(`Version check -> Server: ${data.version} (Build: ${data.build}) | Local: ${previousVersion} (localBuild: ${previousBuild})`);

        if (data.version !== previousVersion || data.build !== previousBuild) {
            localStorage.setItem('app-version', data.version);
            localStorage.setItem('app-build', data.build);

            console.log("New update detected! Invoking C# method...");

            if (data.version !== previousVersion) {
                await caller.invokeMethodAsync("OnUpdateAvailable", data.version, data.changes || '');
            }
            else if (data.build !== previousBuild) {
                await caller.invokeMethodAsync("OnBuildChange", data.build || '');
            }
        }
    } catch (error) {
        console.error('Error fetching version info:', error);
    }
}