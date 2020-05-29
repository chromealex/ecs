# ME.ECS
ME.ECS - it's ECS implementation for Unity Engine with full state automatic rollbacks. In general ME.ECS could be used for multiplayer real-time strategy games (or any tcp-based) because of Network support out of the box with automatic rollbacks. You can set up tick time for your game and ME.ECS will store your state and automatically sync game instances using minimum traffic (just user RPC calls, no full game sync required).

## Glossary
| Link | Description |
| ------ | ----- |
| [Video Tutorials](Docs/VideoTutorials.md) | Here are some links to youtube channel which shows how to use some features |
| [Classes](Docs/Manual-Classes.md) | All classes and structures used in ME.ECS |
| [Default Modules](Docs/DefaultModules.md) | Describe default modules included in ME.ECS by default |
| [Create World](Docs/Manual-CreatingWorld.md) | Describe how to create new world and set up your empty project |
| [Create Feature](Docs/Manual-CreatingFeature.md) | How to create new feature |
| [Create Entity](Docs/Manual-CreatingEntities.md) | How to create new entity |
| [Create Component](Docs/Manual-CreatingComponents.md) | How to create new component |
| [Create Filter](Docs/Manual-CreatingFilters.md) | How to create new filter |
| [Register Prefabs](Docs/Manual-RegisteringPrefabs.md) | How to register prefab in ME.ECS |
| [Instantiate View](Docs/Manual-InstantiatingViews.md) | How to instantiate view from previously registered prefab |
| [Update Views](Docs/Manual-UpdatingViews.md) | How to update views with entity data |
| [Send User Input](Docs/Manual-SendingUserInputToWorld.md) | How to send user inputs to systems |
| [Send UI Events](Docs/Manual-SendingUIEventsToWorld.md) | How to send events from UI to systems |
| [Receive User Input](Docs/Manual-ReceivingUserInputInWorld.md) | How to receive markers in systems |
| [Send and Receive RPC Calls](Docs/Manual-SendingAndReceivingRPCCalls.md) | How to register object in **NetworkModule**, send and receive RPC packages |
| [Defines](Docs/Defines.md) | Define usage |

## How It Works
![](Readme/HowItWorks.png?raw=true "How It Works")
## Update
![](Readme/UpdateTick.png?raw=true "Update Tick")
