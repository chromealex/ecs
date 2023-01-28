# Views [![](Logo-Tiny.png)](/../../#glossary)
In ME.ECS **Logic** layer and **Presentation** layer are separated. You can completely turn off the view (presentation) layer and your logic would run the same as before. Entities themselves do not have a visual representation, instead you can provide a `View` to represent a certain entity. 

There are several View Providers in ME.ECS. Also you can write your own Provider implementation (inherit from **ViewsProvider**).

##### No View
**NoView** class is needed to draw something special without any visual representation. For example, if you have 2D grid or something, but all you need is to receive entity data and make an update. NoView provider is a default provider and it is always exists in ME.ECS.

##### MonoBehaviourView
```csharp
public class SomeMonoBehaviourView : MonoBehaviourView {
        
    // You can define any public fields here
    ...
        
    // You can turn off jobs for the certain instance
    public override bool applyStateJob => true;
        
    public override void OnInitialize() {

        // Initialize view, link some data or cache something here

    }

    public override void OnDeInitialize() {

        // Clean up your cache data

    }

    // By default ApplyStateJob will be called from a job
    // If jobs are turned off for this provider or globally, this method would be skipped.
    public override void ApplyStateJob(UnityEngine.Jobs.TransformAccess transform, float deltaTime, bool immediately) {

        transform.position = this.entity.GetPosition();

    }

    // If you need to use something that couldn't be done in jobs - you could use this method
    // This method calls on entity change
    public override void ApplyState(float deltaTime, bool immediately) {

    }

    // This method calls every frame
    public override void OnUpdate(float deltaTime) {
    
    }

}
```

##### ParticleView
```csharp
public class ApplySomeParticleViewStateParticle : ParticleView<ApplySomeParticleViewStateParticle> {
        
    // You can define any public fields here
    // but note that you need override CopyFrom(T) method to properly instantiate this view from prefab
    ...
        
    public override void OnInitialize() {
        // Initialize view, link some data or cache something here
    }
    public override void OnDeInitialize() {
        // Clean up your cache data
    }
    // By default ApplyStateJob will be called from a job
    // If jobs are turned off for this provider or globally, it will be called inside the main thread.
    public override void ApplyStateJob(float deltaTime, bool immediately) {
        // Getting root data
        ref var rootData = ref this.GetRootData();
        // Updating root particle position, rotation, scale, etc.
        rootData.position = this.entity.GetPosition();
        rootData.startSize = 1f;
        // You need to push data in any way here
        this.SetRootData(ref rootData);
    }
    // If you need to use something that couldn't be done in jobs - you could use this method
    // This method calls on entity change
    public override void ApplyState(float deltaTime, bool immediately) {
    }
    
    // This method calls every frame
    public override void OnUpdate(float deltaTime) {
    
    }
}
```
##### DrawMesh View
```csharp
public class ApplySomeDrawMeshViewStateDrawMesh : DrawMeshView<ApplySomeDrawMeshViewStateDrawMesh> {
        
    // You can define any public fields here,
    // but note that you need override CopyFrom(T) method to properly instantiate this view from prefab
    ...
        
    public override void OnInitialize() {
        // Initialize view, link some data or cache something here
    }
    public override void OnDeInitialize() {
        // Clean up your cache data
    }
    // By default ApplyStateJob will be called from a job
    // If jobs are turned off for this provider or globally, it will be called inside the main thread.
    public override void ApplyStateJob(float deltaTime, bool immediately) {
        // Getting root data
        ref var rootData = ref this.GetRootData();
        // Updating root mesh position, rotation, scale, etc.
        rootData.position = this.entity.GetPosition();
        // You need to push data in any way here
        this.SetRootData(ref rootData);
    }
    // If you need to use something that couldn't be done in jobs - you could use this method
    // This method calls on entity change
    public override void ApplyState(float deltaTime, bool immediately) {
    }
    // This method calls every frame
    public override void OnUpdate(float deltaTime) {
    
    }
}
```
## Instantiating a View [![](Logo-Tiny.png)](/../../#glossary)
First, you need to register a prefab source (type is based on your provider). **world.RegisterViewSource(viewPrefabSource)** could be called in systems or in the Feature constructor. It returns **ViewId**.
```csharp
// Register source prefab with auto views provider
// Provider will be choosen by sourceViewPrefab type
this.someViewId = this.world.RegisterViewSource(this.someViewPrefab);
```
Then, to instantiate a view - you need to call a thread-safe method **entity.InstantiateView(viewId)**.
> Every entity can have only one view at a time. If you try to Instantiate second view on entity, that already has view component, you will get an exception.
> If you want to change the view on the entity - you should use **entity.DestroyAllViews()** before calling InstantiateView or call **entity.ReplaceView(viewId);**.
```csharp
entity.InstantiateView(this.someId);
```
## Initializing a View from a Scene
You can also create a view on the scene and initialize an entity for it. To do so, create an object on the scene and attach one of three scripts **MonoViewInitializer**, **ParticleViewInitializer** or **DrawMeshViewInitializer**. In there you can specify `EntityFlags` to define entity behaviour, apply `DataConfig`, define view behaviour after the entity would be destroyed, and you need to provide a source View to initialize. This initializer would automatically register a ViewSource and create an entity for it.

```csharp
public class MonoViewInitializer : SceneViewInitializer {

        public MonoBehaviourView view;
        
        protected override void OnInitialize(World world, in Entity entity) {
            
            if (this.view != null) {
                var viewId = world.RegisterViewSource(this.view);
                entity.AssignView(viewId, this.destroyViewBehaviour);
            }
            
        }

    }
```

## Interpolation
> **Note**
> Useful with MonoBehaviourView only.

ME.ECS has interpolation in views by default. You just need to turn on interpolation in your view settings. There are default speed settings, but you can override them by implementing these two methods:
```csharp
// Movement speed
public override float GetInterpolationMovementSpeed() => this.entity.Read<YourMovementSpeedComponent>().value;
// Rotation speed
public override float GetInterpolationRotationSpeed() => this.entity.Read<YourRotationSpeedComponent>().value;
```

[![](Footer.png)](/../../#glossary)
