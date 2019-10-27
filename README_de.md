### Installing Gobchat

1. Gehe zu latest release und lade damit die neueste Version von Gobchat (zip) herunter. (Nicht die source zips, außer du willst es selbst bauen)
2. Mache einen Rechtsklick auf die Zip Datai und gehe zu „Eigenschaften/Properties“.  Unten rechts in der Ecke des Eigenschaften Menüs auf „Unblock/freigeben“ klicken. Dann auf ok, um das Menü zu schließen. (Die Zip Datei beinhaltet eine dll).
3. Zip Datei entpacken
4. Kopiere die Ordner „addons“ Und „GobchatUI“ aus der entpackten Zip Datei in deinen OverlayPlugin Ordner. 

Am Ende sollte es etwa so aussehen:
	```code
	- ...\Advanced Combat Tracker\
	  - Advanced Combat Tracker.exe
	  - OverlayPlugin\
		 - OverlayPlugin.dll
		 - some OverlayPlugin files
		 - addons\
			- Gobchat.dll
		 - GobchatUI\
			- some files
	```


5. Starte ACT neu
6. Öffne ATC und öffne den Tab „Plugin“ -> wähle OverlayPlugin.dll aus
7. Klicke auf „new“
8. Gebe einen Namen deiner Wahl ein und wähle unter dem Drop Down Menü „Type“ aus und         bestätige mit „ok“
9. Klicke auf den Knopf im URL Feld und öffne den Pfad zu deines GobchatUI Ordners, um die Datei gobchat.html auszuwählen
10. Nun sollte das Gobchat Plugin auftauchen. 
11. Setzte ein Haken bei „Activate parsing“.  Nun sollte es den relevanten Inhalt deines FF14 Chats wiedergeben. 

### Updating Gobchat

Schritte 1 bis 4 von „Installation“ wiederholen und alle Dateien ersetzen.
