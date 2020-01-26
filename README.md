# ME.ECS
ME.ECS - it's ECS implementation for Unity Engine with full state automatic rollbacks.
In general ME.ECS should be used for multiplayer real-time strategy games games because of Network support out of the box with automatic rollbacks. You can set up tick time for your game and system should store your state and automatically sync game instances using minimum traffic (just user RPC calls, no full game sync required).
<br>
<br>

### Demo

[![](https://img.youtube.com/vi/360PyjjjZTE/0.jpg)](https://www.youtube.com/watch?v=360PyjjjZTE)

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

### FPS Module
Module just show FPS/FPSMax/FPSMin in world viewer.
<br>
<br>

## How It Works
![](Readme/HowItWorks.png?raw=true "How It Works")
#### World (```IWorld<TState>```)
The container for all components like modules, systems, etc.
You can store multiple worlds with different states, entities, components, modules and systems.

#### State (```IState<TState>```)
User-side data storage. Just need to implement a couple of methods:
```csharp
int GetHash() // If possible returns the most unique hash
void Initialize(IWorld<State> world, bool freeze, bool restore) // Register all filter and component storages in the world
void CopyFrom(State other) // copies other state into current
void OnRecycle() // return all used resources into pools
```

#### Modules (```IModule<TState>```)
Modules do visual update on the beginning of the frame and on the beginning of every tick.

#### Systems (```ISystem<TState>```)
Systems do visual update at the end of the frame and on the ending of every tick.

#### Entities (```IEntity```)
Entities are storing base data of your objects like position, rotation, user data, etc.

#### Components (```IComponent<TEntity>```)
Components are working with a certain Entity type and implements AdvanceTick (in which you can add logic of your tick) and CopyFrom. Or you can use components like a markers. Btw, you can use IComponentOnce interface to be sure all components removed at the end of current tick.
<br>
<br>

## Update
![](Readme/UpdateTick.png?raw=true "Update Tick")

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
- Add multithreading support <b>(15% done)</b>
- Preformance refactoring <b>(70% done)</b>
