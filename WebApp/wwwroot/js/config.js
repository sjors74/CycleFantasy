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
    const response = await fetch(`${API_BASE_URL}/config`);
    const data = await response.json();
    API_BASE_URL = data.apiBaseUrl;
}
