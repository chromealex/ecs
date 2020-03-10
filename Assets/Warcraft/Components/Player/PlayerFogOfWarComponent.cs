using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.PlayerEntity;
    
    public class PlayerFogOfWarComponent : IComponentCopyable<TState, TEntity> {

        public byte[] revealed;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (PlayerFogOfWarComponent)other;
            
            if (this.revealed != null) PoolArray<byte>.Recycle(ref this.revealed);
            this.revealed = PoolArray<byte>.Spawn(_other.revealed.Length);
            System.Array.Copy(_other.revealed, this.revealed, _other.revealed.Length);

        }

        void IPoolableRecycle.OnRecycle() {

            if (this.revealed != null) PoolArray<byte>.Recycle(ref this.revealed);

        }

    }
    
}