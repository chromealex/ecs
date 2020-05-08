using ME.ECS;

namespace Prototype.Features.Units.Components {

    public struct InitializeBullet : IStructComponent {

        public Entity owner;
        public Entity target;
        public UnityEngine.Vector3 position;
        public Prototype.Features.Units.Data.BulletData data;

    }

    public struct IsBullet : IStructComponent {
    }
    
}