# Gobchat (FFXIV chat overlay)
Gobchat is an overlay with the goal to provide a better chat experience for roleplayer.

Gobchat in its current version is a plugin for [hibiyasleep's OverlayPlugin](https://github.com/hibiyasleep/OverlayPlugin) which itself is a plugin for
[Advanced Combat Tracker](http://advancedcombattracker.com/).

This plugin took a lot of inspiration (and some code) from [quisquous cactbot](https://github.com/quisquous/cactbot)

### Smart autoscroll
It won't scroll if you are (re)reading a text from the past

![screenshot 1](docs/screen_scroll_noautoscrol.png)

but will gladly do so again, if you move the scroll bar back to the bottom of the chat

![screenshot 1](docs/screen_scroll_bottom.png)

### Formatting text

![screenshot 2](docs/screen3.png)

becomes

![screenshot 3](docs/screen4.png)

Four different types

![screenshot 1](docs/screen_formats.png)

### Text-Highlighting for key words
Case-insensitive detection for a customizable list of words, which then will be highlighted. This will help you not missing out on important messages.

![screenshot 1](docs/screen_mention_highlighting.png)


## Installation

### Dependencies

Install [.NET Framework](https://www.microsoft.com/net/download/framework) version 4.7.2 or above

Install [Advanced Combat Tracker](http://advancedcombattracker.com/) 64-bit version

Install the most recent version of [ravahn's FFXIV ACT plugin](https://github.com/ravahn/FFXIV_ACT_Plugin/releases/latest) to ACT

Install the most recent version of [hibiyasleep OverlayPlugin](https://github.com/hibiyasleep/OverlayPlugin/releases/latest) to ACT

fflogs has a video [guide](https://www.fflogs.com/help/start/) for setting up ACT

### Installing Gobchat

1. Go to [latest release](https://github.com/marblebag/gobchat/releases/latest) and download the latest version of Gobchat (zip) (Do not download the source zips, except you want to build it yourself)
2. Right click the zip file and go to properties. In the bottom right corner of the properties menu, click "Unblock", and then "OK" to close the menu. (zip file contains a dll)
3. Unzip the zip file
4. Copy the Gobchat.dll into your OverlayPlugin\addons folder (The OverlayPlugin will load all dlls which are inside of this folder)
- The zip already contains a folder named `OverlayPlugin`, which already contains the correct folder structure and can be directly copied to your OverlayPlugin
5. Copy the GobchatUI folder into your OverlayPlugin folder (The exact position is not important, you just need to find it again)

The end result should look something like that:

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

6. Restart ACT
7. Open ACT, go to `plugin` -> `OverlayPlugin.dll`
8. Click on `new`
9. Enter a name of your choice. Select `Gobchat`under type and click ok
10. Click the button at the URL field and browse to your GobchatUI folder, select the `gobchat.html`
11. It should start up!
12. Check `Activate parsing` and it should start to show any relevant content that's coming up

### Updating Gobchat

1. Repeat steps 1 to 5 of [installing Gobchat](#Installing Gobchat), replace all files.