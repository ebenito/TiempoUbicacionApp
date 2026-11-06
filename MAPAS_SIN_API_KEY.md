# Mapas en MAUI Blazor Hybrid sin API key de Google

Esta guía explica cómo TiempoUbicacionApp muestra mapas sin registrar ninguna clave de Google Maps, y qué hace falta para repetirlo en otro proyecto. Todo el código citado está en este repositorio y compila con .NET 10 y MAUI 10.

## La idea en dos líneas

No se usa ningún control de mapa nativo (`Microsoft.Maui.Controls.Maps` ni `CommunityToolkit.Maui.Maps`). Como la UI es Blazor dentro de un `BlazorWebView`, un mapa es simplemente contenido web: un `<iframe>` con una URL pública que no pide clave, o una librería JavaScript (Leaflet) con teselas de OpenStreetMap.

Hay tres variantes en la app, cada una para un caso:

| Caso | Técnica | Dónde |
|---|---|---|
| Mapa estático con marcador | `<iframe>` a Google Maps (`output=embed`) o a OpenStreetMap (`export/embed.html`) | `Components/Pages/MainPage.razor` |
| Mapa interactivo con pin arrastrable | Leaflet 1.9.4 con teselas de OSM y JS interop | `Components/Pages/EditEntryPage.razor`, `wwwroot/scripts.js` |
| Abrir el mapa en el navegador o en la app de mapas | `<a href target="_blank">` con URL de Google o de OSM | `AndroidHistory.razor`, `DesktopHistory.razor` |

Las URLs se construyen en un único helper, `Helpers/CoordinatesHelper.cs`.

## 1. Construir las URLs: `CoordinatesHelper`

El helper recibe las coordenadas como texto (en este proyecto se guardan en DMS, `41°05'35" N`), las convierte a decimal con `TryParseCoordinate` y las formatea siempre con `CultureInfo.InvariantCulture` para que el separador decimal sea el punto, independientemente del idioma del dispositivo. Este detalle es el fallo más habitual al copiar la técnica: con cultura `es-ES`, `lat.ToString()` produce `41,09` y la URL deja de funcionar.

### Google Maps embebido sin clave

```csharp
public static string? GetGoogleMapsUrl(
    string latStr, string lonStr,
    int zoom = 15, bool isEmbebed = true)
{
    if (!TryParseCoordinate(latStr, out var lat) ||
        !TryParseCoordinate(lonStr, out var lon))
        return null;

    var latS = lat.ToString("G", CultureInfo.InvariantCulture);
    var lonS = lon.ToString("G", CultureInfo.InvariantCulture);

    return isEmbebed
        ? $"https://maps.google.com/maps?q={latS},{lonS}&t=&z={zoom}&ie=UTF8&iwloc=&output=embed"
        : $"https://www.google.com/maps?q={latS},{lonS}&z={zoom}&ie=UTF8&iwloc=";
}
```

Parámetros que importan:
- `q=lat,lon` fija el centro y pone un marcador.
- `z=15` es el zoom (1 mundo, 20 calle).
- `output=embed` devuelve la versión embebible, sin barra lateral ni cabecera. Es la clave de todo: esta URL antigua de `maps.google.com` funciona dentro de un iframe sin API key.
- `t=` vacío es mapa normal; `t=k` sería satélite.
- La variante sin `output=embed` se usa para enlaces que abren el navegador o la app de Google Maps.

### OpenStreetMap embebido

```csharp
public static string? GetOSMEmbedUrl(string latStr, string lonStr, int zoom = 15)
{
    if (!TryParseCoordinate(latStr, out var lat) ||
        !TryParseCoordinate(lonStr, out var lon))
        return null;

    var latS = lat.ToString("G", CultureInfo.InvariantCulture);
    var lonS = lon.ToString("G", CultureInfo.InvariantCulture);

    double offset = 0.005;
    string bbox =
        $"{(lon - offset).ToString(CultureInfo.InvariantCulture)}%2C" +
        $"{(lat - offset).ToString(CultureInfo.InvariantCulture)}%2C" +
        $"{(lon + offset).ToString(CultureInfo.InvariantCulture)}%2C" +
        $"{(lat + offset).ToString(CultureInfo.InvariantCulture)}";

    return $"https://www.openstreetmap.org/export/embed.html?bbox={bbox}&layer=mapnik&marker={latS}%2C{lonS}";
}
```

