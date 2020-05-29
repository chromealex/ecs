# Receiving user input in world system
To receive data from marker, you need to get it inside Update method.
> **IMPORTANT!** Do not receive markers inside AdvanceTick, because markers lifetime are limited by current frame.
```csharp
public class UserInputReceiveSystem : ISystem<TState>, ISystemUpdate<TState> {
    ...
    void ISystemUpdate<TState>.Update(in TState state, in float deltaTime) {
        if (this.world.GetMarker(out WorldClick markerClick) == true) {
            ...
        }
    }
}
```
