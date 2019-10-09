# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com)

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