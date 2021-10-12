# Creating System [![](Logo-Tiny.png)](/../../#glossary)
To create system you can use menu command on folder "Systems", choose **ME.ECS/System with Filter** to create system with predefined filter + jobs support (with **ISystemJobs**) and **ME.ECS/System** to create simple system source. 
You can add systems inside initializer or inside feature class (best choice). Inside feature class you can add modules and systems in constructor, also could load data for this feature.

## System with Filter (ISystemFilter)

In **System with Filter** you should define default filter and jobs behaviour:
```csharp
public class YourSystem : ISystemFilter {
    
    public World world { get; set; }
    
    void ISystemBase.OnConstruct() {
        // Here you can initialize additional filters, load some data, or cache references to features
    }
    
    void ISystemBase.OnDeconstruct() {
        // Here you need to deinitialize all your data
    }
    
    Filter ISystemFilter.filter { get; set; }
    Filter ISystemFilter.CreateFilter() {
        
	// Here you should define default system filter
        return Filter.Create("Filter-YourSystem").[...].Push();
        
    }

    void ISystemFilter.AdvanceTick(in Entity entity, in float deltaTime) {

        // Place your logic here

    }

}
```

In **System** you have Update and AdvanceTick methods to implement your logic.
In Update method you shouldn't implement any logic depends on your state (get world marker is the best choice). In AdvanceTick you shouldn't implement getting markers for example.

## Jobs (ISystemJobs)

**ISystemJobs** interface provide some parameters which allow to start job automatically with your filter (Works with ISystemFilter only).
```csharp
public class YourSystem : ISystemFilter, ISystemJobs {
    
    ...
    
    int ISystemJobs.jobsBatchCount => 64; // If you have ISystemJobs, how it should be batched?
    
    ...

    void ISystemFilter.AdvanceTick(in Entity entity, in float deltaTime) {

        // This method will call inside job

    }

}
```

[![](Footer.png)](/../../#glossary)
