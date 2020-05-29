# Registering Prefabs
If you need to spawn view, first of all you need to register prefab source (type is based on your provider). **world.RegisterViewSource(viewPrefabSource)** could be called in system or in the feature constructor. It returns **ViewId**.
```csharp
// Register point source prefab with custom views provider
// GameObject (Will call Instantiate/Destroy)
this.pointViewSourceId = this.world.RegisterViewSource<UnityGameObjectProvider>(this.pointSource);
// Register unit source prefab with custom views provider
// Particles (Will draw particles instead of regular GameObjects)
this.unitViewSourceId = this.world.RegisterViewSource<UnityParticlesProvider>(this.unitSource);
// Register unit source prefab with auto views provider
// Here provider should be choosen by unitSource2 type
this.unitViewSourceId2 = this.world.RegisterViewSource(this.unitSource2);
```