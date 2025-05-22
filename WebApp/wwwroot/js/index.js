document.addEventListener("DOMContentLoaded", () => {
    fetchDataAndRenderTiles();
});

async function fetchDataAndRenderTiles(retries = 3, delay = 1000) {
    const loader = document.getElementById('loader');
    const container = document.getElementById("container");

    if (!container) {
        console.warn("Container niet gevonden - pagina gebruikt dit script waarschijnlijk niet.");
        return;
    }

    loader.style.display = 'block';
    container.innerHTML = '<p>Loading events...</p>';

    try {
        await loadConfig();
    } catch (err) {
        console.error("Configuratie laden mislukt:", err);
        loader.innerHTML = '<p>Fout bij laden configuratie</p>';
        return;
    }

    for (let attempt = 1; attempt <= retries; attempt++) {
        try {
            const response = await fetch(`${API_BASE_URL}/event`);
            if (!response.ok) throw new Error(`Serverfout: ${response.status}`);

            const events = await response.json();
            container.innerHTML = '';

            events.forEach(event => {
                const tile = document.createElement("div");
                tile.className = "tile";
                if (event.colorName) {
                    tile.style.backgroundColor = event.colorName;
                }
                // Inhoud van de tegel
                tile.innerHTML = `
                     <div class="flag">
                         <img src="${FLAGS_BASE_URL}/w40/${event.countryCode.toLowerCase()}.png" alt="Vlag">
                      </div>
                      <h3>${event.eventName}</h3>
                      <p><strong>Start:</strong> ${formatDate(event.startDate)}</p>
                      <p><strong>Einde:</strong> ${formatDate(event.endDate)}</p>
                    `;

                // Click-handler
                tile.onclick = () => handleTileClick(event);
                container.appendChild(tile);
            });

            break;
        } catch (error) {
            console.warn(`Poging ${attempt} mislukt:`, error);
            if (attempt === retries) {
                container.innerHTML = '<p style="color:red;">Fout bij laden van events. Probeer later opnieuw.</p>';
            } else {
                await new Promise(res => setTimeout(res, delay));
            }
        }
    }

    loader.style.display = 'none';
}

function handleTileClick(event) {
    // Event opslaan in localStorage als JSON
    localStorage.setItem("selectedEvent", JSON.stringify(event));

    // Navigeren naar de detailpagina
    window.location.href = `/Event?EventId=${event.eventId}`;
}







