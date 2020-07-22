
namespace ME.ECS {

    public static class PhysicsComponentsInitializer {
    
        public static void Init(ref ME.ECS.StructComponentsContainer structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Features.PhysicsDeterministic.Components.PhysicsRigidbody>(true);
            structComponentsContainer.Validate<ME.ECS.Features.PhysicsDeterministic.Components.PhysicsOnCollisionEnter>();
            structComponentsContainer.Validate<ME.ECS.Features.PhysicsDeterministic.Components.PhysicsOnCollisionExit>();
            structComponentsContainer.Validate<ME.ECS.Features.PhysicsDeterministic.Components.PhysicsOnCollisionStay>();

        }

        public static void Init(in Entity entity) {

            entity.ValidateData<ME.ECS.Features.PhysicsDeterministic.Components.PhysicsRigidbody>(true);
            entity.ValidateData<ME.ECS.Features.PhysicsDeterministic.Components.PhysicsOnCollisionEnter>();
            entity.ValidateData<ME.ECS.Features.PhysicsDeterministic.Components.PhysicsOnCollisionExit>();
            entity.ValidateData<ME.ECS.Features.PhysicsDeterministic.Components.PhysicsOnCollisionStay>();
            
        }

    }

}
