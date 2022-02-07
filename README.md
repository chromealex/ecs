<a href="https://github.com/chromealex/ecs"><img src="Docs/Logo.png" width="300px" height="150px" align="left" /></a>

<div><h4>ME.ECS - it's ECS implementation for Unity Engine with full state automatic rollbacks. In general ME.ECS could be used for multiplayer real-time strategy games (or any tcp-based) because of Network support out of the box with automatic rollbacks. You can set up tick time for your game and ME.ECS will store your state and automatically sync game instances using minimum traffic (just user RPC calls, no full game sync required).</h4></div>

<br>

[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/chromealex/ecs/blob/master/LICENSE)

## Installation

### Using submodule

1. Download or add as a submodule this repository https://github.com/chromealex/ecs-submodule.
2. Create an empty file called ```csc.gen.rsp``` inside Assets folder. Your file should have the path ```Assets/csc.gen.rsp```.
3. Add packages (or see [Package Dependencies](#package-dependencies) section):
    1. In Package Manager install ```Burst``` (tested with version 1.5.3).
    2. In Package Manager install ```Unity Collections``` (tested with version 1.0.0-pre.3). Note: to find unity collections you need to choose "Add package from git URL" and fill it with ```com.unity.collections```.
    3. In Package Manager install ```Unity Profiling Core``` (tested with version 1.0.0). Note: to find unity profiling core you need to choose "Add package from git URL" and fill it with ```com.unity.profiling.core```.
    4. In Package Manager install ```Unity UI Toolkit``` (tested with version 1.0.0-preview.16). Note: to find unity ui toolkit you need to choose "Add package from git URL" and fill it with ```com.unity.ui```.
    5. In Package Manager install ```Unity Mathematics``` (tested with version 1.2.1).
    6. In Package Manager install ```Unity Addressables``` (tested with version 1.18.13).
    7. In Package Manager install ```Unity Localization``` (tested with version 1.0.5). Note: to find unity collections you need to choose "Add package from git URL" and fill it with ```com.unity.localization```.
4. Be sure your submodule folder has a name ```Assets/ecs-submodule``` or ```Assets/ECS-submodule```.
5. You are ready to [Initialize Project](Docs/VideoTutorials.md).

### Using Unity Package Manager

1. Create an empty file called ```csc.gen.rsp``` inside Assets folder. Your file should have the path ```Assets/csc.gen.rsp```.
2. Open ```Packages/manifest.json``` file.
3. Add ME.ECS to your dependencies section:

```
{
  "dependencies": {
    [HERE ARE OTHER PACKAGES]
    "com.me.ecs": "https://github.com/chromealex/ecs-submodule.git"
  }
}
```
4. You are ready to [Initialize Project](Docs/VideoTutorials.md).

### Package Dependencies

```
  "com.unity.collections": "1.0.0-pre.6",
  "com.unity.ui": "1.0.0-preview.18",
  "com.unity.addressables": "1.19.9",
  "com.unity.burst": "1.5.6",
  "com.unity.mathematics": "1.2.1",
  "com.unity.localization": "1.0.5",
  "com.unity.profiling.core": "1.0.0"
```

## Example Repository
https://github.com/chromealex/ecs.example

## Submodule Repository
https://github.com/chromealex/ecs-submodule

## FAQ
[FAQ](Docs/FAQ.md)

## Glossary
| Link | Description |
| ------ | ----- |
| [Video Tutorials](Docs/VideoTutorials.md) | Here are some links to youtube channel which shows how to use some features |
| [Classes](Docs/Manual-Classes.md) | All classes and structures used in ME.ECS |
| [Deterministic Operations](Docs/Manual-Deterministic.md) | Deterministic Operations like Random and GetHashCode in collections |
| [Burst](Docs/Manual-Burst.md) | How to use burst |
| [Default Modules](Docs/DefaultModules.md) | Describe default modules included in ME.ECS by default |
| [Create World](Docs/Manual-CreatingWorld.md) | Describe how to create new world and set up your empty project |
| [Create Feature](Docs/Manual-CreatingFeature.md) | How to create new feature |
| [Create System](Docs/Manual-CreatingSystems.md) | How to create new system |
| [Create Module](Docs/Manual-CreatingModules.md) | How to create new module |
| [Create Entity](Docs/Manual-CreatingEntities.md) | How to create new entity |
| [Create Component](Docs/Manual-CreatingComponents.md) | How to create new component |
| [Create Filter](Docs/Manual-CreatingFilters.md) | How to create new filter |
| [Create Marker](Docs/Manual-CreatingMarkers.md) | How to create new marker |
| [Create Timers](Docs/Manual-Timers.md) | How to create timers |
| [Register Prefabs](Docs/Manual-RegisteringPrefabs.md) | How to register prefab in ME.ECS |
| [Instantiate View](Docs/Manual-InstantiatingViews.md) | How to instantiate view from previously registered prefab |
| [Update Views](Docs/Manual-UpdatingViews.md) | How to update views with entity data |
| [Send User Input](Docs/Manual-SendingUserInputToWorld.md) | How to send user inputs to systems |
| [Send UI Events](Docs/Manual-SendingUIEventsToWorld.md) | How to send events from UI to systems |
| [Receive User Input](Docs/Manual-ReceivingUserInputInWorld.md) | How to receive markers in systems |
| [Send and Receive RPC Calls](Docs/Manual-SendingAndReceivingRPCCalls.md) | How to register object in **NetworkModule**, send and receive RPC packages |
| [Defines](Docs/Defines.md) | Define usage |
| [Data Configs](Docs/DataConfig-Readme.md) | How to create and use data configs |
| [Global Events](Docs/GlobalEvents-Readme.md) | How to create and use global events |
| [Code Generators](Docs/CodeGenerators.md) | Code Generators usage |
| [Essentials](https://github.com/chromealex/ecs-submodule/tree/master/Essentials) | Here are essential packages for ME.ECS |
| [Addons](https://github.com/chromealex/ecs-submodule/tree/master/Addons) | Here are addon packages for ME.ECS |
| [Pathfinding Package](https://github.com/chromealex/ecs-submodule/tree/master/Essentials/Pathfinding) | Here is Pathfinding Package readme |
| [Serializer Package](https://github.com/chromealex/ecs-submodule/tree/master/Essentials/Serializer) | Here is Serializer Package readme |
| [Data Config Generator](https://github.com/chromealex/ecs-submodule/tree/master/Addons/DataConfigGenerator) | Data Config Generator with Google Spreadsheets |

## Discord
https://discord.gg/SxJJPPNsSf

## Contact Me
Telegram: https://t.me/chromealex</br>
E-Mail: chrome.alex@gmail.com</br>
Facebook: https://www.facebook.com/chrome.alex</br>

## How It Works
![](Readme/HowItWorks.png?raw=true "How It Works")
## Update
![](Readme/UpdateTick.png?raw=true "Update Tick")
