# Creating Feature ![](Logo-Tiny.png)
To create feature you can use menu command on folder "Features", choose **ME.ECS/Feature (Complex)** to create feature with hierarchy and **ME.ECS/Feature** to create simple feature source. Then you need to create systems (in menu **ME.ECS/System with Filter** to create system with default filter inside, these systems are support jobs out of the box). 
You can add systems inside initializer or inside feature class (best choice). Inside feature class you can add modules and systems in constructor, also could load data for this feature.
Features are ScriptableObject classes, so you need to use your **Initializer** prefab to set up right feature order and state.
```csharp
public class YourFeature : Feature<TState> {

    ...

    protected override void OnConstruct() {

        this.AddSystem<YourNewSystem>(); // The same as this.world.AddSystem<YourNewSystem>();

    }

    ...

}
```
