# Entity Framework Core - Performance-Optimierung aus der Praxis

## Setup
1) `ConnectionString` in `appsettings.json` prüfen.
2) Die Variable `_ensureDatabase` in `Program.cs` schaltet das automatische Anlegen der Datenbank ein- und aus. Beim ersten Durchlauf muss die Variable `true` sein.

## Verwendung

Die Klasse `Demos.cs` enthält die einzelnen Demos. Pro Durchlauf nur 1 Demo laufen lassen, ansonsten sieht man den erwarteten Effekt ggf. nicht, da der `DbContext` ein Cache ist.  

Für einige Demos muss das Lazy-Loading aktiviert sein, diese haben den Kommentar `Activate lazy-loading`. Hierfür muss die Variable `_ENSURE_DATABASE` in `Program.cs` auf `true` gesetzt werden.  
