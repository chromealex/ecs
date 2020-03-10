using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class UnitFogOfWarComponent : IComponentCopyable<TState, TEntity> {

        public ulong playersRevealed;
        public ulong playersVisible;

        public bool IsRevealed(int playerIndex) {
            
            return (this.playersRevealed & (ulong)(1 << playerIndex)) != 0;

        }

        public bool IsVisible(int playerIndex) {
            
            return (this.playersVisible & (ulong)(1 << playerIndex)) != 0;

        }

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (UnitFogOfWarComponent)other;
            this.playersRevealed = _other.playersRevealed;
            this.playersVisible = _other.playersVisible;

        }

        void IPoolableRecycle.OnRecycle() {

            this.playersRevealed = default;
            this.playersVisible = default;

        }

    }
    
}