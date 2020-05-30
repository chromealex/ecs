# Registering Prefabs [![](Logo-Tiny.png)](/../../#glossary)
If you need to spawn view, first of all you need to register prefab source (type is based on your provider). **world.RegisterViewSource(viewPrefabSource)** could be called in system or in the feature constructor. It returns **ViewId**.
```csharp
// Register source prefab with auto views provider
// Provider will be choosen by sourceViewPrefab type
this.sourceViewId = this.world.RegisterViewSource(this.sourceViewPrefab);
```

[![](Footer.png)](/../../#glossary)
