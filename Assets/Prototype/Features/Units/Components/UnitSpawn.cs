using ME.ECS;

namespace Prototype.Features.Units.Components {

    public struct InitializeSquad : IStructComponent {

        public Entity owner;
        public UnityEngine.Vector3 position;
        public Prototype.Features.Units.Data.SquadData data;

    }

    public struct InitializeUnit : IStructComponent {

        public Entity owner;
        public Entity squad;
        public int indexInSquad;
        public Prototype.Features.Units.Data.UnitData data;

    }

}