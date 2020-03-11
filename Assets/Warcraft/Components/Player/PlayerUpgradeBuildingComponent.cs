using ME.ECS;

namespace Warcraft.Components.Player {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.PlayerEntity;

    public class PlayerUpgradeBuildingComponent : IComponentCopyable<TState, TEntity> {

        public Entity player;
        public Entity selectedUnit;
        public ActionsGraphNode actionInfo;
        public UnitInfo unitInfo;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (PlayerBuildingQueueComponent)other;

            this.player = _other.player;
            this.selectedUnit = _other.selectedUnit;
            this.actionInfo = _other.actionInfo;
            this.unitInfo = _other.unitInfo;

        }

        void IPoolableRecycle.OnRecycle() {

            this.player = default;
            this.selectedUnit = default;
            this.actionInfo = default;
            this.unitInfo = default;

        }

    }
    
}