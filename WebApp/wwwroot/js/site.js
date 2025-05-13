document.addEventListener("DOMContentLoaded", () => {
    fetchDataAndRenderTiles();
});

function fetchDataAndRenderTiles() {
    loadConfig().then(() => {
        fetch(`${API_BASE_URL}/event`)
            .then(response => response.json())
            .then(events => {
                const container = document.getElementById("container");
                events.forEach(event => {
                    const tile = document.createElement("div");
                    tile.className = "tile";
                    if (event.colorName) {
                        tile.style.backgroundColor = event.colorName;
                    }
                    // Inhoud van de tegel
                    tile.innerHTML = `
         <div class="flag">
             <img src="https://flagcdn.com/w40/${event.countryCode.toLowerCase()}.png" alt="Vlag">
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
            });
    });
}

// Datum formatteren (YYYY-MM-DD naar bijv. 9 mei 2025)
function formatDate(isoDateString) {
    const options = { day: "numeric", month: "long", year: "numeric" };
    return new Date(isoDateString).toLocaleDateString("nl-NL", options);
}

function handleTileClick(event) {
    // Event opslaan in localStorage als JSON
    localStorage.setItem("selectedEvent", JSON.stringify(event));

    // Navigeren naar de detailpagina
    window.location.href = `/Event?EventId=${event.eventId}`;
}







