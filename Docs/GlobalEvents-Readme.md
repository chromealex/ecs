# Global Events [![](Logo-Tiny.png)](/../../#glossary)
ME.ECS has Global Event files to use events outside the ECS logic step.
To add Global Event object to your project, choose **Create/ME.ECS/Global Event** menu and create your event file.
In event file you can set up another events to be called before this event.

For example if you need to change UI button state when your player's health changes, you can fire the event inside any **AdvanceTick** step life this:
```csharp
void AdvanceTick(float deltaTime) {
  ...
  this.feature.testEvent.Execute();
  ...
}
```

In your feature's inspector you must to set up `testEvent` link to be sure it's not null.
In your UI script you need to set up subscribtion onto your `testEvent`:
```csharp
public GlobalEvent testEvent;
void Start() {
  
  this.testEvent.Subscribe(this.TestEventCallback);
  
}

void OnDestroy() {
  this.testEvent.Unsubscribe(this.TestEventCallback);
}

private void TestEventCallback(in Entity entity) {
  // Do some work
}
```

> Note, that Execute method doesn't run immediatelly, so it's wait until ticks job has been ended.

```csharp
GlobalEvent::Execute(); // Make a call with Entity.Empty and GlobalEventType.Visual
GlobalEvent::Execute(in entity); // Make a call with GlobalEventType.Visual
GlobalEvent::Execute(in entity, GlobalEventType.Visual); // Make a call after all ticks work is done
GlobalEvent::Execute(in entity, GlobalEventType.Logic); // Make a call after current tick work is done
```

> To make a call immediatelly you should use **GlobalEvent::Run()** method, but note that it is not recommended.

> Please note, that Execute events works only once by unique GlobalEvent keys. For example: if you call Execute on the same GlobalEvent instance twice - only one event would be fired.

You can cancel event don't want to call this event anymore:
```csharp
void AdvanceTick(float deltaTime) {
  ...
  this.feature.testEvent.Cancel();
  ...
}
```
All Global Events run once and removed after this call, so you shoudn't do Cancel work manually.

[![](Footer.png)](/../../#glossary)
