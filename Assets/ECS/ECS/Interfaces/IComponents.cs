namespace ME.ECS {

    public interface IComponentBase { }

    public interface IComponentOnceBase : IComponentBase { }

    public interface IComponentPredicate<in TComponent> where TComponent : IComponentBase {

        bool Execute(TComponent data);

    }

    public interface IComponent<TState, TEntity> : IComponentBase where TState : IStateBase where TEntity : IEntity {

    }

    public interface IComponentCopyable<TState, TEntity> : IComponent<TState, TEntity>, IPoolableRecycle where TState : IStateBase where TEntity : IEntity {

        void CopyFrom(IComponent<TState, TEntity> other);
        
    }

    public interface IRunnableComponent<TState, TEntity> : IComponentCopyable<TState, TEntity> where TState : IStateBase where TEntity : IEntity {

        void AdvanceTick(TState state, ref TEntity data, float deltaTime, int index);
        
    }

    public interface IComponentOnce<TState, TEntity> : IComponent<TState, TEntity>, IComponentOnceBase where TState : IStateBase where TEntity : IEntity {}

    public interface IComponentOnceCopyable<TState, TEntity> : IComponentCopyable<TState, TEntity>, IComponentOnceBase where TState : IStateBase where TEntity : IEntity {}

    public interface IRunnableComponentOnce<TState, TEntity> : IRunnableComponent<TState, TEntity>, IComponentOnceBase where TState : IStateBase where TEntity : IEntity {}

    public interface IComponentSharedBase { }
    public interface IComponentSharedOnce<TState> : IComponentOnce<TState, SharedEntity>, IComponentSharedBase where TState : IStateBase {}
    public interface IComponentShared<TState> : IComponent<TState, SharedEntity>, IComponentSharedBase where TState : IStateBase { }
    public interface IRunnableComponentSharedOnce<TState> : IRunnableComponentOnce<TState, SharedEntity>, IComponentSharedBase where TState : IStateBase {}
    public interface IRunnableComponentShared<TState> : IRunnableComponent<TState, SharedEntity>, IComponentSharedBase where TState : IStateBase { }

}