# Creating Markers [![](Logo-Tiny.png)](/../../#glossary)
To create marker you can choose **ME.ECS/Marker** menu.
For registering markers you need to run world.AddMarker(...).
Markers unlike struct components could store any type of data, you **do not need** to implement CopyFrom method because markers not store in State.
You should call AddMarker in modules or in UI because marker has a one-frame lifetime.
When you add marker to world, world systems **Update** should be able to get marker and send data via RPC, systems couldn't change the State in Update method, just run RPC call.
```csharp
public struct YourMarker : IMarker {
    
    public Vector3 worldPos;
    public int[] otherData;
    
}
```

[![](Footer.png)](/../../#glossary)
