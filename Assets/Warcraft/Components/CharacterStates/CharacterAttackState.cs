using ME.ECS;

namespace Warcraft.Components.CharacterStates {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;

    public class CharacterStopState : IComponent<TState, TEntity> {}

    public class CharacterAttackState : IComponent<TState, TEntity> {}
    
}