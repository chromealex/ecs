namespace ME.ECS.Features.PhysicsDeterministic.Components {

    public struct PhysicsRigidbody : IStructComponent {}
    
    public struct PhysicsOnCollisionEnter : IStructComponent {

        public UnityEngine.Collision collision;

    }

    public struct PhysicsOnCollisionExit : IStructComponent {

        public UnityEngine.Collision collision;

    }

    public struct PhysicsOnCollisionStay : IStructComponent {

        public UnityEngine.Collision collision;

    }

}