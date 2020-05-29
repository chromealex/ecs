# Classes
#### Entities
Entities are containers with ID and Version.

#### Components (```IComponent<TEntity>```)
Components are storing data. In ME.ECS there are 2 component types: IComponent and IStructComponent.
IComponent could store multiple times and could be iterated by type. It is reference type and must implement CopyFrom and OnRecycle methods.
IStructComponent could store just simple types or StackArray. It is value type.
[How to create a component](Manual-CreatingComponents.md)

#### Systems (```ISystem<TState>```)
Systems do visual update at the end of the frame and on the ending of every tick.
[How to create a system](Manual-CreatingSystems.md)

#### Features (```FeatureBase```)
Features are introduced for grouping systems and modules into one block. Features are ScriptableObjects and could be ordered in Initializer on your scene.
[How to create a feature](Manual-CreatingFeatures.md)

#### Modules (```IModule<TState>```)
Modules do visual update on the beginning of the frame and on the beginning of every tick. Here you can get controller input and create some markers.

#### Markers (```IMarker```)
Markers needed to implement Controller/UI events or something that doesn't exist in game state.
