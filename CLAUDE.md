# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Qué es

App **.NET MAUI Blazor Hybrid** (un único `BlazorWebView` dentro de `MainPage.xaml`) para **Android** y **Windows (MSIX)**. Muestra hora local/UTC, zona horaria y DST, la posición GPS con nombre del lugar y un mapa, y guarda un histórico en SQLite. UI con **MudBlazor**. Idioma del código, comentarios, UI y commits: **español**.

Proyecto único: `TiempoUbicacionApp/TiempoUbicacionApp.csproj`. Solo compila con **.NET 10** (`net10.0-android` y, solo en Windows, `net10.0-windows10.0.26100.0`). No hay proyecto de tests.

## Comandos

```bash
# Restaurar (no admite -f; restaura todos los TFM)
dotnet restore TiempoUbicacionApp/TiempoUbicacionApp.csproj

# Compilar un destino concreto (lo que usa la CI)
dotnet build TiempoUbicacionApp/TiempoUbicacionApp.csproj -f net10.0-android -c Debug --no-restore
dotnet build TiempoUbicacionApp/TiempoUbicacionApp.csproj -f net10.0-windows10.0.26100.0 -c Debug --no-restore

# Ejecutar en Android (emulador o dispositivo conectado)
dotnet build TiempoUbicacionApp/TiempoUbicacionApp.csproj -f net10.0-android -t:Run

# Publicar MSIX (perfil en Properties/PublishProfiles)
dotnet publish TiempoUbicacionApp/TiempoUbicacionApp.csproj -f net10.0-windows10.0.26100.0 -c Release -p:PublishProfile=MSIX-win-x64
```

En Windows la ejecución habitual es desde Visual Studio (el perfil de `launchSettings.json` es `MsixPackage`). Para verificar un cambio basta con compilar ambos TFM y revisar que no aparecen errores ni nuevos avisos.

Problemas de build conocidos:
- **"Pide .NET 9" o restaura TFM antiguos**: `obj/` contiene un `project.assets.json` viejo. Borrar `TiempoUbicacionApp/obj` y `bin` (están en `.gitignore`) y restaurar.
- **Android falla con `XARLP7000 Renaming temporary file failed: Permission denied`**: un bloqueo temporal en `obj/` (Visual Studio o antivirus). Reintentar el build.
- La restauración completa tarda bastante (más de un minuto) por los workloads de Android.

## Arquitectura

### Arranque y navegación
- `MauiProgram.cs` registra todo. `App.xaml.cs` crea una sola `Window` con `MainPage`, que solo contiene el `BlazorWebView` (`HostPage=wwwroot/index.html`, componente raíz `Routes`).
- `Components/Routes.razor` enruta con `MainLayout` por defecto. **No hay `NavMenu.razor`**: el menú es un `MudDrawer` dentro de `Components/Layout/MainLayout.razor`, que además aloja los providers de MudBlazor, aplica el tema y lanza `RatingDialog` tras el arranque.
- Páginas con ruta: `/` (`MainPage.razor`), `/history`, `/configuracion`, `/about`. `HistoryPage` es solo un contenedor: elige `AndroidHistory` o `DesktopHistory` según `DeviceInfo.Platform` y muestra `EditEntryPage` como subcomponente cuando hay un registro en edición.

### Servicios por plataforma (tres mecanismos distintos, todos en `MauiProgram.cs`)
1. `#if ANDROID` / `#elif WINDOWS` importa `Platforms/<Plataforma>/PlatformServiceRegistration.cs`; ambas clases exponen el mismo método `AddPlatformServices()` y registran `IFeedbackService` (vibración en Android, `SystemSounds` en Windows).
2. `IAlertService` se elige con `#if WINDOWS`: `MauiAlertService` (Snackbar de MudBlazor) en Windows y `NativeAlertService` (toasts de CommunityToolkit) en el resto. Para que el Snackbar funcione hay que llamar a `Initialize(ISnackbar)` desde un componente.
3. Ficheros `*.android.cs` envueltos por completo en `#if ANDROID`.

Diálogos nativos (confirmaciones, alertas con botón): usar `Helpers/DialogHelper.cs`. `Application.Current.MainPage` y `DisplayAlert` están obsoletos en MAUI 10 y ya no se usan en el código.

