# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com)

## [1.12.3] - 2025.04.03
### Fixed
- `Check own messages for mentions` in party chat
- Rangefilter should work again with 7.2

### Removed
- Exclude Mentions from Rangefilter checkbox in `Config / Rangefilter`
  - This was the same checkbox as in `Config / Mentions` 

## [1.12.2] - 2023.07.09
### Fixed
- Blink animation on Tabs

- Broken chat if you have an empty group

## [1.12.1] - 2023.05.04
### Fixed
- chat formatting for say and emote

## [1.12.0] - 2023.05.03
### Added
- Font size
  - There is now a wider range of sizes to choose from - or just enter whatever number you like


- Customizable gap between chat messages


- Customizable chat UI size
  - UI scales with menu font size, independently of your font size for chat messages


- Config
  - UI scales automatically with the width of the dialog


- Previous messages are updated if group settings are changed


- Tabs can be filtered by groups


- Color pickers for channel specific senders


### Changed
- Some settings were split into their own tabs
  - Chat log
  - Range filter
  - Formatting

- Chat log
  - by default enabled

### Fixed
- Chat position and size
  - The values in config will now update if you move your chat around (hover with your mouse over the chat and press ctrl-key, you can drag and resize it now!)
  - It's now possible to enter negative values for multi-monitor setups

- Gobchat will now recognize more FF14 auto translated terms

## [1.11.4] - 2023.01.13
### Fixed
With 6.3 Gobchat was unable to identify player characters from the game. While no error message was shown, features like rangefilter and `Check own messages for mentions` were not working previously.

- Rangefilter

- Mentions


## [1.11.3] - 2022.08.03
### Added
- Chat log
  - A new keyword {sender-cha} which writes the sender name as seen in game. For example; tells will have their '>>', emote won't use colons, etc
  - Added a new predefined log format which will use {sender-cha}
  - {sender-cha} not supported by log converter


### Fixed
- Gobchat was unable to identify a character as the sender of a chat message, if said character had one or more hyphens in its name.


- Rangefilter
  - The filter should no longer ignore names with hyphens


- Mentions
  - `Check own messages for mentions` should work as expected for a character with hyphens in their name


## [1.11.2] - 2022.04.15
### Fixed
- Rangefilter
  - memory signature updated

## [1.11.1] - 2021.09.30
### Fixed
- An issue where the order of elements, if changed without adding or removing, was not saved
  - tabs, groups, etc.


- An issue where the chat log format was not saved to a new log file


- An issue where the reset button for channel colors didn't work

##  [1.11.0] - 2021.09.09
### Added
- Chat log
  - Change which channels are logged in `Config / Channel`
  - Customizable chat log format in `Config / App`
    - Details in readme under `Chat Log`


- LogConverter
  - Supports customizable chat log format `CCLv1`


- Message culling for 'outdated' messages is back
  - It uses FFXIV's time now


## [1.10.0] - 2021.04.14
### Added
- Chat command: `config open`
  - Opens the `Config` dialog


- Chat command: `config reset frame`
  - Resets the overlay position and size to default


- More localization (`Config`, `Chat commands`)


- Off-screen protection
  - If moved off-screen, Gobchat reverts back to its last position
  - If it's still off-screen, Gobchat resets position and size


- Tray icon
  - Button to reset overlay position and size to default

### Removed
- Message culling for 'outdated' messages
  - Gobchat no longer removes messages with an outdated timestamp (if your system clock goes ahead, every message was outdated, until the game caught up)
  - Needs to be tested

### Changed
- Chat command: `profile` changed to `profile load`


- `Config / App / Chat position and size`
  - Fields `X & Y` allow negative values

### Fixed
- Rangefilter
  - Now utilizes the whole opacity range from start to end

## [1.9.0] - 2020.09.08
### Added
- Hotkeys
  - Button to remove hotkeys


- Support for multi-boxing
  - By default Gobchat shows the chat of the first process of FFXIV it can find
  - It's possible to select a specific process of FFXIV in `Config / App / Multi-boxing`


- Chat log
  - The output folder of chat logs can be changed on a profile level in `Config / App`


- Config
  - Button to close Gobchat
    - Gobchat can now be closed via config, tray icon (right click) or chat command (/e gc close)


### Removed
- Limit of 10 chat entries per update
  - It no longer chokes on battle logs

### Fixed
- LogConverter

- Newly created profiles were not saved without changes to the config

## [1.8.0] - 2020.08.17
### Added
- Chat tabs
  - Configuration can be found in `Config / Chat tabs`
  - Supports any number of tabs.
    - To navigate all tabs either use the buttons on the side of the tab bar or a scroll wheel while hovering it (Remember: Gobchat needs focus to detect a scroll wheel)
  - A yellow dot marks the active tab
  - Inactive tabs change their appearance on new messages or mentions
     - Can be changed in `Config / Chat tabs`  


