# Entity Framework Core - Performance-Optimierung aus der Praxis

## Setup
1) Als erstes sollte geprüft werden ob der `ConnectionString` in `appsettings.json` stimmt.
2) Mithilfe der Variable `_ensureDatabase` in `Program.cs`kann das automatische Anlegen der Datenbank ein- und abgeschaltet werden. Beim ersten Durchlauf muss die Variable `true` sein.

## Verwendung

Die einzelnen Demos sind in `Demos.cs` zu finden. Es sollte pro Durchlauf genau nur 1 Demo laufen, ansonsten sieht man den erwarteten Effekt ggf. nicht, da der `DbContext` ein Cache ist.  

Für einige Demos muss das Lazy-Loading aktiviert sein, diese haben den Kommentar `Activate lazy-loading`. Hierfür muss die Variable `_activateLazyLoading` in `Program.cs` auf `true` gesetzt werden.  
