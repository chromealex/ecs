# ME.ECS
ME.ECS - it's ECS implementation for Unity Engine with full state automatic rollbacks.
<br>
<br>

### Demo
[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/WR4ohL_FtRs/0.jpg)](https://www.youtube.com/watch?v=WR4ohL_FtRs)

(Click image to play)

## Default Modules
### States History Module
##### Submodules: IEventRunner
Store states checkpoints and all added events sorting by custom order. Can simulate world tick by tick to restore state with deterministic logic.

### Network Module
##### Submodules: ITransport, ISerializer
##### Dependencies: StatesHistoryModule
Send RPCs through any network transport implemented from the interface, serialize and deserialize data.
By default is the EventRunner for StatesHistoryModule, just send all incoming events to the network and receive events from transport and send them into StatesHistoryModule.
<br>
<br>

## How It Works
![](Readme/HowItWorks.png?raw=true "How It Works")
#### World
The container for all components like modules, systems, etc.
#### State
User-side data storage. Just need to implement a couple of methods.
#### Modules
Modules are living out of the determinism, runing according on world time.
#### Systems
Systems are living inside of the determinism, but could implement two variants of Deterministic and Non-Deterministic logic (for example user inputs).
#### Entities
Entities are not the same like in a normal ECS architecture, in ME.ECS Entities are structs of data without any methods.
#### Components
Components has no data, but has a small part of logic, working with a sertain Entity type.
<br>
<br>

## World Initialization
```csharp
// Create new world
WorldUtilities.CreateWorld(ref this.world, 0.033f); // Initialize new world with custom tick time
this.world.AddModule<StatesHistoryModule>(); // Add custom states history module
this.world.AddModule<NetworkModule>();       // Add custom network module

// Initialize default state
this.world.SetState(this.world.CreateState()); // Create new state and set it by default

// Create default data
this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 1f, increaseRate = 1f });

// Add systems
this.world.AddSystem<InputSystem>();
this.world.AddSystem<PointsSystem>();

// Save current world state as a Reset State,
// it's very important to do after the scene loaded
// and all default entities are set.
this.world.SaveResetState();
```

## Upcoming plans
- Implement automatic states history with rollback system <b>(100% done)</b>
- Decrease initialization time and memory allocs <b>(90% done)</b>
- Random support to generate random numbers, store RandomState in game state <b>(100% done)</b>
- Add full game example <b>(80% done)</b>
- Add auto sync on packets drop <b>(0% done)</b>
- Rendering system <b>(0% done)</b>
