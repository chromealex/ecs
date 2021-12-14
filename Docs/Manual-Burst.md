# Burst [![](Logo-Tiny.png)](/../../#glossary)

Filters in ME.ECS can be used with burst compiler. There are two variants to use burst to iterate filter.

### Use Filter Bag (Recommended)

This method can be used with job system:
```csharp
public Filter filter; // for example we use filter define

void AdvanceTick(in float deltaTime) {
  
    // Here we define new filter bag with 2 components (up to 9 components)
    var bag = new FilterBag<Component1, Component2>(this.filter, Allocator.Temp);
    
    // Schedule the job with your bag
    var job = new Job() { bag = bag };
    job.Schedule(bag.Length, 64).Complete();
    
    // Push changes to world
    // Or you can discard changes by calling Revert method
    bag.Push();
  
}

[BurstCompile(FloatPrecision.High, FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
public struct Job : IJobParallelFor {

    public FilterBag<Component1, Component2> bag;
    
    public void Execute(int index) {
      
        // Read first component in bag and change its value
        ref var component1 = ref this.bag.GetT0(index);
        component1.data = ...;
        
        // Here you can read, change or remove components defined in bag
        
    }
  
}
```

### Use ForEach

This method has several restrictions: 
* Components can't be removed
* ForEach couldn't been run in parallel mode
```csharp
this.filter.ForEach((in Entity entity, in Component1 component1, ref Component2 component2) => {
    
    
    
}).WithBurst();
```

[![](Footer.png)](/../../#glossary)
