# Creating Components [![](Logo-Tiny.png)](/../../#glossary)
In ME.ECS there are 2 component variations: struct and class. Class components could be added multiple times on certain entity. Struct components couldn't been added twice.

### Class Components

In class components you need to implement **CopyFrom** method and copy your data manually. And you need **OnRecycle** implementation to clean up component from your data. It's very important to implement these two methods. Is you need to store complex data like managed arrays or so you need to use these components type.
```csharp
public class MyComponent : IComponentCopyable {
        
    public int[] someArray;

    void IComponentCopyable.CopyFrom(IComponentCopyable other) {

        var _other = (MyComponent)other;
	ArrayUtils.Copy(_other.someArray, ref this.someArray);

    }
       
    void IPoolableRecycle.OnRecycle() {

        PoolArray<int>.Recycle(ref this.someArray);

    }

}
```

In systems where you need to use components you could use these methods:
```csharp
void ISystemFilter.AdvanceTick(in Entity entity, in float deltaTime) {
    
    // Add new component or get if this type exists.
    // Note that you can add these types more than once.
    var component = entity.GetOrAddComponent<MyComponent>();
    if (component.someArray == null) component.someArray = PoolArray<int>.Spawn(10);
    component.someArray[...] = ...;

    // Add components (more than once)
    entity.AddComponent<ClassComponent1>();
    entity.AddComponent<ClassComponent1>();
    entity.AddComponent<ClassComponent1>();
    
    // Existance check
    if (entity.HasComponent<ClassComponent1>() == true) {
    	// exists
    }
    
    // No allocation here
    var components = entity.ForEachComponent<ClassComponent1>();
    
    entity.RemoveComponents<ClassComponent1>();
    
    // An example how to use common types to use foreach
    entity.AddComponent<ClassComponent1, ICommonInterface>();
    entity.AddComponent<ClassComponent2, ICommonInterface>();
    var components = entity.ForEachComponent<ICommonInterface>();
    
}
```

### Struct Components

In struct components you don't have any methods and all copies make automatically. But here are some limitations about managed arrays. If you store managed array here, only pointer will be copied, so if you have static array with some data you can use managed arrays here, but if you change data by your logic in these arrays, you shouldn't store any managed data here. In some cases you can use **StackArray** to allocate struct array, but there are some limitations.
```csharp
public struct MyStructComponent : IStructComponent {
        
    public int someData;

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
