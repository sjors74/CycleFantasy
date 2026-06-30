document.addEventListener("DOMContentLoaded", function () {

    document.querySelectorAll(".competitor-autocomplete")
        .forEach(input => setupAutocomplete(input));

});

function setupAutocomplete(input) {
    const row = input.closest("tr");
    const hiddenId = row.querySelector(".competitor-id");

    if (!hiddenId) {
        return;
    }

    if (!Array.isArray(window.competitors)) {
        return;
    }

    if (!hiddenId || !window.competitors) return;

    function closeDropdown() {
        const items = document.querySelectorAll(`#${input.id}-autocomplete-list`);
        items.forEach(el => el.remove());
    }

    input.addEventListener("input", function () {

        const val = this.value.toLowerCase().trim();

        // altijd eerst oude dropdown weg
        row.querySelectorAll(".autocomplete-items")
            .forEach(el => el.remove());

        if (!val) return;

        const filtered = window.competitors.filter(c =>
            c.CompetitorName?.toLowerCase().includes(val)
        );

        if (
            filtered.length === 1 &&
            filtered[0].CompetitorName !== input.value
        ) {
            input.value = filtered[0].CompetitorName;
            hiddenId.value = filtered[0].CompetitorId;
            closeDropdown();
            return;
        }

        // ❗ belangrijk: als geen resultaten → niets tonen
        if (filtered.length === 0) return;

        const dropdown = document.createElement("div");
        dropdown.className = "autocomplete-items";
        input.parentNode.appendChild(dropdown);

        filtered.slice(0, 10).forEach(c => {   // optioneel limit voor UX
            const item = document.createElement("div");
            item.className = "autocomplete-item";

            item.textContent = c.CompetitorName;

            item.addEventListener("click", function () {
                input.value = c.CompetitorName;
                hiddenId.value = c.CompetitorId;

                dropdown.remove(); // direct sluiten
            });

            dropdown.appendChild(item);
        });
    });

    document.addEventListener('click', function (e) {
        if (!input.contains(e.target)) {
            closeDropdown();
        }
    });
}