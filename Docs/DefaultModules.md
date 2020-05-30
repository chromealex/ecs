# Default Modules [![](Logo-Tiny.png)](/../../#glossary)

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

[![](Footer.png)](/../../#glossary)
