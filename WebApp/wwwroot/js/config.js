let API_BASE_URL = "https://webcycleapi20250508145015.azurewebsites.net";
//let API_BASE_URL = "https://localhost:44302"

async function loadConfig() {
    const response = await fetch(`${API_BASE_URL}/config`);
    const data = await response.json();
    API_BASE_URL = data.apiBaseUrl;
}
