# Creating Components [![](Logo-Tiny.png)](/../../#glossary)

### Struct Components

In struct components you don't have any methods and all copies make automatically.
If you store managed array here, only pointer will be copied, so if you have static array with some data you can use managed arrays here, but if you change data by your logic in these arrays, you will get sync problems. In some cases you can use **StackArray** to allocate struct array, but there are some limitations with data size. To use managed types in struct components you need to use ```[GeneratorIgnoreManagedType]``` attribute to avoid generator errors.
In order to use some arrays, you can use [**Intrusive collections**](https://github.com/chromealex/ecs-submodule/tree/master/ECS/Core/Collections/Intrusive) (IntrusiveList, IntrusiveHashSet, IntrusiveDictionary, IntrusiveStack, IntrusiveQueue, etc.) to be free of copying any struct data. Also you can use DataArray<T> collection without Copyable component.

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
    entity.Set(new MyStructComponent() {
    	someData = 123
    });

    // Read data (Here you can read data only, entity version has not been changed)
    ref readonly var dataReadonly = ref entity.Read<MyStructComponent>();
    
    // Change data (Any call of GetData inside Logic step will trigger entity version to change)
    ref var data = ref entity.Get<MyStructComponent>();
    ++data.someData;
    
    // Remove data
    entity.Remove<MyStructComponent>();
    
}
```

### Shared Components

Shared components are the same as default components, but this feature support shared component groups.
For example: you have an Entity1 and Entity2, also you have Component1 that you would like to add to Entity1 and to Entity2, but you don't want to create data copy for each entity, so you can store one copy for these entities.
Also you have an option to store shared components by the groupId. Provide groupId to store a copy for each unique groupId.

```csharp
struct MySharedComponent : IComponentShared {
    ...
}

void Example() {

    ...
    ref readonly var c = ref entity.ReadShared<MySharedComponent>([groupId]); // Read data if exist
    ref var c = ref entity.GetShared<MySharedComponent>([groupId]); // Get or create data for this entity
    // If you change component data, all entities with the same groupId will be changed
    entity.RemoveShared<MySharedComponent>([groupId]); // Remove data for this entity

}
```

### Static Components

Useful with [DataConfigs](https://github.com/chromealex/ecs/blob/master/Docs/DataConfig-Readme.md) only.
If you don't want to store some components on entity, but you still want to get data by reading data configs - you can provide IComponentStatic interface to avoid any applying on entity.

```csharp
struct MyStaticComponent : IComponentStatic {
    ...
}

void Example() {

    ...
    var data = config.Get<MyStaticComponent>(); // Read data from DataConfig
    
    var emptyData = entity.Read<MyStaticComponent>(); // Fail! Returns an empty data

}
```

### IComponentRuntime

Useful with [DataConfigs](https://github.com/chromealex/ecs/blob/master/Docs/DataConfig-Readme.md) only.<br>
To be able to add only data components and to avoid components marked ```IComponentRuntime``` you can use this interface.

### IComponentInitializable

Useful with [DataConfigs](https://github.com/chromealex/ecs/blob/master/Docs/DataConfig-Readme.md) only.<br>
Sometimes you need to call initialization method for components, so you need to use ```IComponentInitializable``` interface.

### Versioned Components

In ME.ECS you could track version of component type by calling ```entity.GetDataVersion<TComponent>()```, it would return world's tick when component has changed on this entity. To enable this feature for component type, you should define ```IVersioned interface at your component:
	
```csharp
struct MyComponent : IStructComponent, IVersioned {
    ...
}
```

### Versioned Components (Non-state)

In ME.ECS you could track version of component type by calling entity.GetDataVersionNoState<TComponent>()```, it would return incremented number each time component has changed on this entity. To enable this feature for component type, you should define **IVersionedNoState** interface at your component:
	
```csharp
struct MyComponent : IStructComponent, IVersionedNoState {
    ...
}
```

### Component Lifetime

For struct components there are lifetime property as described in the table below:
| Value | Description |
| ----- | ----------- |
| Infinite | Lifetime has not set, so you need to remove it manually by calling **RemoveData** method on entity. It is component default value. |
| NotifyAllSystemsBelow | If set all systems defined after executing system will be able to get this component. At the end of current tick this component will be destroyed automatically (see [custom lifetime](https://github.com/chromealex/ecs/blob/master/Docs/Manual-CreatingComponents.md#component-custom-lifetime) section). |
| NotifyAllSystems | If set all systems will be able to get this component, but only from the begining of the next tick. At the end of next tick this component will be destroyed automatically (see [custom lifetime](https://github.com/chromealex/ecs/blob/master/Docs/Manual-CreatingComponents.md#component-custom-lifetime) section). |
| NotifyAllModulesBelow | Is set all modules and systems defined after executing system will be able to get this component. At the end of current frame this component will be destroyed automatically. |
| NotifyAllModules | If set all modules and systems will be able to get this component, but only from the begining of the next frame. At the end of next frame this component will be destroyed automatically. |

In general you need to use **Infinite**, **NotifyAllSystemsBelow** and **NotifyAllSystems** lifetime.
```csharp
// Set struct data with Lifetime = Infinite
entity.Set(new YourStructComponent());

// Set struct data with Lifetime = NotifyAllSystemsBelow
entity.Set(new YourAnotherStructComponent(), ComponentLifetime.NotifyAllSystemsBelow);
```

> Note! If you set **Infinite** lifetime and after that set **non-Infinite** lifetime, **non-Infinite** will be ignored. First you need to call **RemoveData** before set another lifetime.

### Component Custom Lifetime

In some cases you need to set up custom lifetime for component, so you can use ```entity.Set(new YourStructComponent(), ComponentLifetime.NotifyAllSystemsBelow, lifetimeInSeconds)``` to determine how long your component should be alive. Note that NotifyAllSystems or NotifyAllSystemsBelow should work as expected because this parameter controls only creation time of the component.
        
[![](Footer.png)](/../../#glossary)