OSM no acepta `zoom`: el encuadre lo define `bbox` (oeste, sur, este, norte). Un `offset` de 0.005 grados equivale a unos 500 m alrededor del punto. Para hacer zoom, cambia el offset. El parámetro `zoom` del método se ignora en la implementación actual.

### Elegir proveedor

El usuario escoge el proveedor en la página de configuración y se guarda en `Preferences`. El helper lo consulta en cada llamada:

```csharp
public static async Task<string?> GetMapUrlAsync(
    string latStr, string lonStr, ISettingsService settingsService,
    int zoom = 15, bool isEmbedded = true)
{
    var provider = await settingsService.GetMapProviderAsync();

    return provider switch
    {
        MapProvider.Google        => GetGoogleMapsUrl(latStr, lonStr, zoom, isEmbedded),
        MapProvider.OpenStreetMap => GetOSMEmbedUrl(latStr, lonStr, zoom),
        _                         => GetOSMEmbedUrl(latStr, lonStr, zoom)
    };
}
```

En un proyecto nuevo puedes prescindir del selector y llamar directamente a una de las dos funciones.

## 2. Mapa estático con iframe (`MainPage.razor`)

Marcado:

```razor
@if (!string.IsNullOrWhiteSpace(mapUrl))
{
    <div id="map-container" class="map-container">
        <iframe width="100%" height="230" frameborder="0"
                src="@mapUrl" allowfullscreen
                style="display:block;border:none;min-height:230px;">
        </iframe>
    </div>
}
```

Cálculo de la URL tras obtener el GPS. `location.Lat` y `location.Lng` son doubles:

```csharp
var culture = CultureInfo.InvariantCulture;
lat    = location.Lat.ToString(culture);
lon    = location.Lng.ToString(culture);
mapUrl = await CoordinatesHelper.GetMapUrlAsync(lat, lon, SettingsService, 15, true) ?? string.Empty;
```

Ajuste de altura para que el mapa ocupe lo que queda de pantalla sin tapar los botones inferiores. Se llama una sola vez desde `OnAfterRenderAsync`:

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (!string.IsNullOrWhiteSpace(mapUrl) && !_mapRendered)
    {
        _mapRendered = true;
        await JS.InvokeVoidAsync("fitMapHeight", "map-container", "#actions-bar", 230, 24);
    }
}
```

JavaScript en `wwwroot/shareInterop.js`:

```js
window.calcMapHeight = (mapContainerId, bottomElementsSelector, minHeight, bottomPadding) => {
    minHeight = minHeight || 230;
    bottomPadding = bottomPadding || 16;

    const container = document.getElementById(mapContainerId);
    if (!container) return minHeight;

    const containerTop = container.getBoundingClientRect().top;
    const viewportHeight = window.innerHeight;

    let bottomHeight = 0;
    if (bottomElementsSelector) {
        document.querySelectorAll(bottomElementsSelector).forEach(el => {
            bottomHeight += el.getBoundingClientRect().height;
        });
    }

    const available = viewportHeight - containerTop - bottomHeight - bottomPadding;
    return Math.max(minHeight, Math.floor(available));
};

