using ME.ECS;

namespace Prototype.Features.Units.Components {

    public struct AttackTarget : IStructComponent {

        public Entity entity;

    }

    public struct AttackAction : IStructComponent {

        public float timer;

    }

}