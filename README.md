# ME.ECS
ME.ECS - it's ECS implementation for Unity Engine with full state automatic rollbacks.
In general ME.ECS should be used for multiplayer real-time strategy games games because of Network support out of the box with automatic rollbacks. You can set up tick time for your game and system should store your state and automatically sync game instances using minimum traffic (just user RPC calls, no full game sync required).
<br>
<br>

## Glossary
- [Video Tutorials](Docs/VideoTutorials.md)
- [Default Modules](Docs/DefaultModules.md)
- [Classes](Docs/Manual-Classes.md)
- [Creating World](Docs/Manual-CreatingWorld.md)
- [Creating Feature](Docs/Manual-CreatingFeature.md)
- [Creating Entity](Docs/Manual-CreatingEntities.md)
- [Instantiate View](Docs/Manual-InstantiatingViews.md)
- [Send User Input](Docs/Manual-SendingUserInputToWorld.md)
- [Send UI Events](Docs/Manual-SendingUIEventsToWorld.md)
- [Receive User Input](Docs/Manual-ReceivingUserInputInWorld.md)
- [Register Prefabs](Docs/Manual-RegisteringPrefabs.md)
- [Send and Receive RPC Calls](Docs/Manual-SendingAndReceivingRPCCalls.md)
- [Update Views](Docs/Manual-UpdatingViews.md)

## How It Works
![](Readme/HowItWorks.png?raw=true "How It Works")
## Update
![](Readme/UpdateTick.png?raw=true "Update Tick")
