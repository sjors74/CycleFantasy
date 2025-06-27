function formatDate(dateString) {
    const date = new Date(dateString);
    if (isNaN(date)) return "—"; // of "" of "Onbekend"
    return date.toLocaleDateString("nl-NL", {
        day: "numeric",
        month: "long",
        year: "numeric"
    });
}

function toggleGlobalLoader(show = true) {
    const loader = document.getElementById('global-loader');
    if (!loader) {
        console.warn("global-loader niet gevonden in DOM");
        return;
    }
    loader.style.display = show ? 'flex' : 'none';
}