- Chat
  - Color selector for chat frame background color in `Config / App`


- More font sizes
  - Smaller, larger and very large


- More localization

### Changed
- Profile selection drop down shows its content sorted


- Rangefilter
  - Removed check box for activation in `Config / App`


- Config / chat tabs
  - Rows in the tab table can be clicked directly to open the tab config      


- Javascript dialogs
  - Replaced by proper html dialogs

### Fixed
- Roleplay formatting
  - In some cases colors weren't applied correctly


- FFXIV error messages
  - Will no longer show up with an empty colon at the front


- Chat position (X, Y) in `Config / App` will now accept 0 as a value


## [1.7.1] - 2020.07.18
### Fixed
- Groups
  - Delete button for custom groups did not work


- Updater
  - Download status text will now show the correct number of downloaded bytes
  - Archive will be deleted if an error occurs while unpacking to avoid the same error on restart due to a corrupted archive

## [1.7.0] - 2020.07.16
### Added
- Theme selection in `Config / App`
  - Available  themes are ffxiv dark / light and a kind of Gobchat legacy


- A checkbox in `Config / app` to (de)activate mention scanning in your own messages
  - If turned on, Gobchat will mark any mention it can find in your message
  - If turned off, Gobchat will not mark them, unless another user uses them.


- Chat commands: `info off`, `info on`
  - `info off` will suppress any Gobchat info until turned back on via `info on` or restart of Gobchat


- Chat commands: `error off`, `error on`
  - `error off` will suppress any Gobchat errors until turned back on via `error on` or restart of Gobchat


- Language selection dropdown in `Config / App`
  - Most of Gobchat is translated into german


- Tons of new tooltips

### Changed
- Merged channels which are used by NPCs to talk into a single channel

### Fixed
- An error on `Config / Roleplay` where `reset entries` stops working after one use
  - This bug also affected some other reset buttons

## [1.6.3] - 2020.06.05
### Added
- Color selection to channels: NPC dialog, animated emote, echo

### Fixed
- groups
  - color reset buttons didn't work
  - ffxiv specific groups had their colors wrong


- hotkey: hide & show
  - Previously set hotkey could become stuck on profile reset


- profiles
  - A newly created profile could not be immediately activated without pressing save in between
  - Profiles which were created and deleted in the same session were not correctly deleted from disk and were loaded again at the next start

## [1.6.2] - 2020.05.26
### Fixed
- Messages not shown
  - A message which ends on one or more letters of a multi-letter token for roleplay, like '((', could cause an exception which leads to Gobchat not displaying the message

## [1.6.1] - 2020.05.23
### Fixed
- Profiles were not loaded
  - Player who used 1.5.2 but not any beta version of 1.6.0 experienced problems with their profiles

## [1.6.0] - 2020.05.21
### Added
- `Character location updates`
  - Gobchat tries to get information about your character and nearby players.
  - This feature is optional and can be deactivated in `Config / App`


- Chat command: `player count`
  - Counts nearby players.
  - Requires `Character location updates`


- Chat command: `player list`
  - Lists nearby players and their distance to you.
  - Requires `Character location updates`


- Chat command: `player distance`
  - Shows distance (in yalms) to a player.
  - Requires `Character location updates`


- Range filter
  - Applies (or hides) messages from players depending on their distance to you. The affected channels can be changed in `Config / Channels`. Will only work for players which are nearby. By default, deactivated and can be activated in `Config / App`
  - Requires `Character location updates`


- ChatLog converter
  - Can be found in the root directory and can be used to convert old chatlogs into the new format

### Changed
- Chatlogs are now easier to read by splitting message header from content


- Your message will no longer be scanned for any mentions
  - Requires: `Character location updates`

## [1.5.2] - 2020.03.22
### Added
- A small border to the left side of the chat
- Profile import & export

### Fixed
- Some grammatical errors in labels and tool-tips
- Command 'group': A Name containing '-' will now be saved correctly
- Error on download of beta versions

## [1.5.1] - 2020.03.02
### Added
- Another button `Add new group` in `Config / Groups` at the bottom, which will attach a new group at the end
- `Textsearch`, it's now possible to search through the chat. Gobchat will highlight all entries which fit the search term and allows stepping through them.
- A new button to open `textsearch` at the top of the chat
- Input boxes to set position and size of the chat in `Config / App`
- A `Save & Exit` button in `Config`
- Color picker for `textsearch` in `Config / App`

### Changed
- `Add new group` in `Config / Groups` will now insert a new group at the beginning
- `Save` in `Config` will no longer close `Config`

### Fixed
- Gobchat can now distinguish between different beta releases by parsing the pre-release version

## [1.4.1] - 2020.02.19
### Fixed
- Gobchat should work again - FFXIV uses new control characters since patch 5.2

