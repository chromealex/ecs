# ME.ECS
ME.ECS - it's ECS implementation for Unity Engine with full state automatic rollbacks.
<br>
<br>

### Demo

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/360PyjjjZTE/0.jpg)](https://www.youtube.com/watch?v=360PyjjjZTE)

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

### Views Module
##### Submodules: IViewsProvider (Implemented: GameObjectProvider, ParticlesProvider, DrawMeshProvider)
Synchronizing current world state to views. Automatically destroy and create views (with pools), sync with current entities state and process all ticks correctly to restore visual state even objects already destroyed for a long time ago.
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
Entities are not the same like in a normal ECS architecture, in ME.ECS Entities are structs of data without any methods (Data Containers).
#### Components
Components like a functions has some data to apply on tick, working with a certain Entity type.
<br>
<br>

## Example
#### 1. World Initialization
```csharp
// Initialize new world with custom tick time and custom world id
// If customWorldId ignored - it will setup automatically
WorldUtilities.CreateWorld(ref this.world, 0.133f, [customWorldId]);
this.world.AddModule<StatesHistoryModule>(); // Add custom states history module
this.world.AddModule<NetworkModule>();       // Add custom network module
```

#### 2. State Initialization
```csharp
// Create new state and set it by default
this.world.SetState(WorldUtilities.CreateState<State>());
```

#### 3. Register Prefabs
```csharp
// Register point source prefab with custom views provider
// GameObject (Will call Instantiate/Destroy)
this.pointViewSourceId = this.world.RegisterViewSource<Point, UnityGameObjectProvider>(this.pointSource);
// Register unit source prefab with custom views provider
// Particles (Will draw particles instead of regular GameObjects)
this.unitViewSourceId = this.world.RegisterViewSource<Unit, UnityParticlesProvider>(this.unitSource);
// Register unit source prefab with auto views provider
// Here provider should be choosen by unitSource2 type
this.unitViewSourceId2 = this.world.RegisterViewSource<Unit>(this.unitSource2);
...
```

#### 4. Create Default Entities with Data
```csharp
// Create default data for all players at this level
var p1 = this.world.AddEntity(new Point() {
  position = new Vector3(0f, 0f, 3f),
  unitsCount = 99f,
  increaseRate = 1f
});
var p2 = this.world.AddEntity(new Point() {
  position = new Vector3(0f, 0f, -3f),
  unitsCount = 1f,
  increaseRate = 1f
});
...
```

#### 5. Instantiate Views (Don't worry, you can call Instantiate on any thread)
```csharp
// Attach views onto entities
// You can attach any count of views on each entity
// But here are some limitations: for now you couldn't attach one source twice, only different sources for one entity allowed.
this.world.InstantiateView<Point>(this.pointViewSourceId, p1);  // Add view with id pointViewSourceId onto p1 Entity
this.world.InstantiateView<Point>(this.pointViewSourceId, p2);  // Add view with id pointViewSourceId onto p2 Entity
...
```

#### 6. Add Systems
```csharp
// Add custom systems
this.world.AddSystem<InputSystem>();
this.world.AddSystem<PointsSystem>();
this.world.AddSystem<UnitsSystem>();
...
```

#### 7. Save Reset State
```csharp
// Save current world state as a Reset State.
// It's very important to do after the scene loaded and all default entities were set.
// Sure after that you could run any of API methods, but be sure you call them through RPC calls.
this.world.SaveResetState();
```

## Upcoming plans
- Implement automatic states history with rollback system <b>(100% done)</b>
- Decrease initialization time and memory allocs <b>(90% done)</b>
- Random support to generate random numbers, store RandomState in game state <b>(100% done)</b>
- Add full game example <b>(80% done)</b>
- Add auto sync on packets drop (TCP) <b>(100% done)</b>
- Add auto sync on packets drop (UDP) <b>(20% done)</b>
- Views module <b>(100% done)</b>
- Implement UnityGameObjectProvider <b>(100% done)</b>
- Implement UnityParticlesProvider <b>(90% done)</b> - only MeshFilter/MeshRenderer support added
- Implement UnityDrawMeshProvider <b>(90% done)</b> - only MeshFilter/MeshRenderer support added
- Add particle system simulation support on state change <b>(100% done)</b>
- Add multithreading in World class <b>(5% done)</b>
