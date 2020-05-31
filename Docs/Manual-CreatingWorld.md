# Creating World [![](Logo-Tiny.png)](/../../#glossary)
> Create an empty directory, select it and choose **ME.ECS/Initialize Project** to initialize and set up your new project.
> 
> At this page all information and code for understanding only, you do not need to use this manually.

#### Creating State
In general you don't need to write state code, you can use **ME.ECS/Initialize Project** menu command to generate the template. State in ME.ECS is the basis which must be initialized and it's type should be present in World class for example.

#### State (```State```)
User-side data storage. Just need to implement a couple of methods:
```csharp
int GetHash() // If possible returns the most unique hash
void Initialize(World world, bool freeze, bool restore) // Register all filter and component storages in the world
void CopyFrom(State other) // copies other state into current
void OnRecycle() // return all used resources into pools
```
#### World Initialization
> In general you don't need to write this code, you can use **ME.ECS/Initialize Project** menu command to generate the template.
```csharp
// Initialize new world with custom tick time and custom world id
// If customWorldId ignored - it will setup automatically
WorldUtilities.CreateWorld<TState>(ref this.world, 0.133f, [customWorldId]);
this.world.AddModule<StatesHistoryModule>(); // Add default states history module
this.world.AddModule<NetworkModule>();       // Add default network module
...
// Create new state and set it by default
this.world.SetState<TState>(WorldUtilities.CreateState<TState>());
// Initialize default components
ComponentsInitializer.DoInit();
this.Initialize(this.world);
// Save current world state as a Reset State.
// It's very important to do after the scene loaded and all default entities were set.
this.world.SaveResetState<TState>();
```

[![](Footer.png)](/../../#glossary)
