namespace ME.ECS {

    public interface IComponentBase { }

    public interface IComponentPredicate<in TComponent> where TComponent : IComponentBase {

        bool Execute(TComponent data);

    }

    public interface IComponent : IComponentBase {

    }

    public interface IComponentCopyable : IComponent, IPoolableRecycle {

        void CopyFrom(IComponentCopyable other);
        
    }

    public interface IComponentSharedBase { }
    public interface IComponentShared : IComponent, IComponentSharedBase { }
    public interface IComponentSharedCopyable : IComponentCopyable { }

}