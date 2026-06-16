const root = document.getElementById("autocomplete-root");
if (root) {
    const configurationItemCount = parseInt(root.dataset.count);
    for (let i = 0; i < configurationItemCount; i++) {
        setupAutocomplete(i);
    }
}

function setupAutocomplete(index) {
    const input = document.getElementById(`competitor-autocomplete-${index}`);
    const hiddenId = document.getElementById(`selectedCompetitorId-${index}`);

    if (!input || !hiddenId || !Array.isArray(competitors)) return;

    function closeDropdown() {
        const items = document.querySelectorAll(`#${input.id}-autocomplete-list`);
        items.forEach(el => el.parentNode.removeChild(el));
    }

    input.addEventListener('input', function () {
        const val = this.value.toLowerCase();

        const filtered = competitors.filter(c =>
            c.CompetitorName?.toLowerCase().includes(val)
        );

        closeDropdown();
        if (!val) return;

        if (filtered.length === 1) {
            input.value = filtered[0].CompetitorName;
            hiddenId.value = filtered[0].CompetitorId;
            return;
        }

        const dropdown = document.createElement('div');
        dropdown.setAttribute('id', this.id + '-autocomplete-list');
        dropdown.setAttribute('class', 'autocomplete-items');
        this.parentNode.appendChild(dropdown);

        filtered.forEach(c => {
            const item = document.createElement('div');
            item.classList.add('autocomplete-item'); // voeg een klasse toe voor styling
            item.innerHTML = `<span>${c.CompetitorName}</span>`;
            item.addEventListener('click', function () {
                input.value = c.CompetitorName;
                hiddenId.value = c.CompetitorId;
                closeDropdown();
            });
            dropdown.appendChild(item);
        });
    });

    document.addEventListener('click', function (e) {
        if (e.target !== input) {
            closeDropdown();
        }
    });
}