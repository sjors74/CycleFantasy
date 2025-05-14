// Datum formatteren (YYYY-MM-DD naar bijv. 9 mei 2025)
function formatDate(isoDateString) {
    const options = { day: "numeric", month: "long", year: "numeric" };
    return new Date(isoDateString).toLocaleDateString("nl-NL", options);
}







