let API_BASE_URL = window.__config.apiBaseUrl;
let FLAGS_BASE_URL = window.__config?.flagsBaseUrl || "https://flagcdn.com";

async function loadConfig() {
    try {
        console.log("einde loading config.")
    }
    catch (error) {
        console.error('Fout bij laden config:', error);
        throw error;
    }
}

loadConfig();