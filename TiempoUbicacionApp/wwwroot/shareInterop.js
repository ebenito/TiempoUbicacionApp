// wwwroot/shareInterop.js

window.canShare = () => {
    return !!(navigator.share); // Devuelve true si el navegador soporta Web Share API
};

window.invokeShare = async (data) => {
    try {
        if (navigator.share) {
            await navigator.share({
                title: data.title || '',
                text: data.text || '',
                url: data.url || ''
            });
        } else {
            console.warn("Web Share API no está disponible.");
        }
    } catch (err) {
        console.error("Error al intentar compartir:", err);
        throw err;
    }
};

// Calcula el alto óptimo para el mapa dejando visibles todos los elementos inferiores.
// minHeight: altura mínima en px (230 por defecto).
// bottomPadding: margen de seguridad en px bajo el mapa.
window.calcMapHeight = (mapContainerId, bottomElementsSelector, minHeight, bottomPadding) => {
    minHeight = minHeight || 230;
    bottomPadding = bottomPadding || 16;

    const container = document.getElementById(mapContainerId);
    if (!container) return minHeight;

    const containerTop = container.getBoundingClientRect().top;
    const viewportHeight = window.innerHeight;

    // Calcular la altura total de los elementos que deben aparecer debajo del mapa
    let bottomHeight = 0;
    if (bottomElementsSelector) {
        document.querySelectorAll(bottomElementsSelector).forEach(el => {
            bottomHeight += el.getBoundingClientRect().height;
        });
    }

    const available = viewportHeight - containerTop - bottomHeight - bottomPadding;
    return Math.max(minHeight, Math.floor(available));
};

// Aplica el alto calculado al iframe dentro del map-container.
window.fitMapHeight = (mapContainerId, bottomElementsSelector, minHeight, bottomPadding) => {
    const height = window.calcMapHeight(mapContainerId, bottomElementsSelector, minHeight, bottomPadding);
    const container = document.getElementById(mapContainerId);
    if (!container) return height;
    const iframe = container.querySelector('iframe');
    if (iframe) iframe.style.height = height + 'px';
    container.style.height = height + 'px';
    return height;
};
