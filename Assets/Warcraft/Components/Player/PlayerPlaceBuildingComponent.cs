using ME.ECS;

namespace Warcraft.Components.Player {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.PlayerEntity;
    
    public class PlayerPlaceBuildingComponent : IComponentCopyable<TState, TEntity> {

        public Entity player;
        public UnityEngine.Vector2 position;
        public UnityEngine.Vector2Int size;
        public ActionsGraphNode actionInfo;
        public UnitInfo unitInfo;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {
            
            var _other = (PlayerPlaceBuildingComponent)other;

            this.player = _other.player;
            this.position = _other.position;
            this.size = _other.size;
            this.actionInfo = _other.actionInfo;
            this.unitInfo = _other.unitInfo;

        }

        void IPoolableRecycle.OnRecycle() {
            
            this.player = default;
            this.position = default;
            this.size = default;
            this.actionInfo = default;
            this.unitInfo = default;

        }

    }
    
}