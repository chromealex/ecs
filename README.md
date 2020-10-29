<a href="https://github.com/chromealex/ecs"><img src="Docs/Logo.png" width="300px" height="150px" align="left" /></a>

<div><h4>ME.ECS - it's ECS implementation for Unity Engine with full state automatic rollbacks. In general ME.ECS could be used for multiplayer real-time strategy games (or any tcp-based) because of Network support out of the box with automatic rollbacks. You can set up tick time for your game and ME.ECS will store your state and automatically sync game instances using minimum traffic (just user RPC calls, no full game sync required).</h4></div>

<br>

[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/chromealex/ecs/blob/master/LICENSE)

## Example Repository
https://github.com/chromealex/ecs.example

## Glossary
| Link | Description |
| ------ | ----- |
| [Video Tutorials](Docs/VideoTutorials.md) | Here are some links to youtube channel which shows how to use some features |
| [Classes](Docs/Manual-Classes.md) | All classes and structures used in ME.ECS |
| [Deterministic Operations](Docs/Manual-Deterministic.md) | Deterministic Operations like Random and GetHashCode in collections |
| [Default Modules](Docs/DefaultModules.md) | Describe default modules included in ME.ECS by default |
| [Create World](Docs/Manual-CreatingWorld.md) | Describe how to create new world and set up your empty project |
| [Create Feature](Docs/Manual-CreatingFeature.md) | How to create new feature |
| [Create System](Docs/Manual-CreatingSystems.md) | How to create new system |
| [Create Module](Docs/Manual-CreatingModules.md) | How to create new module |
| [Create Entity](Docs/Manual-CreatingEntities.md) | How to create new entity |
| [Create Component](Docs/Manual-CreatingComponents.md) | How to create new component |
| [Create Filter](Docs/Manual-CreatingFilters.md) | How to create new filter |
| [Create Marker](Docs/Manual-CreatingMarkers.md) | How to create new marker |
| [Register Prefabs](Docs/Manual-RegisteringPrefabs.md) | How to register prefab in ME.ECS |
| [Instantiate View](Docs/Manual-InstantiatingViews.md) | How to instantiate view from previously registered prefab |
| [Update Views](Docs/Manual-UpdatingViews.md) | How to update views with entity data |
| [Send User Input](Docs/Manual-SendingUserInputToWorld.md) | How to send user inputs to systems |
| [Send UI Events](Docs/Manual-SendingUIEventsToWorld.md) | How to send events from UI to systems |
| [Receive User Input](Docs/Manual-ReceivingUserInputInWorld.md) | How to receive markers in systems |
| [Send and Receive RPC Calls](Docs/Manual-SendingAndReceivingRPCCalls.md) | How to register object in **NetworkModule**, send and receive RPC packages |
| [Defines](Docs/Defines.md) | Define usage |
| [Data Configs](Docs/DataConfig-Readme.md) | How to create data config files |
| [Code Generators](Docs/CodeGenerators.md) | Code Generators usage |
| [Pathfinding Package](https://github.com/chromealex/ecs-submodule/tree/master/ECSPathfinding) | Here is Pathfinding Package readme |
| [Serializer Package](https://github.com/chromealex/ecs-submodule/tree/master/ECSSerializer) | Here is Serializer Package readme |

## Contact Me
Telegram: https://t.me/chromealex</br>
E-Mail: chrome.alex@gmail.com</br>
Facebook: https://www.facebook.com/chrome.alex</br>

## How It Works
![](Readme/HowItWorks.png?raw=true "How It Works")
## Update
![](Readme/UpdateTick.png?raw=true "Update Tick")
