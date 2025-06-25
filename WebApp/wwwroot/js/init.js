let pingUrl;

window.addEventListener("load", async () => {
    try {
        await loadConfig(); // wacht tot API_BASE_URL is ingesteld
        pingUrl = `${API_BASE_URL}/config/ping`;

        console.log("Config geladen, nu pingen...");
        await fetch(pingUrl);
        console.log("API is wakker:", pingUrl);

        setInterval(() => {
            fetch(pingUrl)
                .then(response => {
                    if (!response.ok) throw new Error('Ping response not OK');
                    console.log('Keep-alive ping verzonden');
                })
                .catch(err => console.error('Ping error:', err));
        }, 5 * 60 * 1000); // elke 5 minuten

    } catch (err) {
        console.error("Fout bij config/ping:", err);
    }
});