## [1.4.0] - 2020.02.03
### Added
- Chat command to close gobchat. Use: /e gc close
- Message on config save
- Profiles will be saved directly to disk on save now
- Gobchat now uses a wider array of characters to detect say and emote, they can be configured in the roleplay tab

### Fixed
- Hotkeys were not applied on change, only after restarting Gobchat
- Config dialog can only be opened once at the same time

## [1.3.1] - 2020.01.20
### Fixed
- Some typos
- Command group add / remove will now tell the user the result of the command


## [1.3.0] - 2020.01.10
### Added
- Sound on mention! The mention tab was extended and now includes settings to play a sound file
- Gobchat can now hide itself when FFXIV gets minimized

### Fixed
- On profile delete the profile file will also be deleted from the filesystem, so it won't be loaded again
- Profile names can be changed

## [1.2.1] - 2020.01.05
### Fixed
- Changed settings will be applied immediately again, just how it should be!

## [1.2.0] - 2020.01.05
### Bug
- It's currently necessary to start Gobchat with admin rights, otherwise it can't read FF chatlog.

### Added
- Profiles! It's now possible to have more settings per settings, so you can twink your twink
- Chat command to switch between profiles without open the settings. Try /e gc profile
- A checkbox which allows gobchat to be updated with pre-releases / beta-releases
- Gobchat will now communicate some of its problems with the user. Be nice
- Two new channels for Gobchat to report errors and inform the user about stuff
- Color selector for FFXIV ECHO and ERROR channel
- On using '/e gc' gobchat will now tell you which commands are available

## [1.1.0] - 2019.12.06
### Added
- Auto-Updater
- Field to set the frequency of chat updates in milliseconds (Experimental)
- Additional sections to the german readme

### Changed
- Chatlogs names are changed from yyyy_MM_dd_HH_mm to yyyy-MM-dd_HH-mm

### Fixed
- Lost messages when a high amount of messages from other players are received

## [1.0.0] - 2019.11.27
### Changed
- Gobchat is now a stand-alone application

### Added
- Checks for a new version of gobchat on start-up
- Tray Icon! It even can be clicked. Watch out!
- Support for ffxiv's autotranslate. (English)
- Chat History to file

## [0.2.2] - 2019.10.27
### Added
- Channel for npc dialogue

### Fixed
- Players didn't show up, when their last name contained an apostrophe/additional uppercase letter and gobchat didn't know the datacenter yet

## [0.2.1] - 2019.10.22
### Added
- Tooltips to some interactable fields
- Party 'Random / Roll / Dice' channel

### Changed
- Group delete requires a confirmation
- Delete buttons have now a proper icon
- Reset buttons have now a proper icon
- Merged all 'Random / Roll / Dice' channels into one

## [0.2.0] - 2019.10.18
### Added
- Player groups. It is now possible to make own groups, which get activated on certain players to style their message further
- Player groups are sorted. Only the first group that matches will be applied.
- Server name. If someone comes from another server, the overlay tries to display their server separate from their last name
- A chat command manager. To start a chat command use '/e gc'
- chat command 'group'. Allows to add, remove and clear player groups. Example: /e gc group 1 add firstname lastname [server] (Server is only needed if the player comes from a different server than you!)

### Fixed
- Checkboxes on the plugin control form have now the correct size

## [0.1.5] - 2019.10.11
### Added
- Channels for random rolls

### Fixed
- Automatic Stylesheet generation was not triggered on a fresh start with no settings
- No messages if mentions are empty
- Party chat should work again

## [0.1.4] - 2019.10.10
### Added
- Color can be changed for each (cross-world)-linkshell separately
- Friendlist Groups can now be colorized too!

## [0.1.3] - 2019.10.09
### Added
- Config button to overlay. On click it will open a config dialog
- Detection for 'ooc' comments. A ooc-comment starts with (( and ends with ))
- Detection for 'emote' in say. When quotation marks are used to mark speech in say, everything that's not enclosed in quotation marks will be seen as emote
- Mention config to config dialog
- Channel color selection to config dialog
- Channel visibility to config dialog
- Channel roleplay formatting to config dialog
- Basic font selection

### Changed
- Moved mention config from plugin to overlay

### Fixed
- Multiple whitespaces in chat are not removed anymore

## [0.1.2] - 2019.09.28
### Fixed
- Error on mentions, because of typo in javascript
- Autoscroll

## [0.1.1] - 2019.09.27
### Fixed
- Mentions should work better, even while enclosed by non-alphanumeric letters

## [0.1.0] - 2019.09.25
### Added
- A fixed roleplay format which will be applied to roleplay channels
- A fixed set of roleplay channels: Say (/s), Emote (/em), Yell (/y)
- A fixed color encoding for channels
- A scrollbar to overlay
- Ability to resize overlay
- Ability to set overlay to visible/hidden
- Ability to set overlay to activated/deactivated (If deactivated, the plugin does not process any input)
- Ability to set mentions
