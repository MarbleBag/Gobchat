# Gobchat (FFXIV chat overlay)
Gobchat is an overlay with the goal to provide a better chat experience for roleplayer.

## Readme needs to be rewritten for release 1.0.0
While the features stay, the way the app is installed and run changed significantly.

Gobchat in its current version is a plugin for [hibiyasleep's OverlayPlugin](https://github.com/hibiyasleep/OverlayPlugin) which itself is a plugin for
[Advanced Combat Tracker](http://advancedcombattracker.com/).

This plugin took a lot of inspiration (and some code) from [quisquous cactbot](https://github.com/quisquous/cactbot)

1. [Features](#features)
   1. [Smart Autoscroll](#smart-autoscroll)
   1. [Text color encoding](#text-formatting)
   1. [Text-Highlighting](#text-highlighting-for-key-words---mentions)
   1. [Groups](#groups)
   1. [Chat Commands](#chat-commands)
1. [Installation](#installation)
1. [Updating Gobchat](#updating-gobchat)

## Features

### Smart autoscroll
By moving the scroll bar up, autoscroll will be disabled for new messages and you can undisturbed (re)read text from the past.

![no autoscroll](docs/screen_scroll_noautoscroll.png)

By moving the scroll bar back to the bottom of the chat, autoscroll will be re-enabled!

![autoscroll reenables](docs/screen_scroll_bottom.png)

### Text formatting
(TODO)

![(missing) example screen](docs/screen3.png)

becomes

![(missing) example screen](docs/screen4.png)

Four different types 

![Different formats](docs/screen_formats.png)

### Channel settings

### Formatting settings

### Text-Highlighting for key words - mentions
Case-insensitive detection for a customizable list of words, which then will be highlighted. This will help you not missing out on important messages.

![Mentions](docs/screen_mention_highlighting.png)

### Groups
The game allows you to sort players from your friend-list into seven predefined groups. Doing so, marks said players with a special icon in your chat, making it easier to keep track of them.

Gobchat includes these groups into its styling options and allows to create as many additional groups as you want.
Each group can have a name, activated or deactivated, styled and keeps track of the players which belong to it.
It's no longer required to add players to your friend-list, just to make it easier to see what they're writing.

Groups are sorted by importance. While a player can belong to multiple groups, only the style of the first matching group is applied. To change the order, just drag & drop the group to its new position.

### Chat Commands
Gobchat accepts chat commands. To send a chat command to Gobchat, use the echo channel `/e` and type `gc` (short for Gobchat!).
Example:
- `/e gc `

Gobchat supports the following chat commands:
- [group](#group)

***

#### group
Usage:
- `/e gc group groupnumber add/remove/clear playername`

This chat command can be used to manipulate a player group without using the config menu, for example via macros.
To use the group command, type `/e gc group`.

Groupnumber is a number, starting from 1 and references the group you want to manipulate. The assigned number is identical to the position in the config menu..

Next is the task which should be performed. Possible values are `add`, `remove` and `clear`
##### clear
Doesn't need any additional  arguments. This task will remove all players from a group.
Example:
- `/e gc group 3 clear` - will remove all players from group 3

##### add
Needs the full name of a player, which will be added to the group. Names are case-insensitive!
When a player comes from a different server, it is also necessary to specify the server name in brackets. 
Placeholders like <t> are an exception to this rule and will always be accepted.

Examples:
- `/e gc group 1 add M'aka Ghin` 			/ `/e gc group 1 add firstname lastname`
- `/e gc group 1 add M'aka Ghin[ultros]` 	/ `/e gc group 1 add firstname lastname[servername]`
- `/e gc group 1 add M'aka Ghin [ultros]` 	/ `/e gc group 1 add firstname lastname [servername]`
- `/e gc group 1 add <t>`

##### remove
Needs the full name of a player, which will be removed to the group. Names are case-insensitive!
When a player comes from a different server, it is also necessary to specify the server name in brackets. 
Placeholders like <t> are an exception to this rule and will always be accepted.

Examples:
- `/e gc group 1 remove M'aka Ghin` 			/ `/e gc group 1 remove firstname lastname`
- `/e gc group 1 remove M'aka Ghin[ultros]` 	/ `/e gc group 1 remove firstname lastname[servername]`
- `/e gc group 1 remove M'aka Ghin [ultros]` 	/ `/e gc group 1 remove firstname lastname [servername]`
- `/e gc group 1 remove <t>`


## Installation

### Dependencies

Install [.NET Framework](https://www.microsoft.com/net/download/framework) version 4.7.2 or above

Install [Advanced Combat Tracker](http://advancedcombattracker.com/) 64-bit version

Install the most recent version of [ravahn's FFXIV ACT plugin](https://github.com/ravahn/FFXIV_ACT_Plugin/releases/latest) to ACT
[Guide](https://github.com/ravahn/FFXIV_ACT_Plugin/tree/master)

Install the most recent version of [hibiyasleep OverlayPlugin](https://github.com/hibiyasleep/OverlayPlugin/releases/latest) to ACT
Download the `-x64-full.zip` file, right click it and go to properties. Click `Unblock` in the bottom right corner, otherwise Windows will remove needed files on unzip.

fflogs has a video [guide](https://www.fflogs.com/help/start/) for setting up ACT

### Installing Gobchat

1. Go to [latest release](https://github.com/marblebag/gobchat/releases/latest) and download the latest version of Gobchat (zip) (Do not download the source zips, unless you want to build it yourself)
2. Right click the zip file and go to properties. In the bottom right corner of the properties menu, click `Unblock`, and then "OK" to close the menu. (zip file contains a dll)
3. Unzip the zip file
4. Copy the `addons` folder into your OverlayPlugin folder (The OverlayPlugin will load all dlls which are inside of this folder)
5. Copy the `GobchatUI` folder into your OverlayPlugin folder (The exact position is not important, you just need to find it again)

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

1. Repeat steps 1 to 5 of [installing Gobchat](#installing-gobchat), replace all files.
