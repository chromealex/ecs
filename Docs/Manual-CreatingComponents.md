# Creating Components [![](Logo-Tiny.png)](/../../#glossary)
In ME.ECS there are 2 component variations: struct and class. Class components could be added multiple times on certain entity. Struct components couldn't been added twice.

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

In struct components you don't have any methods and all copies make automatically. But here are some limitations about managed arrays. If you store managed array here, only pointer will be copied, so if you have static array with some data you can use managed arrays here, but if you change data by your logic in these arrays, you shouldn't store any managed data here. In some cases you can use **StackArray** to allocate struct array, but there are some limitations.
```csharp
public struct MyStructComponent : IStructComponent {
        
    public int someData;

}
```

In systems where you need to use components you could use these methods:
```csharp
void ISystemFilter<TState>.AdvanceTick(in Entity entity, in TState state, in float deltaTime) {
        
    // Struct components usage example
    ref var data = ref entity.GetData<MyStructComponent>();
    ++data.someData;
    
    // Class components usage example
    var component = entity.GetOrAddComponent<MyComponent>();
    if (component.someArray == null) component.someArray = PoolArray<int>.Spawn(10);
    component.someArray[...] = ...;

}
```

[![](Footer.png)](/../../#glossary)
