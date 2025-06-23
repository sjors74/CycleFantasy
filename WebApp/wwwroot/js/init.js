window.addEventListener("load", async () => {
    try {
        await loadConfig(); // wacht tot API_BASE_URL is ingesteld
        console.log("Config geladen, nu pingen...");

        const pingUrl = `${API_BASE_URL}/config/ping`;
        const response = await fetch(pingUrl);

        if (response.ok) {
            console.log("API is wakker:", pingUrl);
        } else {
            console.warn("Ping faalde met status:", response.status);
        }

        setInterval(() => {
            if (document.visibilityState === 'visible') {
                fetch(pingUrl)
                    .then(response => {
                        if (!response.ok) throw new Error('Ping response not OK');
                        console.log('Keep-alive ping verzonden');
                    })
                    .catch(err => console.error('Ping error:', err));
            }
        }, 5 * 60 * 1000); // elke 5 minuten


    } catch (err) {
        console.error("Fout bij config/ping:", err);
    }
});