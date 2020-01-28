namespace ME.ECS {

    public interface IComponentBase { }

    public interface IComponentOnceBase : IComponentBase { }

    public interface IComponent<TState, TEntity> : IComponentBase, IPoolableRecycle where TState : IStateBase where TEntity : IEntity {

        void AdvanceTick(TState state, ref TEntity data, float deltaTime, int index);
        void CopyFrom(IComponent<TState, TEntity> other);
        
    }

    public interface IComponentOnce<TState, TEntity> : IComponent<TState, TEntity>, IComponentOnceBase where TState : IStateBase where TEntity : IEntity {}

    public interface IComponentPredicate<in TComponent> where TComponent : IComponentBase {

        bool Execute(TComponent data);

    }

}