### Datos
- `Services/LocationDatabaseService.cs`: SQLite (`sqlite-net-pcl`) en `FileSystem.AppDataDirectory/Ubicaciones.db`. Una sola tabla (`Models/LocationEntry.cs`), creada de forma perezosa; **no hay migraciones**. Si se añaden columnas hay que gestionar el cambio a mano.
- Las coordenadas se guardan como **strings en DMS** (`41°05'35" N`, con `O` para oeste), no como doubles. Todo lo que necesite un decimal pasa por `CoordinatesHelper.TryParseCoordinate`, que acepta decimal, DMS con símbolos, con puntos (`40.51.33 N`), con dos puntos y palabras en español.
- `Services/BackupService.cs` centraliza compartir, restaurar (valida la cabecera `SQLite format 3` y guarda copia previa) y enviar por correo. `AndroidHistory.razor` todavía tiene una copia propia de esa lógica.

### Mapas
No se usa ningún control de mapa nativo ni API key. Hay tres implementaciones independientes: iframe en `MainPage` (Google `output=embed` u OSM `export/embed.html` según `ISettingsService.GetMapProviderAsync`), Leaflet con teselas de OSM en `EditEntryPage` (JS en `wwwroot/scripts.js`, con `DotNetObjectReference` para devolver la posición del pin) y enlaces `<a target="_blank">` en el histórico. Las URLs se construyen en `Helpers/CoordinatesHelper.cs`. Detalle completo y guía para replicarlo en `MAPAS_SIN_API_KEY.md`.

`CommunityToolkit.Maui.Maps` y la variable `MAPS_API_KEY` (secreto de CI leído en `MauiProgram.cs`) están configurados pero **ningún código los consume**; en el dispositivo la variable llega vacía.

### Tema y estilos
- `Services/ThemeService.cs` construye el `MudTheme` (paleta teal) y guarda la preferencia. `MainLayout` añade o quita la clase `app-dark` en `<body>` por JS interop; los overrides de modo oscuro de `wwwroot/css/app.css` cuelgan de `.app-dark` y `.mud-dark`.
- Safe area de Android: solo CSS (`MainLayout.razor.css` con `env(safe-area-inset-*)`, AppBar sticky) más `viewport-fit=cover` en `index.html` y `SafeAreaEdges="All"` en `MainPage.xaml`. Explicado en `ANDROID_SAFE_AREA_FIX.md`.
- Ajustes de usuario (tema, intervalo de refresco, proveedor de mapa) en `Services/SettingsService.cs` sobre `Preferences`.

### Red
- Geocodificación inversa con Nominatim mediante `HttpClient` tipado para `GeolocationService` con `User-Agent` obligatorio (`MauiProgram.cs`). Nominatim bloquea peticiones sin User-Agent identificable.
- Leaflet se carga desde unpkg en `index.html`; no hay copia local. Sin red, el mapa de edición queda en blanco.

### Versionado y publicación
- Versión de la app en el csproj (`ApplicationDisplayVersion` / `ApplicationVersion`). `Platforms/Windows/Package.appxmanifest` lleva su propia `Version` y **no se sincroniza sola**: al subir versión hay que tocar ambos. `AboutPage.razor` muestra chips con versiones de librerías escritas a mano.
- La app muestra `AppInfo.Current.VersionString` en el pie del drawer y en `/about`.
- CI (`.github/workflows/build.yml`): restaura y compila ambos TFM en Release en cada push/PR a `master`. No ejecuta tests ni publica artefactos.

## Estado conocido
- `Services/PhotoService.android.cs` (búsqueda de fotos cercanas por EXIF) no está registrado ni se usa; es código sin conectar. Si se retoma, necesita `READ_MEDIA_IMAGES` en el manifest y sustituir `GetLatLong` por la propiedad `LatLong`.
- `MUDBLAZOR_9_IMPROVEMENTS.md` describe una paleta Material 3 morada que ya no existe; la paleta real vive en `ThemeService`.
- `.github/upgrades/` son los artefactos del asistente de migración a .NET 10; no son código.
