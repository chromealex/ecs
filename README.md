# ME.ECS
ME.ECS - it's ECS implementation for Unity Engine with full state automatic rollbacks.
In general ME.ECS should be used for multiplayer real-time strategy games games because of Network support out of the box with automatic rollbacks. You can set up tick time for your game and system should store your state and automatically sync game instances using minimum traffic (just user RPC calls, no full game sync required).
<br>
<br>

| Demo | Tutorial 01: Initialization |
| ----------- | ----------- |
| [![](https://img.youtube.com/vi/360PyjjjZTE/0.jpg)](https://www.youtube.com/watch?v=360PyjjjZTE) | [![](https://img.youtube.com/vi/qRQ2E8pv7Dk/0.jpg)](https://www.youtube.com/watch?v=qRQ2E8pv7Dk) |

| Tutorial 02: First Feature | Tutorial 02: Draw with particles |
| ----------- | ----------- |
| [![](https://img.youtube.com/vi/Y_BNGrEBJnY/0.jpg)](https://www.youtube.com/watch?v=Y_BNGrEBJnY)   | [![](https://img.youtube.com/vi/XWFI7jEHbS4/0.jpg)](https://www.youtube.com/watch?v=XWFI7jEHbS4) |

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

#### Features (```FeatureBase```)
Features are introduced for grouping systems and modules into one block.

#### Modules (```IModule<TState>```)
Modules do visual update on the beginning of the frame and on the beginning of every tick.

#### Systems (```ISystem<TState>```)
Systems do visual update at the end of the frame and on the ending of every tick.

#### Entities
Entities are containers with ID and Version.

#### Components (```IComponent<TEntity>```)
Components are storing data. In ME.ECS there are 2 component types: IComponent and IStructComponent.
IComponent could store multiple times and could be iterated by type. It is reference type and must implement CopyFrom and OnRecycle methods.
IStructComponent could store just simple types or StackArray. It is value type.

#### Markers (```IMarker```)
Markers needed to implement UI events or something that doesn't exist in game state.
<br>
<br>

## Update
![](Readme/UpdateTick.png?raw=true "Update Tick")

#### Creating State
In general you don't need to write state code, you can use **ME.ECS/Initialize Project** menu command to generate the template. State in ME.ECS is the basis which must be initialized and it's type should be present in World class for example.

#### World Initialization
> In general you don't need to write this code, you can use **ME.ECS/Initialize Project** menu command to generate the template.
```csharp
// Initialize new world with custom tick time and custom world id
// If customWorldId ignored - it will setup automatically
WorldUtilities.CreateWorld(ref this.world, 0.133f, [customWorldId]);
this.world.AddModule<StatesHistoryModule>(); // Add default states history module
this.world.AddModule<NetworkModule>();       // Add default network module
...
// Create new state and set it by default
this.world.SetState(WorldUtilities.CreateState<TState>());
// Initialize default components
ComponentsInitializer<TState>.DoInit();
this.Initialize(this.world);
// Save current world state as a Reset State.
// It's very important to do after the scene loaded and all default entities were set.
this.world.SaveResetState();
```

#### Creating Entities
For creating entities you need to run world.AddEntity().
Names are used for Editor debug information only.
```csharp
var entity = this.world.AddEntity([name]); // The same as var entity = new Entity(name);
...
```

#### Registering Prefabs
If you need to spawn view, first of all you need to register prefab source (type is based on your provider). **world.RegisterViewSource(viewPrefabSource)** could be called in system or in the feature constructor. It returns **ViewId**.
```csharp
// Register point source prefab with custom views provider
// GameObject (Will call Instantiate/Destroy)
this.pointViewSourceId = this.world.RegisterViewSource<UnityGameObjectProvider>(this.pointSource);
// Register unit source prefab with custom views provider
// Particles (Will draw particles instead of regular GameObjects)
this.unitViewSourceId = this.world.RegisterViewSource<UnityParticlesProvider>(this.unitSource);
// Register unit source prefab with auto views provider
// Here provider should be choosen by unitSource2 type
this.unitViewSourceId2 = this.world.RegisterViewSource(this.unitSource2);
```

#### Instantiating Views
To instantiate view need to call thread-safe method **world.InstantiateView(viewId, entity)**. Short method is **entity.InstantiateView(viewId)**.
> You can attach any count of views on each entity.
>
> But here are some **limitations**: for now you couldn't attach one source twice, only different sources for one entity allowed.
```csharp
this.world.InstantiateView(this.pointViewSourceId, entity);
this.world.InstantiateView(this.pointViewSourceId, entity);
```

#### Adding Systems
You can add systems inside initializer or (the better way) inside feature class. Inside feature class you can add modules and systems in constructor, also could load data for this feature.
```csharp
this.world.AddSystem<YourNewSystem>(); // The same as this.AddSystem<YourNewSystem>(); in feature class
```

#### Sending user input to world
All you need to send any user input to world - is to create Marker struct and call world.AddMarker(...) method. This could be done in modules or in UI. You shouldn't send markers inside systems because of their lifetime limit and out of state storing.
```csharp
public class MouseInputModule : IModule<TState> {
    ...
    void IModule<TState>.Update(in TState state, in float deltaTime) {
        if (Input.GetMouseButtonDown(0) == true) {
            // As usual we get ray depends on camera frustum and put it via Physics.Raycast for example
            var ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (UnityEngine.Physics.Raycast(ray, out var hitInfo, float.MaxValue, -1) == true) {
                Worlds.currentWorld.AddMarker(new WorldClick() { worldPos = hitInfo.point });
            }
        }
    }
    
}
```

#### Sending UI events to world
It's the same as the method above.
```csharp
public class YourUIBehaviour : MonoBehaviour {
    
    public Button button;
    
    public void Start() {
    
        this.button.onClick.AddListener(this.AddMarker);
    
    }
    
    ...
    
    private void AddMarker() {
        
        // Send marker click to world
        Worlds.currentWorld.AddMarker(new OnYourUIBehaviourClick());
        
    }
    
}
```

#### Receiving user input in world system
To receive data from marker, you need to get it inside Update method.
> **IMPORTANT!** Do not receive markers inside AdvanceTick, because markers lifetime is limit by current frame.
```csharp
public class UserInputReceiveSystem : ISystem<TState>, ISystemUpdate<TState> {
    ...
    void ISystemUpdate<TState>.Update(in TState state, in float deltaTime) {
        if (this.world.GetMarker(out WorldClick markerClick) == true) {
            ...
        }
    }
}
```

#### Sending and receiving RPC calls
After you have got a marker, you can easily initiate RPC call with marker data.
```csharp
public class UserInputReceiveSystem : ISystem<TState>, ISystemUpdate<TState> {
    
    // This number must determined directly
    private const int GLOBAL_RPC_ID = 1;
            
    private RPCId rpcId;
            
    void ISystemBase.OnConstruct() {
            
        // Get registered Network Module
        var networkModule = this.world.GetModule<NetworkModule>();
        // Registering this system as an RPC receiver
        networkModule.RegisterObject(this, UserInputReceiveSystem.GLOBAL_RPC_ID);

        // Register RPC call. This method returns RPCId which determines your method.
        this.rpcId = networkModule.RegisterRPC(new System.Action<WorldClick>(this.WorldClick_RPC).Method);

    }

    void ISystemBase.OnDeconstruct() {

        // Unregister object on deconstruction
        var networkModule = this.world.GetModule<NetworkModule>();
        networkModule.UnRegisterObject(this, UserInputReceiveSystem.GLOBAL_RPC_ID);

    }
    
    void ISystemUpdate<TState>.Update(in TState state, in float deltaTime) {
    
        if (this.world.GetMarker(out WorldClick markerClick) == true) {
            
            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RPC(this, this.rpcId, markerClick);
            
        }
        
    }
    
    private void WorldClick_RPC(WorldClick worldClick) {
    
        // You can use worldClick data here
        // For example set to some entity or create the new entity here
        var networkEntity = this.world.AddEntity();
        networkEntity.SetPosition(worldClick.worldPos);
    
    }
    
}
```

#### Updating Views
There are several View Providers in ME.ECS. Also you can write your own Provider implemenetation (inherit from **ViewsProvider**).

##### No View
**NoView** class need to draw something special without any view representation. For example if you have 2D grid or something, but all you need is to receive entity data and make an update. NoView provider is a default provider and it is always exists in ME.ECS.

##### Particle View
```csharp
public class ApplyYourViewStateParticle : ParticleView<ApplyYourViewStateParticle> {
        
    // You can define any public fields here
    // but note that you need override CopyFrom(T) method to properly instantiate this view from prefab
    ...
        
    public override void OnInitialize() {

        // Initialize view, link some data or cache something here

    }

    public override void OnDeInitialize() {

        // Clean up your cache data

    }

    // By default ApplyStateJob will be called from a job
    // If jobs are turned off for this provider or globaly, it will be called inside the main thread.
    public override void ApplyStateJob(float deltaTime, bool immediately) {

        // Getting root data
        ref var rootData = ref this.GetRootData();

        // Updating root particle position, rotation, scale, etc.
        rootData.position = this.entity.GetPosition();
        rootData.startSize = 1f;

        // You need to push data in any way here
        this.SetRootData(ref rootData);

    }

    // If you need to use something that couldn't be done in jobs - you could use this method
    public override void ApplyState(float deltaTime, bool immediately) {

    }

}
```

##### MonoBehaviour View
```csharp
public class ApplyYourViewStateParticle : MonoBehaviourView {
        
    // You can define any public fields here
    ...
        
    // You can turn off jobs for the certain instance
    public override bool applyStateJob => true;
        
    public override void OnInitialize() {

        // Initialize view, link some data or cache something here

    }

    public override void OnDeInitialize() {

        // Clean up your cache data

    }

    // By default ApplyStateJob will be called from a job
    // If jobs are turned off for this provider or globaly, this method would be skipped.
    public override void ApplyStateJob(UnityEngine.Jobs.TransformAccess transform, float deltaTime, bool immediately) {

        transform.position = this.entity.GetPosition();

    }

    // If you need to use something that couldn't be done in jobs - you could use this method
    public override void ApplyState(float deltaTime, bool immediately) {

    }

}
```

##### DrawMesh View
```csharp
public class ApplyYourViewStateDrawMesh : DrawMeshView<ApplyYourViewStateDrawMesh> {
        
    // You can define any public fields here,
    // but note that you need override CopyFrom(T) method to properly instantiate this view from prefab
    ...
        
    public override void OnInitialize() {

        // Initialize view, link some data or cache something here

    }

    public override void OnDeInitialize() {

        // Clean up your cache data

    }

    // By default ApplyStateJob will be called from a job
    // If jobs are turned off for this provider or globaly, it will be called inside the main thread.
    public override void ApplyStateJob(float deltaTime, bool immediately) {

        // Getting root data
        ref var rootData = ref this.GetRootData();

        // Updating root mesh position, rotation, scale, etc.
        rootData.position = this.entity.GetPosition();

        // You need to push data in any way here
        this.SetRootData(ref rootData);

    }

    // If you need to use something that couldn't be done in jobs - you could use this method
    public override void ApplyState(float deltaTime, bool immediately) {

    }

}
```
