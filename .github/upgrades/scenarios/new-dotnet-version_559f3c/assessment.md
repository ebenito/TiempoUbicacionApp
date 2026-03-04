# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [TiempoUbicacionApp\TiempoUbicacionApp.csproj](#tiempoubicacionapptiempoubicacionappcsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 1 | 0 require upgrade |
| Total NuGet Packages | 8 | All compatible |
| Total Code Files | 23 |  |
| Total Code Files with Incidents | 0 |  |
| Total Lines of Code | 1157 |  |
| Total Number of Issues | 0 |  |
| Estimated LOC to modify | 0+ | at least 0,0% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| [TiempoUbicacionApp\TiempoUbicacionApp.csproj](#tiempoubicacionapptiempoubicacionappcsproj) | net10.0-android;net10.0-windows10.0.26100.0 | ✅ None | 0 | 0 |  | ClassLibrary, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 8 | 100,0% |
| ⚠️ Incompatible | 0 | 0,0% |
| 🔄 Upgrade Recommended | 0 | 0,0% |
| ***Total NuGet Packages*** | ***8*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 0 |  |
| ***Total APIs Analyzed*** | ***0*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| CommunityToolkit.Maui | 13.0.0 |  | [TiempoUbicacionApp.csproj](#tiempoubicacionapptiempoubicacionappcsproj) | ✅Compatible |
| CommunityToolkit.Maui.Maps | 4.0.0 |  | [TiempoUbicacionApp.csproj](#tiempoubicacionapptiempoubicacionappcsproj) | ✅Compatible |
| Microsoft.AspNetCore.Components.WebView.Maui | 10.0.20 |  | [TiempoUbicacionApp.csproj](#tiempoubicacionapptiempoubicacionappcsproj) | ✅Compatible |
| Microsoft.Extensions.Logging.Debug | 10.0.1 |  | [TiempoUbicacionApp.csproj](#tiempoubicacionapptiempoubicacionappcsproj) | ✅Compatible |
| Microsoft.Maui.Controls | 10.0.20 |  | [TiempoUbicacionApp.csproj](#tiempoubicacionapptiempoubicacionappcsproj) | ✅Compatible |
| MudBlazor | 8.15.0 |  | [TiempoUbicacionApp.csproj](#tiempoubicacionapptiempoubicacionappcsproj) | ✅Compatible |
| sqlite-net-pcl | 1.9.172 |  | [TiempoUbicacionApp.csproj](#tiempoubicacionapptiempoubicacionappcsproj) | ✅Compatible |
| System.Windows.Extensions | 10.0.1 |  | [TiempoUbicacionApp.csproj](#tiempoubicacionapptiempoubicacionappcsproj) | ✅Compatible |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;TiempoUbicacionApp.csproj</b><br/><small>net10.0-android;net10.0-windows10.0.26100.0</small>"]
    click P1 "#tiempoubicacionapptiempoubicacionappcsproj"

```

## Project Details

<a id="tiempoubicacionapptiempoubicacionappcsproj"></a>
### TiempoUbicacionApp\TiempoUbicacionApp.csproj

#### Project Info

- **Current Target Framework:** net10.0-android;net10.0-windows10.0.26100.0✅
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 0
- **Dependants**: 0
- **Number of Files**: 107
- **Lines of Code**: 1157
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["TiempoUbicacionApp.csproj"]
        MAIN["<b>📦&nbsp;TiempoUbicacionApp.csproj</b><br/><small>net10.0-android;net10.0-windows10.0.26100.0</small>"]
        click MAIN "#tiempoubicacionapptiempoubicacionappcsproj"
    end

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 0 |  |
| ***Total APIs Analyzed*** | ***0*** |  |

