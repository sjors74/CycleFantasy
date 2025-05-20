document.addEventListener("DOMContentLoaded", () => {
    fetchDataAndRenderTiles();
});

function fetchDataAndRenderTiles() {
    const loader = document.getElementById('loader');
    loadConfig().then(() => {
        loader.style.display = 'block';
        fetch(`${API_BASE_URL}/event`)
            .then(response => response.json())
            .then(events => {
                const container = document.getElementById("container");

                if (!container) {
                    console.warn("Container niet gevonden - pagina gebruikt dit script waarschijnlijk niet.");
                    return;
                }

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
            })
            .catch(error => {
                console.error("Fout bij ophalen events:", error);
            })
            .finally(() => {
                loader.style.display = 'none';
            });
        });
}

function handleTileClick(event) {
    // Event opslaan in localStorage als JSON
    localStorage.setItem("selectedEvent", JSON.stringify(event));

    // Navigeren naar de detailpagina
    window.location.href = `/Event?EventId=${event.eventId}`;
}







