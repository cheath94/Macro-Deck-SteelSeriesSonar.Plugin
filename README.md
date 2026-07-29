# SteelSeries Sonar Plugin for Macro Deck 2

## Overview

Control SteelSeries Sonar directly from Macro Deck. This plugin provides actions for adjusting volume, muting and unmuting channels, toggling mute state, and exposes Sonar channel variables that can be used to create dynamic button states.

This plugin communicates with the local SteelSeries Sonar HTTP API.
No audio processing or virtual audio drivers are installed or modified by
this plugin.

The plugin communicates with the local SteelSeries Sonar API and does not
require administrator privileges.

## Why this plugin?

SteelSeries Sonar provides an excellent virtual audio mixer, but it does not
offer direct integration with Macro Deck.

This plugin bridges that gap by allowing Macro Deck buttons to control Sonar
channels and react to Sonar's current state using Macro Deck variables.    
	
## Features

- Control Sonar Master, Game, Chat, Mic, Media, and Aux channels
- Set channel volume to a specific level
- Increase or decrease channel volume
- Mute, unmute, and toggle channel mute
- Set, Increase, or Decrease the ChatMix
- Works with Streamer mode
- Automatic Macro Deck variable updates
- Dynamic button state support for mute status
- Uses the local SteelSeries Sonar API

## Screenshots

![Example of Buttons](docs/images/Button_Example.png)
![Adjusting Volume Configuration](docs/images/Adjust_Volume_Example.png)
![Set Specific Volume Configuration](docs/images/Set_Volume_Example.png)
![Toggle Mute Configuration](docs/images/Toggle_Mute_Example.png)
![Example of Variables](docs/images/Variable_Example.png)

## Requirements

- Windows 11
- Macro Deck 2.15 or later
- SteelSeries GG with Sonar enabled
	
## Installation

1. install the plugin according to the Macro Deck plugin installation
	process.

## Available actions

| Action                    | Description                               |
|---------------------------|-------------------------------------------|
| Set Sonar Volume          | Set Channel to specific volume            |
| Adjust Sonar Volume       | Increase or decrease volume               |  
| Mute Sonar Channel        | Mute a channel                            |
| Unmute Sonar Channel      | Unmute a channel                          |
| Toggle Sonar Mute         | Toggle a mute state                       |
| Set ChatMix		        | Set ChatMix to specific level             |
| Adjust ChatMix	        | Adjust Chatmix Level %		            |
| Set Streamer Volume       | Set specific volume (stream mode)         |
| Adjust Streamer Volume    | Increase or decrease volume (Stream mode) |
| Toggle Streamer Mute      | Toggle a mute State (Stream mode)         |
| Toggle Stream Redirection | Toggle stream routing (Stream mode)       |

	
## Available variables

| Variables                                 | Description                   |
|-------------------------------------------|-------------------------------|
| sonar_(channel)_volume                    | Float (0.0 - 1)               |
| sonar_(channel)_muted                     | Boolean                       |
| sonar_(channel)_volume_percent            | String (0-100)                |
| sonar_(channel)_volume_text               | String (0-100)%               |
| sonar_any_muted                           | Boolean                       |
| sonar_chatmix_balance                     | Float (-1 - 1)                |
| sonar_chatmix_percent                     | String (1 - 100)			    |
| sonar_chatmix_side                        | String (balanced/game/chat)   |
| sonar_chatmix_text                        | String (percent + side)       |
| sonar_monitoring_(channel)_muted          | Boolean                       |
| sonar_monitoring_(channel)_redirected     | Boolean                       |
| sonar_monitroing_(channel)_volume         | Float (0.0-1)                 |
| sonar_monitoring_(channel)_volume_percent | String (0-100)                |
| sonar_monitoring_(channel)_volume_text    | String (0-100)%               |
| sonar_streaming_(channel)_muted           | Boolean                       |
| sonar_streaming_(channel)_redirected      | Boolean                       |
| sonar_streaming_(channel)_volume          | Float (0.0-1)                 |
| sonar_streaming_(channel)_volume_percent  | String (0-100)                |
| sonar_streaming_(channel)_volume_text     | String (0-100)%               |

## Supported Channels

| Sonar Channel     | Displayed in Plugin  |
|-------------------|----------------------|
| Master            | Master               |
| Game              | Game                 |
| Chat Render       | Chat                 |
| Chat Capture      | Mic                  |
| Media             | Media                |
| Aux               | Aux                  |

## Development/build instructions

### Prerequisites

- Windows 11
- .NET 10 SDK
- SteelSeries GG with Sonar enabled
- Macro Deck 2.15.0

### Build

```pwsh
git clone <repository-url>
cd SteelSeriesSonar.Plugin
dotnet restore
dotnet build --configuration Release
```

### Troubleshooting

### Sonar is not detected

- Verify SteelSeries GG is running.
- Verify Sonar is enabled.

### Buttons do not update

- Restart Macro Deck.
- Restart SteelSeries GG.

### Plugin loads but actions fail

- Ensure Sonar is not running.

## Known limitations

- The plugin communicates with the local Sonar API, which may change between SteelSeries GG releases.

## License

This project is licensed under the MIT License.
See the LICENSE file for details.

## Disclaimer -

This project is not affiliated with, endorsed by, or sponsored by SteelSeries
or the Macro Deck project.

SteelSeries, SteelSeries GG, and Sonar are trademarks of their respective
owners. This plugin relies on locally available SteelSeries Sonar interfaces,
which may change in future SteelSeries GG releases.