window.fitMapHeight = (mapContainerId, bottomElementsSelector, minHeight, bottomPadding) => {
    const height = window.calcMapHeight(mapContainerId, bottomElementsSelector, minHeight, bottomPadding);
    const container = document.getElementById(mapContainerId);
    if (!container) return height;
    const iframe = container.querySelector('iframe');
    if (iframe) iframe.style.height = height + 'px';
    container.style.height = height + 'px';
    return height;
};
```

CSS del contenedor (`wwwroot/css/app.css`):

```css
.map-container {
    border-radius: var(--radius-md);
    overflow: hidden;
    border: 1px solid var(--slate-200);
    margin-top: 14px;
    position: relative;
}
.map-container::before {
    content: '';
    position: absolute;
    inset: 0;
    border-radius: var(--radius-md);
    box-shadow: inset 0 0 0 1px rgba(13,148,136,.15);
    z-index: 1;
    pointer-events: none;
}
```

El `overflow: hidden` con `border-radius` es lo que redondea las esquinas del iframe. El pseudoelemento `::before` con `pointer-events: none` dibuja un borde interior sin bloquear la interacción con el mapa.

## 3. Mapa interactivo con Leaflet (`EditEntryPage.razor`)

Para dejar que el usuario corrija la posición arrastrando un pin, el iframe no sirve (Google no devuelve eventos). Se usa Leaflet con teselas de OpenStreetMap, que tampoco requieren clave.

### Cargar Leaflet en `wwwroot/index.html`

Al final del `<body>`, antes de `blazor.webview.js`:

```html
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
```

### JavaScript en `wwwroot/scripts.js`

```js
window._editMaps = {};

window.initEditMap = function (containerId, lat, lon, dotNetRef) {
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
```

El diccionario `_editMaps` evita el error de Leaflet "Map container is already initialized" cuando Blazor vuelve a renderizar el componente.

### Componente Razor

Contenedor. Necesita altura explícita, Leaflet no la calcula:

```razor
<div id="@_mapContainerId"
     style="width:100%;height:300px;border-radius:10px;border:1px solid var(--mud-palette-divider);overflow:hidden;">
</div>
```

Código C#. El componente implementa `IAsyncDisposable` para liberar el mapa y la referencia .NET:

```csharp
private string _mapContainerId = "edit-map";
private DotNetObjectReference<EditEntryPage>? _dotNetRef;
private bool _mapInitialized;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (_entry is not null && !_mapInitialized)
    {
        if (TryGetDecimalCoords(out var lat, out var lon))
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            try
            {
                await JS.InvokeVoidAsync("initEditMap", _mapContainerId, lat, lon, _dotNetRef);
                _mapInitialized = true;
            }
            catch { /* Leaflet no disponible aún: se reintenta en el siguiente render */ }
        }
    }
}

[JSInvokable]
public void OnMarkerMoved(double lat, double lon)
{
    if (_entry is null) return;
    _entry.Latitude  = CoordinatesHelper.DecimalToDms(lat, isLatitude: true);
    _entry.Longitude = CoordinatesHelper.DecimalToDms(lon, isLatitude: false);
    InvokeAsync(StateHasChanged);
}

private void OnCoordsTextChanged(ChangeEventArgs _)
{
    _mapInitialized = false;   // fuerza reiniciar el mapa en el siguiente render
}

public async ValueTask DisposeAsync()
{
    try { await JS.InvokeVoidAsync("destroyEditMap", _mapContainerId); } catch { }
    _dotNetRef?.Dispose();
}
```

Puntos clave del interop:
- `OnAfterRenderAsync` y no `OnInitializedAsync`: el `<div>` tiene que existir en el DOM antes de que Leaflet lo busque.
- `DotNetObjectReference` permite que JS llame al método marcado con `[JSInvokable]`. Hay que liberarlo en `Dispose`.
- `InvokeAsync(StateHasChanged)` porque la llamada llega desde fuera del ciclo de render de Blazor.
- Al editar las coordenadas a mano se pone `_mapInitialized = false` y el siguiente render recrea el mapa centrado en el nuevo punto.

## 4. Enlaces para abrir el mapa fuera de la app

En el histórico cada registro enlaza a su posición:

```razor
<a href="@mapUrl" target="_blank" class="coord-link">@context.Latitude, @context.Longitude</a>
```

con `mapUrl` obtenida de `GetMapUrlAsync(..., zoom: 10, isEmbedded: false)`. El `BlazorWebView` de MAUI abre por defecto en el navegador del sistema los enlaces a dominios externos. En Android, si Google Maps está instalado, el sistema le pasa las URLs de `google.com/maps`. No hace falta código adicional.

Si en otro proyecto quieres forzar la app de mapas nativa en lugar del navegador, la opción habitual es un esquema `geo:` lanzado con `Launcher`. Este proyecto no lo usa, pero sería:

```csharp
await Launcher.Default.OpenAsync($"geo:{latS},{lonS}?q={latS},{lonS}");
```

En Windows ese esquema abre la app Mapas de Microsoft.

## 5. Conversión de coordenadas

Si tu proyecto guarda doubles, no necesitas nada de esto. TiempoUbicacionApp guarda DMS como texto, así que hace ida y vuelta:

- Decimal a DMS: `CoordinatesHelper.DecimalToDms(double, bool isLatitude)`. Usa `O` para oeste, no `W`.
- Texto a decimal: `CoordinatesHelper.TryParseCoordinate(string, out double)`. Acepta `40.859167`, `41º 03' 34'' N`, `40.51.33 N`, `40:51:33 N` y las palabras Norte, Sur, Este, Oeste. Sur y Oeste producen valores negativos.

