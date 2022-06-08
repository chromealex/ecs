# Creating Modules [![](Logo-Tiny.png)](/../../#glossary)
To create module you can choose **ME.ECS/Module** menu.
Modules are the same as Systems, but modules has an Update **before** logic ticks. So modules are ideally for getting input and creating markers.
For example if you have several input devices like mouse, keyboard, joystick, etc., you need to create module for each input device.
```csharp
public class YourModule : IModule, IUpdate {
    
    public World world { get; set; }
    
    void IModuleBase.OnConstruct() {}
    
    void IModuleBase.OnDeconstruct() {}
    
    void IUpdate.Update(float deltaTime) {
        
        // Read some input and place marker to world
        if (Input.GetMouseButtonDown(0) == true) {
            this.world.AddMarker(new ButtonDownMarker());
        }
        
    }
    
}
```

[![](Footer.png)](/../../#glossary)
