# Updating Views [![](Logo-Tiny.png)](/../../#glossary)
There are several View Providers in ME.ECS. Also you can write your own Provider implementation (inherit from **ViewsProvider**).

##### No View
**NoView** class need to draw something special without any view representation. For example if you have 2D grid or something, but all you need is to receive entity data and make an update. NoView provider is a default provider and it is always exists in ME.ECS.

##### Particle View
```csharp
public class ApplyYourViewStateParticle : ParticleView<ApplyYourViewStateParticle> {
        
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

##### MonoBehaviour View
```csharp
public class ApplyYourViewStateParticle : MonoBehaviourView {
        
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

##### DrawMesh View
```csharp
public class ApplyYourViewStateDrawMesh : DrawMeshView<ApplyYourViewStateDrawMesh> {
        
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

[![](Footer.png)](/../../#glossary)
