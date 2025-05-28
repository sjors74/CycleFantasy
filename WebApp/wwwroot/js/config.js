let API_BASE_URL = "";
let FLAGS_BASE_URL = "";

if (window.location.hostname === "localhost") {
    API_BASE_URL = "https://localhost:44302";
    FLAGS_BASE_URL = "https://flagcdn.com";
} else {
    API_BASE_URL = "https://webcycleapi20250508145015.azurewebsites.net";
    FLAGS_BASE_URL = "https://flagcdn.com";
}

async function loadConfig() {
    try {
        console.log("Fetching config from:", `${API_BASE_URL}/config`);
        const response = await fetch(`${API_BASE_URL}/config`);
        if (!response.ok) {
            throw new Error(`Kon config niet laden: ${response.status} ${response.statusText}`);
        }
        const data = await response.json();
        API_BASE_URL = data.apiBaseUrl;
        console.log("einde loading config.")
    }
    catch (error) {
        console.error('Fout bij laden config:', error);
        throw error;
    }
}
