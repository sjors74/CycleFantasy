let newsIntervalId = null;
let nieuwsItems = [];
let currentNewsIndex = 0;
let contentDiv = null;

document.addEventListener("DOMContentLoaded", () => {
    fetchDataAndRenderTiles();
    const newsModal = document.getElementById("newsModal");
    if (newsModal) {
        newsModal.addEventListener("show.bs.modal", stopNewsRotation);
        newsModal.addEventListener("hidden.bs.modal", startNewsRotation);
    }
});

document.addEventListener("click", function (e) {
    const btn = e.target.closest("button[data-bs-target='#newsModal']");
    if (!btn) return;

    const title = btn.getAttribute("data-title");
    const content = btn.getAttribute("data-full");

    document.getElementById("newsModalTitle").textContent = title;
    document.getElementById("newsModalBody").textContent = content;
});

async function fetchDataAndRenderTiles(retries = 3, delay = 1000) {
    const loader = document.getElementById('loader');
    const container = document.getElementById("container");

    if (!container) {
        console.warn("Container niet gevonden - pagina gebruikt dit script waarschijnlijk niet.");
        return;
    }

    try {
        await loadConfig();
    } catch (err) {
        console.error("Configuratie laden mislukt:", err);
        loader.innerHTML = '<p>Fout bij laden configuratie</p>';
        return;
    }

    for (let attempt = 1; attempt <= retries; attempt++) {

        toggleGlobalLoader(true);

        try {
            const response = await fetch(`${API_BASE_URL}/api/event`);
            if (!response.ok) throw new Error(`Serverfout: ${response.status}`);

            const events = await response.json();

            // Sla alle events op in localStorage
            localStorage.setItem('events', JSON.stringify(events));

            try {
                const newsRes = await fetch(`${API_BASE_URL}/api/news/latest`);
                if (!newsRes.ok) throw new Error(`Nieuws ophalen mislukt: ${newsRes.status}`);
                nieuwsItems = await newsRes.json();
            } catch (err) {
                console.error("Fout bij ophalen nieuwsitems:", err);
            }

            container.innerHTML = '';

            events.forEach(event => {
                const tile = document.createElement("div");
                tile.className = "tile tile-event";

                if (event.colorName) {
                   tile.style.backgroundColor = event.colorName;
                }

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

            if (nieuwsItems.length > 0) {
                const newsTile = document.createElement("div");
                newsTile.className = "tile tile-news";

                contentDiv = document.createElement("div");
                contentDiv.className = "tile-news-content";
                newsTile.appendChild(contentDiv);
                container.appendChild(newsTile);

                contentDiv.innerHTML = renderNewsItem(nieuwsItems[currentNewsIndex]);
                startNewsRotation();
            }
            break;

        } catch (err) {
            console.error("Fout bij laden van indexpagina:", err);
            if (attempt < retries) {
                await new Promise(resolve => setTimeout(resolve, delay));
            }
        } finally {
            toggleGlobalLoader(false);
        }
    }
}

function handleTileClick(event) {
    // Event opslaan in localStorage als JSON
    localStorage.setItem("selectedEvent", JSON.stringify(event));

    // Navigeren naar de detailpagina
    window.location.href = `/Event?EventId=${event.eventId}`;
}

function truncateMessage(message, maxLength = 140) {
    return message.length > maxLength
        ? message.slice(0, maxLength) + "…"
        : message;
}

function escapeHtml(str) {
    return str?.replace(/["&<>]/g, char => ({
        '"': '&quot;',
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
    }[char])) ?? '';
}

function renderNewsItem(item) {
    if (!item || !item.message) return `<p>Geen nieuws</p>`;
    return `
            <div class="tile-body limited-text">
            <div class="news-icon">
                <i class="bi bi-newspaper"></i>
            </div>
            <h3 class="news-title">${escapeHtml(item.title)}</h3>
            <p class="news-message">${truncateMessage(item.message)}</p>
            <p class="small text-muted">${formatDate(item.datePosted)}</p>
        </div>
        <div class="tile-footer mt-auto">
            <button class="btn btn-sm btn-outline-primary w-100"
                data-bs-toggle="modal"
                data-bs-target="#newsModal"
                data-full="${escapeHtml(item.message)}"
                data-title="${escapeHtml(item.title)}">
                Lees meer
            </button>
        </div>                            `;
}

function startNewsRotation() {
    stopNewsRotation(); // prevent double interval
    newsIntervalId = setInterval(() => {
        contentDiv.classList.add("fade-out");
        setTimeout(() => {
            currentNewsIndex = (currentNewsIndex + 1) % nieuwsItems.length;
            contentDiv.innerHTML = renderNewsItem(nieuwsItems[currentNewsIndex]);
            contentDiv.classList.remove("fade-out");
        }, 500);
    }, 12000);
}

function stopNewsRotation() {
    if (newsIntervalId) {
        clearInterval(newsIntervalId);
        newsIntervalId = null;
    }
}