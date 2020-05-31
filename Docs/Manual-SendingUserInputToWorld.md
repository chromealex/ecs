# Sending user input to world [![](Logo-Tiny.png)](/../../#glossary)
All you need to send any user input to world is to create Marker struct and call world.AddMarker(...) method. This could be done in modules or in UI. You shouldn't send markers inside systems because of their lifetime limit and out of state storing.
```csharp
public class MouseInputModule : IModule, IUpdate {
    ...
    void IUpdate.Update(in float deltaTime) {
        if (Input.GetMouseButtonDown(0) == true) {
            // As usual we get ray depends on camera frustum and put it via Physics.Raycast for example
            var ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (UnityEngine.Physics.Raycast(ray, out var hitInfo, float.MaxValue, -1) == true) {
                Worlds.currentWorld.AddMarker(new WorldClick() { worldPos = hitInfo.point });
            }
        }
    }
    
}
```

[![](Footer.png)](/../../#glossary)
