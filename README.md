# Todo List - aplikacja do zarządzania zadaniami by Mateusz Damian

Projekt natywnej aplikacji chmurowej realizowany w architekturze 3-warstwowej.

## Deklaracja Architektury (Mapowanie Azure)
Ten projekt został zaplanowany z myślą o usługach PaaS (Platform as a Service) w chmurze Azure.

| Warstwa | Komponent Lokalny      | Usługa Azure |
| :--- |:-----------------------| :--- |
| **Presentation** | React 19 + VITE | Azure Static Web Apps |
| **Application** | API (Node 24 (NestJs)) | Azure App Service |
| **Data** | SQL Server (Dev)               | Azure SQL Database (Serverless) |

## 🏗 Status Projektu i Dokumentacja
* [x] **Artefakt 1:** Zaplanowano strukturę folderów i diagram C4 (dostępny w `/docs`).
* [x] **Artefakt 2:** Konfiguracja środowiska Docker (w trakcie...).
* [x] **Artefakt 3:** Działająca warstwa prezentacji (React + Vite)
* [x] **Artefakt 4:** Działająca warstwa serwerowa + działające API Swaggera
* [x] **Artefakt 5:** System gotowy na chmurę
* [x] **Artefakt 6:** Backend i frontend działający z Azure
