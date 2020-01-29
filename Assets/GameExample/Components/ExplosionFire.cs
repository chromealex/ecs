using ME.ECS;

namespace ME.GameExample.Game.Components {
    
    using TEntity = ME.GameExample.Game.Entities.Explosion;
    
    public class ExplosionFire : IComponent<GameState, TEntity> {

        void IPoolableRecycle.OnRecycle() {}

        void IComponent<GameState, TEntity>.AdvanceTick(GameState state, ref TEntity data, float deltaTime, int index) {

            data.lifetime -= deltaTime;
            
        }
        
        void IComponent<GameState, TEntity>.CopyFrom(IComponent<GameState, TEntity> other) {}
        
    }
    
    public class ExplosionFireDestroy : IComponentOnce<GameState, TEntity> {

        void IPoolableRecycle.OnRecycle() {}

        void IComponent<GameState, TEntity>.AdvanceTick(GameState state, ref TEntity data, float deltaTime, int index) {

            for (int i = 0, count = state.units.Count; i < count; ++i) {

                var unit = state.units[i];
                if ((unit.position - data.position).sqrMagnitude <= data.range * data.range) {

                    if (Worlds<GameState>.currentWorld.RemoveEntity<ME.GameExample.Game.Entities.Unit>(unit.entity) == true) {

                        --i;
                        --count;

                    }

                }

            }

        }
        
        void IComponent<GameState, TEntity>.CopyFrom(IComponent<GameState, TEntity> other) {}
        
    }
    
}