window.canShare = function () {
    return !!navigator.share;
}

window.invokeShare = function (data) {
    return navigator.share(data);
}

window.copyToClipboard = function (text) {
    navigator.clipboard.writeText(text)
        .catch(err => alert("Error al copiar: " + err));
};

// ── Mapa Leaflet para EditEntryPage ──────────────────────────────────────────

window._editMaps = {};

window.initEditMap = function (containerId, lat, lon, dotNetRef) {
    // Destruir instancia previa si existe
    if (window._editMaps[containerId]) {
        window._editMaps[containerId].remove();
        delete window._editMaps[containerId];
    }

    var map = L.map(containerId).setView([lat, lon], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors',
        maxZoom: 19
    }).addTo(map);

    var marker = L.marker([lat, lon], { draggable: true }).addTo(map);

    marker.on('dragend', function (e) {
        var pos = e.target.getLatLng();
        dotNetRef.invokeMethodAsync('OnMarkerMoved', pos.lat, pos.lng);
    });

    // También permitir mover el pin haciendo clic en el mapa
    map.on('click', function (e) {
        marker.setLatLng(e.latlng);
        dotNetRef.invokeMethodAsync('OnMarkerMoved', e.latlng.lat, e.latlng.lng);
    });

    window._editMaps[containerId] = map;
};

window.destroyEditMap = function (containerId) {
    if (window._editMaps[containerId]) {
        window._editMaps[containerId].remove();
        delete window._editMaps[containerId];
    }
};
