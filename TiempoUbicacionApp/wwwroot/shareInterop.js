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
