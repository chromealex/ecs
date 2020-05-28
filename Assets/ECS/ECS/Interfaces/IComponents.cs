namespace ME.ECS {

    public interface IComponentBase { }

    public interface IComponentPredicate<in TComponent> where TComponent : IComponentBase {

        bool Execute(TComponent data);

    }

    public interface IComponent : IComponentBase {

    }

    public interface IComponentCopyable<TState> : IComponent, IPoolableRecycle where TState : IStateBase {

        void CopyFrom(IComponentCopyable<TState> other);
        
    }

    public interface IComponentSharedBase { }
    public interface IComponentShared : IComponent, IComponentSharedBase { }
    public interface IComponentSharedCopyable<TState> : IComponentCopyable<TState> where TState : IStateBase { }

}