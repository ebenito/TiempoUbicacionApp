# TiempoUbicacionApp
Una aplicación para obtener información geográfica y horaria en tiempo real.

App **.NET MAUI + Blazor Hybrid** para **Windows (MSIX)** y **Android** que muestra:
- Fecha y hora local / UTC
- Zona horaria actual (estándar o ahorro de luz)
- Ubicación GPS (latitud, longitud, nombre del lugar)
- Mapa embebido (Google Maps iframe)
- Guarda historial en SQLite
- Comparte datos por apps (WhatsApp, Gmail...)
- Menú con Histórico y Acerca de

## Tecnología
- Target frameworks: `net10.0-android` y `net10.0-windows10.0.26100.0`
- UI: MAUI + Blazor WebView + MudBlazor
- Datos: SQLite

## Requisitos
- Visual Studio 2026 con cargas de trabajo .NET MAUI
- **SDK .NET 10.0**
- Emulador o dispositivo Android (para probar Android)

## Instalación
1. Clona el repositorio:
   ```bash
   git clone https://github.com/ebenito/TiempoUbicacionApp.git
   cd TiempoUbicacionApp
   ```
2. Abre `TiempoUbicacionApp.sln` en Visual Studio.
3. Selecciona la plataforma (Windows/Android) y ejecuta.

## Publicación en Windows (MSIX)
Preparado para empaquetado **MSIX** en Windows (`WindowsPackageType=MSIX`).
