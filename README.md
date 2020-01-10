# ME.ECS
ME.ECS - it's ECS implementation for Unity Engine with full state automatic rollbacks.

# Default Modules
### States History Module
##### Submodules: IEventRunner
Store states checkpoints and all added events sorting by custom order. Can simulate world tick by tick to restore state with deterministic logic.

### Network Module
##### Submodules: ITransport, ISerializer
##### Dependencies: StatesHistoryModule
Send RPCs through any network transport implemented from the interface, serialize and deserialize data.
By default is the EventRunner for StatesHistoryModule, just send all incoming events to the network and receive events from transport and send them into StatesHistoryModule.

# How It Works
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

# Upcoming plans
- Implement automatic states history with rollback system <b>(100% done)</b>
- Decrease initialization time and memory allocs <b>(90% done)</b>
- Random support to generate random numbers, store RandomState in game state <b>(100% done)</b>
- Add full game example <b>(10% done)</b>
- Rendering system <b>(0% done)</b>
