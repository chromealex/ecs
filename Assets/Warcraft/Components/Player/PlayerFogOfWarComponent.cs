using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.PlayerEntity;
    
    public class PlayerFogOfWarComponent : IComponentCopyable<TState, TEntity> {

        public Unity.Collections.NativeArray<byte> revealed;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (PlayerFogOfWarComponent)other;
            ArrayUtils.Copy(_other.revealed, ref this.revealed);
            
        }

        void IPoolableRecycle.OnRecycle() {

            this.revealed.Dispose();
            
        }

    }
    
}