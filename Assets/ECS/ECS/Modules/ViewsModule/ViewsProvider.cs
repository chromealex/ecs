using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS.Views {

    public interface IViewsProvider {

        IViewsProvider<TEntity> Create<TEntity>() where TEntity : struct, IEntity;
        void Destroy<TEntity>(IViewsProvider<TEntity> instance) where TEntity : struct, IEntity;

    }

    public interface IViewsProviderBase { }

    public interface IViewsProvider<TEntity> : IViewsProviderBase where TEntity : struct, IEntity {

        IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId);
        void Destroy(ref IView<TEntity> instance);

        void Update(System.Collections.Generic.List<IView<TEntity>> list, float deltaTime);

    }

    public abstract class ViewsProvider<TEntity> : IViewsProvider<TEntity> where TEntity : struct, IEntity {
    
        public abstract IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId);
        public abstract void Destroy(ref IView<TEntity> instance);

        public virtual void Update(System.Collections.Generic.List<IView<TEntity>> list, float deltaTime) {}

    }

}
