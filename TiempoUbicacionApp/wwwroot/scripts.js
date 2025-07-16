window.canShare = function () {
    return !!navigator.share;
}

window.invokeShare = function (data) {
    return navigator.share(data);
}

window.copyToClipboard = function (text) {
    navigator.clipboard.writeText(text)
        /*.then(() => alert("Datos copiados al portapapeles."))*/
        .catch(err => alert("Error al copiar: " + err));
};
