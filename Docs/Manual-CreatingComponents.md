# Creating Components [![](Logo-Tiny.png)](/../../#glossary)

### Struct Components

In struct components you don't have any methods and all copies make automatically.
If you store managed array here, only pointer will be copied, so if you have static array with some data you can use managed arrays here, but if you change data by your logic in these arrays, you shouldn't store any managed data here. In some cases you can use **StackArray** to allocate struct array, but there are some limitations with data size.
In order to use some arrays, you can use [**Intrusive collections**](https://github.com/chromealex/ecs-submodule/tree/master/ECS/Core/Collections/Intrusive) (IntrusiveList, IntrusiveHashSet, IntrusiveDictionary, IntrusiveStack, IntrusiveQueue) to be free of copying any struct data.

```csharp
public struct MyStructComponent : IStructComponent {
        
    public int someData;

}
```

If you need to store managed data with custom copy interface, you should use **IStructCopyable<>** component where you need to implement CopyFrom(in T other) and OnRecycle() methods.
> Use this if you are really want to use managed data and you really changes this data in your systems. In other cases use [**Intrusive collections**](https://github.com/chromealex/ecs-submodule/tree/master/ECS/Core/Collections/Intrusive).
```csharp
public struct MyStructCopyableComponent : IStructCopyable<MyStructCopyableComponent> {
        
    public int someData;
    
    void IStructCopyable<MyStructCopyableComponent>.CopyFrom(in MyStructCopyableComponent other) {
	// Do some copy work here
    }
    
    void IStructCopyable<MyStructCopyableComponent>.OnRecycle() {
    	// Return data to the pool
    }

}
```

In systems where you need to use components you could use these methods:
```csharp
void ISystemFilter.AdvanceTick(in Entity entity, in float deltaTime) {
      
    // Set data
    entity.SetData(new MyStructComponent() {
    	someData = 123
    });

    // Change data
    ref var data = ref entity.GetData<MyStructComponent>();
    ++data.someData;
    
    // Remove data
    entity.RemoveData<MyStructComponent>();
    
}
```

### Component Lifetime

For struct components there are lifetime property as described in the table below:
| Value | Description |
| ----- | ----------- |
| Infinite | Lifetime has not set, so you need to remove it manually by calling **RemoveData** method on entity. It is component default value. |
| NotifyAllSystemsBelow | If set all systems defined after executing system will be able to get this component. At the end of current tick this component will be destroyed automatically. |
| NotifyAllSystems | If set all systems will be able to get this component, but only from the begining of the next tick. At the end of next tick this component will be destroyed automatically. |
| NotifyAllModulesBelow | Is set all modules and systems defined after executing system will be able to get this component. At the end of current frame this component will be destroyed automatically. |
| NotifyAllModules | If set all modules and systems will be able to get this component, but only from the begining of the next frame. At the end of next frame this component will be destroyed automatically. |

In general you need to use **Infinite**, **NotifyAllSystemsBelow** and **NotifyAllSystems** lifetime.
```csharp
// Set struct data with Lifetime = Infinite
entity.SetData(new YourStructComponent());

// Set struct data with Lifetime = NotifyAllSystemsBelow
entity.SetData(new YourAnotherStructComponent(), ComponentLifetime.NotifyAllSystemsBelow);
```

> Note! If you set **Infinite** lifetime and after that set **non-Infinite** lifetime, **non-Infinite** will be ignored. First you need to call **RemoveData** before set another lifetime.
        
[![](Footer.png)](/../../#glossary)
