# Changelog


## [1.1.0] - 2026-07-29

### Added

#### Streamer Mixer Support

* Added support for the Sonar Streamer Mixer.
* Added actions to:
  * Set Streamer Volume
  * Adjust Streamer Volume
  * Toggle Streamer Mute
* Added support for both Streaming and Monitoring outputs.

#### Stream Redirections

* Added Toggle Stream Redirection action.
* Added monitoring of Stream Redirection state.
* Added Macro Deck variables for Streaming and Monitoring redirection status.

#### Stream Output Monitoring

* Added Toggle Stream Output Monitoring action.
* Added automatic monitoring of the Stream Output Monitoring state.
* Added `sonar_stream_monitoring_enabled` variable.

#### ChatMix

* Added ChatMix monitoring.
* Added ChatMix variables:

  * `sonar_chatmix_balance`
  * `sonar_chatmix_percent`
  * `sonar_chatmix_side`
  * `sonar_chatmix_text`

#### Streamer Variables

Added automatic variables for all supported Streamer Mixer channels, including:

* Volume
* Volume Percentage
* Volume Text
* Mute State

for both:

* Streaming Output
* Monitoring Output

#### Automatic Monitoring

Expanded the Sonar monitor to automatically poll:

* Classic Mixer
* Streamer Mixer
* ChatMix
* Stream Redirections
* Stream Output Monitoring

All monitored values are automatically published as Macro Deck variables whenever their state changes.

---

### Changed

#### Variable System

* Refactored the variable management system into dedicated components:
  * Classic Variables
  * ChatMix Variables
  * Streamer Variables
  * Stream Redirection Variables
* Reduced duplicate code and improved maintainability.
* Improved separation of responsibilities within the monitoring architecture.

#### Streamer Polling

* Added bulk Streamer state retrieval.
* Reduced the number of HTTP requests required during each monitoring cycle.

#### ChatMix API Compatibility

* Updated ChatMix API support to use the new SteelSeries GG v116.0.0 endpoint:

  `/v1/chatMix`


#### Internal Improvements

* Continued refactoring of the Sonar client.
* Improved code organization and helper methods.
* Improved consistency between Classic Mixer and Streamer Mixer implementations.

---

### Fixed

* Improved compatibility with recent SteelSeries GG release v116.0.0.
* Fixed ChatMix support after SteelSeries moved the endpoint to `/v1/chatMix`.
* Improved monitoring reliability for Streamer Mixer state.
* Improved monitoring reliability for Stream Redirection state.
* Improved monitoring reliability for Stream Output Monitoring state.

---

## [1.0.0]

### Initial Release

#### Classic Mixer

* Added support for the Sonar Classic Mixer.
* Added actions to:
  * Set Volume
  * Adjust Volume
  * Mute Channel
  * Unmute Channel
  * Toggle Mute

#### Variable Support

* Added automatic Macro Deck variables for:
  * Channel Volume
  * Volume Percentage
  * Mute State
  * Any Channel Muted

#### Sonar Integration

* Implemented automatic discovery of the local SteelSeries Sonar service.
* Implemented communication with the Sonar local HTTP API.
* Added support for automatic monitoring of Classic Mixer state.

#### Infrastructure

* Initial Macro Deck plugin architecture.
* Sonar client implementation.
* Variable monitoring system.
* Automatic variable updates.
* Initial project documentation.
