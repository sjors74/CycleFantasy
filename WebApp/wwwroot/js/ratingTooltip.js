function renderRatings(ratings, maxRating = 2500) {
    if (!ratings || ratings.length === 0) return '';

    const topRating = ratings.reduce((a, b) => a.rating > b.rating ? a : b);

    return `
        <span class="rating-trigger ms-2">

            <span class="rating-dot bg-${topRating.color}"></span>

            <span class="rating-tooltip">

                ${ratings.map(r => {
        const percentage = (r.rating / maxRating) * 100;

        return `
                        <div class="rating-row">

                            <span class="rating-name">
                                ${r.code}
                            </span>

                            <div class="rating-progress">
                                <div class="rating-progress-bar bg-${r.color}" 
                                    style="width:${percentage.toFixed(1)}%">
                                </div>
                            </div>

                            <span class="rating-value">
                                ${r.rating}
                            </span>

                        </div>
                    `;
    }).join('')}

            </span>

        </span>
    `;
}
window.renderRatings = renderRatings;