# Classes
#### Entities
Entities are containers with ID and Version.

#### Components (```IComponent<TEntity>```)
Components are storing data. In ME.ECS there are 2 component types: IComponent and IStructComponent.
IComponent could store multiple times and could be iterated by type. It is reference type and must implement CopyFrom and OnRecycle methods.
IStructComponent could store just simple types or StackArray. It is value type.

#### Systems (```ISystem<TState>```)
Systems do visual update at the end of the frame and on the ending of every tick.

#### Features (```FeatureBase```)
Features are introduced for grouping systems and modules into one block.

#### Modules (```IModule<TState>```)
Modules do visual update on the beginning of the frame and on the beginning of every tick.

#### Markers (```IMarker```)
Markers needed to implement UI events or something that doesn't exist in game state.