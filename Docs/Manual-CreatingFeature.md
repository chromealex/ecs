# Creating Feature [![](Logo-Tiny.png)](/../../#glossary)
To create feature you can use menu command on folder "Features", choose **ME.ECS/Feature (Complex)** to create feature with hierarchy and **ME.ECS/Feature** to create simple feature source. Then you need to create systems (in menu **ME.ECS/System with Filter** to create system with default filter inside, these systems are support jobs out of the box). 
You can add systems inside initializer or inside feature class (best choice). Inside feature class you can add modules and systems in constructor, also could load data for this feature.
Features are ScriptableObject classes, so you need to use your **Initializer** prefab to set up right feature order and state.
System group by default exists in each feature, but actually you can create the new one by yourself, but pay attention to groups order because feature with all systems inside will play first than groups one-by-one inside this feature.
```csharp
public class YourFeature : Feature {

    ...

    protected override void OnConstruct() {

        this.AddSystem<YourNewSystem>(); // The same as this.world.AddSystem<YourNewSystem>();

        var group = new SystemGroup(); // Add new system group
        group.AddSystem<YourSystemInGroup1>();
        group.AddSystem<YourSystemInGroup2>();

    }

    ...

}
```

[![](Footer.png)](/../../#glossary)
