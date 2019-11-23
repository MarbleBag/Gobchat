# Gobchat (FFXIV chat overlay)
Gobchat is ein Overlay mit dem Ziel den Chat für Rollenspieler angenehmer zu machen.

Die Umsetzung dieser Software wurde inspiriert von [quisquous cactbot](https://github.com/quisquous/cactbot)
und verwendet [sharlayan](https://github.com/FFXIVAPP/sharlayan) module von FFXIVAPP um den Speicher von FFXIV zu verarbeiten.

Die Patchnotes können [hier](CHANGELOG.md) gefunden werden. (Englisch!)

The english version of this readme can be found [here.](README.md)

### Info: Die Readme wurde noch nicht vollständig übersetzt.

1. [Features](#features)
   1. ~~[Intelligentes Autoscroll](#smart-autoscroll)~~
   1. ~~[Text Formatierung](#text-formatting)~~
   1. ~~[Text-Hervorhebung](#text-highlighting-for-key-words---mentions)~~
   1. ~~[Zieh- and Vergrößerbar](#draggable-and-resizeable)~~
   1. ~~[Gruppen](#groups)~~
   1. ~~[Chat Kommandos](#chat-commands)~~
1. [Installation](#installation)
1. [Gobchat updaten](#gobchat-updaten)
1. [Gobchat verwenden](#gobchat-verwenden)
1. [License](#license)

## Installation

### Erforderliche Programme

Installiere [.NET Framework](https://www.microsoft.com/net/download/framework) version 4.8 oder höher

Visual C++ Redistributable Packages
Installiere [redistributables x64](https://aka.ms/vs/16/release/vc_redist.x64.exe) für 64-bit Windows
Installiere [redistributables x84](https://aka.ms/vs/16/release/vc_redist.x86.exe) für 32-bit Windows

### Installation von Gobchat

1. Besuch die Seite mit der [neusten Version](https://github.com/marblebag/gobchat/releases/latest) von Gobchat
2. Lade die neuste Version von Gobchat herunter. Die Datei heißt 'gobchat-{version}.zip'
3. Mache einen Rechtsklick auf die Zip Datai und gehe zu „Eigenschaften/Properties“.  Unten rechts in der Ecke des Eigenschaften Menüs auf „Unblock/freigeben“ klicken. Dann auf ok, um das Menü zu schließen.
4. Die zip Datei am gewünschten Ort entpacken. Die Datei enthält einen Ordner mit dem Namen Gobchat.
5. Wechsel in den Ordner Gobchat
6. Starte die Gobchat.exe
7. Bei jedem Start prüft Gobchat ob neue Updates verfügbar sind
8. Beim ersten Start versucht Gobchat CEF herunterzuladen. CEF ist ein eingebetteter Browser und nötig für die UI von Gobchat, die in HTML und Javascript geschrieben ist.

### Gobchat updaten

Aktuell müssen diese Schritte noch von Hand durchgeführt werden.

1. Wiederhole die Schritte 1 bis 4 der [Installation](#installation-von-gobchat) und ersetzte einfach alle Dateien.
2. Schon fertig!

## Gobchat verwenden
### Gobchat starten
1. Wechsel in deinen Gobchat Ordner
1. Starte die Gobchat.exe
1. Bei jedem Start prüft Gobchat ob neue Updates verfügbar sind

In deiner Tray (Icons unten rechts) wird ein neues Icon erscheinen: ![gobchat looks for ffxiv](screen_gobchat_off.png)
Das bedeutet Gobchat läuft und sucht nach einer aktiven Instanz von FFXIV

Wenn FFXIV läuft und Gobchat findet FFXIV, dann wechselt das Icon zu ![gobchat is ready to rumble](screen_gobchat_on.png)
Gobchat ist jetzt bereit. Das kann beim ersten Start von Gobchat eine Weile dauern.

### Tray Icon
- Linksklick: Zeigt oder versteckt das Overlay
- Rechtsklick:  Öffnet ein Kontextmenü

### Gobchat schließen
1. Rechtsklick das tray icon von Gobchat.
2. Klicke auf 'close'!

### Lizenz
Nur auf Englisch verfügbar:
This program is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License (AGPL-3.0-only) as published by the Free Software Foundation, version 3.
You can find the full license [here](LICENSE.md) or at https://www.gnu.org/licenses/