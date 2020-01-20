using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {

    public interface IViewsProvider {

        void RegisterEntityType<TState, TEntity>(IViewModule<TState, TEntity> module) where TState : class, IState<TState> where TEntity : struct, IEntity;

    }

    public interface IViewsProviderBase { }

    public interface IViewsProvider<TEntity> : IViewsProviderBase where TEntity : struct, IEntity {

        IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId);
        void Destroy(ref IView<TEntity> instance);

    }

    public abstract class ViewsProvider<TEntity> : IViewsProvider<TEntity> where TEntity : struct, IEntity {
    
        public abstract IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId);
        public abstract void Destroy(ref IView<TEntity> instance);

    }

}
