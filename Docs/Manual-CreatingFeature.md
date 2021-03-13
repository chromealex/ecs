# Creating Feature [![](Logo-Tiny.png)](/../../#glossary)
To create feature you can use menu command on folder "Features", choose **ME.ECS/Feature (Complex)** to create feature with hierarchy and **ME.ECS/Feature** to create simple feature source.

You can add systems inside initializer or inside feature class (best choice). Inside feature class you can add modules and systems in constructor, also could load data for this feature.

Features are ScriptableObject classes, so you need to use your **Initializer** prefab to set up right feature order and state.
System group by default exists in each feature, but actually you can create the new one by yourself, but pay attention to groups order because feature with all systems inside will play first than groups one-by-one inside this feature.
```csharp
public class YourFeature : Feature {

    protected override void InjectFilter(Filter filter) {
    
        // Here you can add some restrictions for all filters inside all systems laying inside this feature
            
    }

    protected override void OnConstruct() {

        this.AddSystem<YourNewSystem>(); // The same as this.world.AddSystem<YourNewSystem>();

        // Optionally you can create system group (read above about groups order inside features)
        var group = new SystemGroup(); // Add new system group
        group.AddSystem<YourSystemInGroup1>();
        group.AddSystem<YourSystemInGroup2>();

    }
    
    protected override void OnDeconstruct() {
        
        // Unload data, you don't need to remove systems and modules at this point, just unload your custom data if it was loaded on OnConstruct() stage
        
    }

}
```

[![](Footer.png)](/../../#glossary)
