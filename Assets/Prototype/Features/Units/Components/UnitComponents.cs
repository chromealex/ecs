using ME.ECS;

namespace Prototype.Features.Units.Components {

    public struct IsActive : IStructComponent {}

    public struct IsUnit : IStructComponent {}

    public struct IsSquad : IStructComponent {}

    public struct PickNextPointDistance : IStructComponent {

        public float value;

    }

    public struct UnitData : IStructComponent {

        public Prototype.Features.Units.Data.UnitData data;

    }

    public struct PathTraverse : IStructComponent {

        public int index;

    }

    public struct IsMoveToTargetComplete : IStructComponent { }

    public struct MoveToTarget : IStructComponent {

        public UnityEngine.Vector3 value;

    }

    public struct IsPathComplete : IStructComponent {
    }

    public struct IsPlayerCommand : IStructComponent { }

    public struct BuildPathToTarget : IStructComponent {

        public UnityEngine.Vector3 value;

    }

    public struct FirePoint : IStructComponent {

        public UnityEngine.Vector3 position;

    }

    public struct NearestNode : IStructComponent {

        public UnityEngine.Vector3 position;
        public int nodeIndex;

    }

    public struct TargetNode : IStructComponent {

        public int nodeIndex;

    }

    public struct Owner : IStructComponent {

        public Entity value;

    }

    public struct IsDead : IStructComponent {}

    public struct Health : IStructComponent {

        public float value;

    }

    public struct MaxHealth : IStructComponent {

        public float value;

    }

    public struct Acceleration : IStructComponent {

        public float value;

    }

    public struct SlowdownSpeed : IStructComponent {

        public float value;

    }

    public struct SlowdownDistance : IStructComponent {

        public float value;

    }

    public struct Speed : IStructComponent {

        public float value;

    }

    public struct MaxSpeed : IStructComponent {

        public float value;

    }

    public struct RotationSpeed : IStructComponent {

        public float value;

    }

    public struct AttackRange : IStructComponent {

        public float value;

    }

    public struct AttackSpeed : IStructComponent {

        public float value;

    }

    public struct Damage : IStructComponent {

        public float value;

    }

    public struct IsAttack : IStructComponent {}

    public struct Hit : IStructComponent {

        public float value;

    }

    public struct Squad : IStructComponent {

        public Entity entity;
        public int index;

    }

    public struct SquadSize : IStructComponent {

        public int value;

    }

    public struct SquadChilds : IStructComponent {

        public ME.ECS.Collections.StackArray10<Entity> childs;

    }

}