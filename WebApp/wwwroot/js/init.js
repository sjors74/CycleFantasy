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
    } catch (err) {
        console.error("Fout bij config/ping:", err);
    }
});