# Timers [![](Logo-Tiny.png)](/../../#glossary)

In ME.ECS there are timers which give you an ability to store custom timer per entity and custom index.<br>
Timers will be updated automatically and always tick backwards from your value to zero.<br>
You can check timer value every time you want to get actual timer value.<br>
Timer value storing per entity and your custom index in the state class.
<br>
<br>
For example:
```csharp
entity.SetTimer(yourCustomTimerIndex, time); // set new timer value or replace existing value with the new one
var value = entity.ReadTimer(yourCustomTimerIndex); // value = time
ref var newValue = ref entity.GetTimer(yourCustomTimerIndex); // value = time
newValue = 0f; // at the next tick after settings 0 value - timer will be destroyed for this entity
entity.Remove(yourCustomTimerIndex); // returns true
```

[![](Footer.png)](/../../#glossary)
