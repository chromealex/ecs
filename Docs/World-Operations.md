# World Operations [![](Logo-Tiny.png)](/../../#glossary)

### Update World

```csharp
world.PreUpdate(dt);  // Update world's input modules and input systems
world.Update(dt);     // Simulate world
world.LateUpdate(dt); // Update visual
```

### Automatic World Simulation

To simulate world properly you need to set time since start. Generally you need to call this method every time your server time is updated.
```csharp
world.SetTimeSinceStart(timeFromStartInSeconds);
```
All other things like rollbacks, simulation process, calculating ticks, etc. will be done automatically.
Btw, world's time will be increased automatically by Unity deltaTime, but at the long distance you will get too different results,
so it is recommended to use SetTimeSinceStart API as often as you can.

### Manual World Simulation

NetworkModule controls simulation by default, but you want to set it up manually you can choose how you want to run simulation.
```csharp
// This method set up [from..to) ticks and simulation will be executed in the World::Update() call
world.SetFromToTicks(from, to);
```
```csharp
// This method run simulation immediatelly
world.Simulate(from, to);
```
> **Warning**<br>
> You need to understand what you are doing, because simulation/resimulation can cause sync problems.

NetworkModule uses ```UpdateLate``` event to receive events and apply them in history.
Then it should apply all events from history which are didn't run yet. So the first thing NetworkModule checks for oldest unplayed event in history list.
Next step it will make the rollback if event was older than current tick.
NetworkModule at this point uses ```World::Simulate(fromStateTick, currentTick)``` and then set ```World::SetFromToTicks(currentTick, targetTick)``` to finish simulation on current frame.

### Replay Controls

You have an option to rewind world to any tick you want if you have all events on board already.
```csharp
world.RewindTo(targetTick, doVisualUpdate); // Rewind to targetTick immediately

// Async rewinder allow to simulate world smoothly with maxSimulationTime delay per batch
world.RewindToAsync(targetTick, doVisualUpdate, [onState = null], [maxSimulationTime = 1f]);
```

### Replay Mode

In ME.ECS you can run network module in Replay mode, which doesn't allow to send any RPC events.
```csharp
networkModule.SetReplayMode(true);
```

[![](Footer.png)](/../../#glossary)