Ten en cuenta que los segundos se truncan a entero, lo que introduce hasta unos 30 m de error en cada conversión.

## 6. Requisitos de plataforma

Android (`Platforms/Android/AndroidManifest.xml`):

```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<!-- Solo si además obtienes el GPS -->
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
```

No hace falta `<meta-data android:name="com.google.android.geo.API_KEY">` porque no hay SDK nativo de Google.

Windows (`Platforms/Windows/Package.appxmanifest`): capacidad `internetClient` y, si usas GPS, `location`.

Paquetes NuGet: ninguno específico de mapas. `CommunityToolkit.Maui.Maps` está referenciado en este proyecto por una fase anterior, pero no lo usa ningún código y puede eliminarse sin efecto. Lo mismo vale para la variable `MAPS_API_KEY` que lee `MauiProgram.cs`.

## 7. Limitaciones y recomendaciones

- **La URL `output=embed` de Google no está documentada.** Es el endpoint antiguo y Google puede retirarlo sin aviso. La alternativa oficial, Maps Embed API, exige clave. Por eso conviene mantener OSM como segundo proveedor o como valor por defecto. En esta app el valor por defecto es Google.
- **El iframe no lleva `sandbox` ni `referrerpolicy`.** El contenido de terceros corre sin restricciones dentro del WebView. Si tu proyecto lo requiere, añade `sandbox="allow-scripts allow-same-origin"` y `referrerpolicy="no-referrer"`, y comprueba que el mapa sigue cargando.
- **Todo necesita red:** Leaflet desde unpkg, teselas de OSM, el embed y la geocodificación. Para resistir caídas de unpkg, copia `leaflet.js` y `leaflet.css` en `wwwroot/lib/leaflet/` y referencia la copia local.
- **Política de uso de OSM.** Las teselas de `tile.openstreetmap.org` y Nominatim son para uso moderado y exigen un User-Agent identificable. En esta app el User-Agent se configura solo en el `HttpClient` de Nominatim; las teselas salen con el User-Agent del WebView. Para una app con muchos usuarios, usa un proveedor de teselas comercial o tu propio servidor.
- **`AndroidHistory.razor` ignora el proveedor configurado** y llama directamente a `GetGoogleMapsUrl` con el valor por defecto `isEmbebed = true`, así que el enlace abre la versión embed. Es un defecto conocido, no una técnica.
- Google solo se ha probado con coordenadas; `q=` admite también direcciones de texto, pero habría que codificarlas con `Uri.EscapeDataString`.

## 8. Lista de comprobación para otro proyecto

1. Copia `Helpers/CoordinatesHelper.cs` (o solo `GetGoogleMapsUrl` y `GetOSMEmbedUrl` si guardas doubles).
2. Formatea siempre con `CultureInfo.InvariantCulture`.
3. Para un mapa estático: `<iframe src="@mapUrl">` dentro de un contenedor con `overflow: hidden`.
4. Para un mapa interactivo: Leaflet en `index.html`, las funciones `initEditMap` y `destroyEditMap` en un `.js` de `wwwroot`, y en el componente `OnAfterRenderAsync` más `DotNetObjectReference` más `IAsyncDisposable`.
5. Permiso `INTERNET` en Android y capacidad `internetClient` en Windows.
6. Decide el proveedor por defecto sabiendo que Google es cómodo pero no oficial y OSM es estable pero con política de uso.
