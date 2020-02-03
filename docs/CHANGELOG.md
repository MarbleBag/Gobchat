# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com)

## [1.4.0] - 2020.02.03
## Added
- Chat command to close gobchat. Use: /e gc close
- Message on config save
- Profiles will be saved directly to disk on save now
- Gobchat now uses a wider array of characters to detect say and emote, they can be configured in the roleplay tab

## Fixed
- Hotkeys were not applied on change, only after restarting Gobchat
- Config dialog can only be opened once at the same time

## [1.3.1] - 2020.01.20
## Fixed
- Some typos
- Command group add / remove will now tell the user the result of the command


## [1.3.0] - 2020.01.10
## Added
- Sound on mention! The mention tab was extended and now includes settings to play a sound file
- Gobchat can now hide itself when FFXIV gets minimized

## Fixed
- On profile delete the profile file will also be deleted from the filesystem, so it won't be loaded again
- Profile names can be changed

## [1.2.1] - 2020.01.05
## Fixed
- Changed settings will be applied immediately again, just how it should be!

## [1.2.0] - 2020.01.05
## Bug
- It's currently necessary to start Gobchat with admin rights, otherwise it can't read FF chatlog.

## Added
- Profiles! It's now possible to have more settings per settings, so you can twink your twink
- Chat command to switch between profiles without open the settings. Try /e gc profile
- A checkbox which allows gobchat to be updated with pre-releases / beta-releases
- Gobchat will now communicate some of its problems with the user. Be nice
- Two new channels for Gobchat to report errors and inform the user about stuff
- Color selector for FFXIV ECHO and ERROR channel
- On using '/e gc' gobchat will now tell you which commands are available

## [1.1.0] - 2019.12.06
## Added
- Auto-Updater
- Field to set the frequency of chat updates in milliseconds (Experimental)
- Additional sections to the german readme

## Changed
- Chatlogs names are changed from yyyy_MM_dd_HH_mm to yyyy-MM-dd_HH-mm

## Fixed
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
