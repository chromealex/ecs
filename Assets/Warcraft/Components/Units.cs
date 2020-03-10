using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;

    public class UnitSelectedComponent : IComponent<TState, TEntity> {}

    public class CharacterComponent : IComponent<TState, TEntity> {}

    public class UnitPeasantComponent : IComponent<TState, TEntity> {}

    public class UnitSwordmanComponent : IComponent<TState, TEntity> {}

    public class UnitArcherComponent : IComponent<TState, TEntity> {}

    public class ForestComponent : IComponent<TState, TEntity> {}
    
    public class UnitInteractableComponent : IComponent<TState, TEntity> {}

}